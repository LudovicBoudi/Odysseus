using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Odysseus.Core;

namespace Odysseus.Data;

public static class InventoryRepository
{
	public static long AddItem(Database db, string itemId, int quality, int rank, int qty, ItemContainer container, int slotIndex, string runesJson = "[]")
	{
		db.Execute(@"
			INSERT INTO inventory_item (item_id, quality, rank, slot_index, qty, runes, container)
			VALUES (@iid, @q, @r, @s, @qty, @runes, @c);",
			("iid", itemId),
			("q", quality),
			("r", rank),
			("s", slotIndex),
			("qty", qty),
			("runes", runesJson),
			("c", container.ToString()));

		using var cmd = db.CreateCommand();
		cmd.CommandText = "SELECT last_insert_rowid();";
		return (long)cmd.ExecuteScalar()!;
	}

	public static List<InventoryEntry> LoadAll(Database db, ItemContainer container)
	{
		var list = new List<InventoryEntry>();
		using var cmd = db.CreateCommand();
		cmd.CommandText = "SELECT id, item_id, quality, rank, slot_index, qty, runes, container FROM inventory_item WHERE container = @c ORDER BY slot_index;";
		cmd.Parameters.AddWithValue("@c", container.ToString());
		using var r = cmd.ExecuteReader();
		while (r.Read())
		{
			list.Add(new InventoryEntry
			{
				Id = r.GetInt64(0),
				ItemId = r.GetString(1),
				Quality = r.GetInt32(2),
				Rank = r.GetInt32(3),
				SlotIndex = r.GetInt32(4),
				Qty = r.GetInt32(5),
				Runes = JsonSerializer.Deserialize<List<string>>(r.GetString(6)) ?? new(),
				Container = Enum.Parse<ItemContainer>(r.GetString(7)),
			});
		}
		return list;
	}

	public static void Remove(Database db, long entryId)
	{
		db.Execute("DELETE FROM inventory_item WHERE id = @id;", ("id", entryId));
	}

	public static void UpdateQty(Database db, long entryId, int qty)
	{
		if (qty <= 0)
		{
			Remove(db, entryId);
			return;
		}
		db.Execute("UPDATE inventory_item SET qty = @q WHERE id = @id;", ("q", qty), ("id", entryId));
	}

	public static void Equip(Database db, EquipSlot slot, long? inventoryItemId, long? fashionItemId = null)
	{
		db.Execute(@"
			INSERT INTO equipment_slot (slot, inventory_item_id, fashion_item_id)
			VALUES (@slot, @iid, @fid)
			ON CONFLICT(slot) DO UPDATE SET
				inventory_item_id = excluded.inventory_item_id,
				fashion_item_id = excluded.fashion_item_id;",
			("slot", slot.ToString()),
			("iid", (object?)inventoryItemId ?? DBNull.Value),
			("fid", (object?)fashionItemId ?? DBNull.Value));
	}

	public static Dictionary<EquipSlot, (long? ItemId, long? FashionId)> LoadEquip(Database db)
	{
		var dict = new Dictionary<EquipSlot, (long?, long?)>();
		using var cmd = db.CreateCommand();
		cmd.CommandText = "SELECT slot, inventory_item_id, fashion_item_id FROM equipment_slot;";
		using var r = cmd.ExecuteReader();
		while (r.Read())
		{
			var slot = Enum.Parse<EquipSlot>(r.GetString(0));
			long? iid = r.IsDBNull(1) ? null : r.GetInt64(1);
			long? fid = r.IsDBNull(2) ? null : r.GetInt64(2);
			dict[slot] = (iid, fid);
		}
		return dict;
	}
}