using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using Odysseus.Core;

namespace Odysseus.Data;

public enum ItemContainer
{
	Inventory,
	Bank,
	Fashion
}

public sealed class InventoryEntry
{
	public long Id { get; set; }
	public string ItemId { get; set; } = "";
	public int Quality { get; set; }
	public int Rank { get; set; } = 1;
	public int SlotIndex { get; set; }
	public int Qty { get; set; } = 1;
	public List<string> Runes { get; set; } = new();
	public ItemContainer Container { get; set; }
}

public sealed class ItemTemplate
{
	public string Id { get; set; } = "";
	public string Name { get; set; } = "";
	public string Kind { get; set; } = "";
	public string? EquipSlot { get; set; }
	public string? WeaponType { get; set; }
	public double? AttackSpeed { get; set; }
	public int Quality { get; set; }
	public int Rank { get; set; } = 1;
	public int AttackBonus { get; set; }
	public int DefenseBonus { get; set; }
	public bool Sellable { get; set; } = true;
	public bool Stackable { get; set; }
	public string? IconPath { get; set; }
	public string Description { get; set; } = "";
	public BaseAttributes BaseStats { get; set; } = BaseAttributes.Zero;
}

public static class ItemTemplateRepository
{
	public static void Upsert(Database db, ItemTemplate t)
	{
		db.Execute(@"
			INSERT INTO item_template (id, name, kind, equip_slot, weapon_type, attack_speed,
			                           quality, rank, base_stats, attack_bonus, defense_bonus,
			                           sellable, stackable, icon_path, description)
			VALUES (@id, @name, @kind, @slot, @wt, @as, @q, @r, @bs, @ab, @db, @sell, @stk, @icon, @desc)
			ON CONFLICT(id) DO UPDATE SET
				name=excluded.name, kind=excluded.kind, equip_slot=excluded.equip_slot,
				weapon_type=excluded.weapon_type, attack_speed=excluded.attack_speed,
				quality=excluded.quality, rank=excluded.rank, base_stats=excluded.base_stats,
				attack_bonus=excluded.attack_bonus, defense_bonus=excluded.defense_bonus,
				sellable=excluded.sellable, stackable=excluded.stackable,
				icon_path=excluded.icon_path, description=excluded.description;",
			("id", t.Id),
			("name", t.Name),
			("kind", t.Kind),
			("slot", (object?)t.EquipSlot ?? DBNull.Value),
			("wt", (object?)t.WeaponType ?? DBNull.Value),
			("as", (object?)t.AttackSpeed ?? DBNull.Value),
			("q", t.Quality),
			("r", t.Rank),
			("bs", PlayerRepository.SerializeBaseAttributes(t.BaseStats)),
			("ab", t.AttackBonus),
			("db", t.DefenseBonus),
			("sell", t.Sellable ? 1 : 0),
			("stk", t.Stackable ? 1 : 0),
			("icon", (object?)t.IconPath ?? DBNull.Value),
			("desc", t.Description ?? ""));
	}

	public static ItemTemplate? Load(Database db, string id)
	{
		using var cmd = db.CreateCommand();
		cmd.CommandText = "SELECT id, name, kind, equip_slot, weapon_type, attack_speed, quality, rank, base_stats, attack_bonus, defense_bonus, sellable, stackable, icon_path, description FROM item_template WHERE id = @id;";
		cmd.Parameters.AddWithValue("@id", id);
		using var r = cmd.ExecuteReader();
		if (!r.Read()) return null;
		return ReadTemplate(r);
	}

	private static ItemTemplate ReadTemplate(System.Data.Common.DbDataReader r)
	{
		return new ItemTemplate
		{
			Id = r.GetString(0),
			Name = r.GetString(1),
			Kind = r.GetString(2),
			EquipSlot = r.IsDBNull(3) ? null : r.GetString(3),
			WeaponType = r.IsDBNull(4) ? null : r.GetString(4),
			AttackSpeed = r.IsDBNull(5) ? null : r.GetDouble(5),
			Quality = r.GetInt32(6),
			Rank = r.GetInt32(7),
			BaseStats = PlayerRepository.DeserializeBaseAttributes(r.GetString(8)),
			AttackBonus = r.GetInt32(9),
			DefenseBonus = r.GetInt32(10),
			Sellable = r.GetInt32(11) != 0,
			Stackable = r.GetInt32(12) != 0,
			IconPath = r.IsDBNull(13) ? null : r.GetString(13),
			Description = r.GetString(14),
		};
	}

	public static int Count(Database db)
	{
		return (int)(long)db.Scalar("SELECT COUNT(*) FROM item_template;");
	}
}