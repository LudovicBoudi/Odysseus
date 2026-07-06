using System;
using System.IO;
using System.Linq;
using Xunit;
using Odysseus.Data;
using Odysseus.Core;

namespace Odysseus.Tests;

public class InventoryAndStarterKitTests : IDisposable
{
	private readonly string _tmp;
	private readonly Database _db;
	private const string Schema = """
		PRAGMA foreign_keys = ON;
		CREATE TABLE IF NOT EXISTS meta (key TEXT PRIMARY KEY, value TEXT NOT NULL);
		CREATE TABLE IF NOT EXISTS item_template (
			id TEXT PRIMARY KEY, name TEXT NOT NULL, kind TEXT NOT NULL,
			equip_slot TEXT, weapon_type TEXT, attack_speed REAL,
			quality INTEGER NOT NULL DEFAULT 0, rank INTEGER NOT NULL DEFAULT 1,
			base_stats TEXT NOT NULL DEFAULT '{}',
			attack_bonus INTEGER NOT NULL DEFAULT 0, defense_bonus INTEGER NOT NULL DEFAULT 0,
			sellable INTEGER NOT NULL DEFAULT 1, stackable INTEGER NOT NULL DEFAULT 1,
			icon_path TEXT, description TEXT NOT NULL DEFAULT ''
		);
		CREATE TABLE IF NOT EXISTS inventory_item (
			id INTEGER PRIMARY KEY AUTOINCREMENT,
			item_id TEXT NOT NULL, quality INTEGER NOT NULL DEFAULT 0,
			rank INTEGER NOT NULL DEFAULT 1, slot_index INTEGER NOT NULL DEFAULT 0,
			qty INTEGER NOT NULL DEFAULT 1, runes TEXT NOT NULL DEFAULT '[]',
			container TEXT NOT NULL DEFAULT 'Inventory'
		);
		CREATE TABLE IF NOT EXISTS equipment_slot (
			slot TEXT PRIMARY KEY, inventory_item_id INTEGER, fashion_item_id INTEGER
		);
		CREATE TABLE IF NOT EXISTS player (
			id INTEGER PRIMARY KEY CHECK (id = 1), name TEXT NOT NULL,
			gender TEXT NOT NULL, divine_affiliation TEXT NOT NULL,
			creation_seed TEXT NOT NULL DEFAULT '', level INTEGER NOT NULL DEFAULT 1,
			prestige INTEGER NOT NULL DEFAULT 0, xp INTEGER NOT NULL DEFAULT 0,
			gold INTEGER NOT NULL DEFAULT 0, gem INTEGER NOT NULL DEFAULT 0,
			base_attributes TEXT NOT NULL DEFAULT '{}',
			unspent_base_pts INTEGER NOT NULL DEFAULT 0,
			unspent_secondary_pts INTEGER NOT NULL DEFAULT 0,
			current_map_id TEXT, created_at TEXT NOT NULL, updated_at TEXT NOT NULL
		);
		CREATE TABLE IF NOT EXISTS player_vitals (
			player_id INTEGER PRIMARY KEY REFERENCES player(id) ON DELETE CASCADE,
			hp_current INTEGER NOT NULL DEFAULT 200, mana_current INTEGER NOT NULL DEFAULT 100
		);
	""";

	public InventoryAndStarterKitTests()
	{
		_tmp = Path.Combine(Path.GetTempPath(), $"ody_inv_{Guid.NewGuid():N}.sqlite");
		_db = Database.Open(_tmp, Schema);
	}

	public void Dispose()
	{
		_db.Dispose();
		try { File.Delete(_tmp); } catch { }
	}

	[Fact]
	public void SeedItemTemplates_CreatesAllStarterItems()
	{
		StarterKitSeeder.SeedItemTemplates(_db);
		int count = ItemTemplateRepository.Count(_db);
		Assert.Equal(20, count);
	}

	[Fact]
	public void GrantToPlayer_Adds20Items_ToInventory()
	{
		StarterKitSeeder.SeedItemTemplates(_db);
		StarterKitSeeder.GrantToPlayer(_db, ItemContainer.Inventory);
		var entries = InventoryRepository.LoadAll(_db, ItemContainer.Inventory);
		Assert.Equal(20, entries.Count);
	}

	[Fact]
	public void StarterItems_AreAllWhiteQualityRank1()
	{
		StarterKitSeeder.SeedItemTemplates(_db);
		StarterKitSeeder.GrantToPlayer(_db, ItemContainer.Inventory);
		var entries = InventoryRepository.LoadAll(_db, ItemContainer.Inventory);
		foreach (var e in entries)
		{
			Assert.Equal((int)Quality.White, e.Quality);
			Assert.Equal(1, e.Rank);
		}
	}

	[Fact]
	public void StarterKit_Includes_All8Weapons_AndShield_And5Armor_And6JewelryCape()
	{
		StarterKitSeeder.SeedItemTemplates(_db);
		StarterKitSeeder.GrantToPlayer(_db, ItemContainer.Inventory);
		var entries = InventoryRepository.LoadAll(_db, ItemContainer.Inventory);

		Assert.NotNull(ItemTemplateRepository.Load(_db, "starter_sword1h"));
		Assert.NotNull(ItemTemplateRepository.Load(_db, "starter_axe1h"));
		Assert.NotNull(ItemTemplateRepository.Load(_db, "starter_mace1h"));
		Assert.NotNull(ItemTemplateRepository.Load(_db, "starter_sword2h"));
		Assert.NotNull(ItemTemplateRepository.Load(_db, "starter_axe2h"));
		Assert.NotNull(ItemTemplateRepository.Load(_db, "starter_mace2h"));
		Assert.NotNull(ItemTemplateRepository.Load(_db, "starter_staff"));
		Assert.NotNull(ItemTemplateRepository.Load(_db, "starter_spear"));
		Assert.NotNull(ItemTemplateRepository.Load(_db, "starter_shield"));
		foreach (var s in new[] { "head", "chest", "legs", "boots", "gloves", "ring1", "ring2", "earring1", "earring2", "necklace", "cape" })
			Assert.NotNull(ItemTemplateRepository.Load(_db, $"starter_{s}"));
	}

	[Fact]
	public void WeaponAttackSpeed_1H_Is1_5s_And2H_Is3_8s()
	{
		StarterKitSeeder.SeedItemTemplates(_db);
		Assert.Equal(1.5, ItemTemplateRepository.Load(_db, "starter_sword1h")!.AttackSpeed);
		Assert.Equal(3.8, ItemTemplateRepository.Load(_db, "starter_sword2h")!.AttackSpeed);
		Assert.Equal(3.8, ItemTemplateRepository.Load(_db, "starter_staff")!.AttackSpeed);
		Assert.Equal(1.5, ItemTemplateRepository.Load(_db, "starter_axe1h")!.AttackSpeed);
	}

	[Fact]
	public void StarterItems_AreNotSellable()
	{
		StarterKitSeeder.SeedItemTemplates(_db);
		var all = new[] { "starter_head", "starter_sword1h", "starter_ring1", "starter_cape" };
		foreach (var id in all)
			Assert.False(ItemTemplateRepository.Load(_db, id)!.Sellable);
	}

	[Fact]
	public void ItemTemplate_TwoHandedWeapon_HasMoreAttackThanOneHanded()
	{
		StarterKitSeeder.SeedItemTemplates(_db);
		var sword1h = ItemTemplateRepository.Load(_db, "starter_sword1h")!;
		var sword2h = ItemTemplateRepository.Load(_db, "starter_sword2h")!;
		Assert.True(sword2h.AttackBonus > sword1h.AttackBonus);
	}

	[Fact]
	public void Equip_AndLoadEquipment_RoundTrips()
	{
		StarterKitSeeder.SeedItemTemplates(_db);
		StarterKitSeeder.GrantToPlayer(_db, ItemContainer.Inventory);
		var entry = InventoryRepository.LoadAll(_db, ItemContainer.Inventory).First();
		InventoryRepository.Equip(_db, EquipSlot.Weapon, entry.Id);
		var equip = InventoryRepository.LoadEquip(_db);
		Assert.True(equip.ContainsKey(EquipSlot.Weapon));
		Assert.Equal(entry.Id, equip[EquipSlot.Weapon].ItemId);
	}

	[Fact]
	public void UpdateQty_ZeroRemovesEntry()
	{
		StarterKitSeeder.SeedItemTemplates(_db);
		long id = InventoryRepository.AddItem(_db, "starter_head", 0, 1, 5, ItemContainer.Inventory, 0);
		InventoryRepository.UpdateQty(_db, id, 0);
		var entries = InventoryRepository.LoadAll(_db, ItemContainer.Inventory);
		Assert.Empty(entries);
	}

	[Fact]
	public void Container_BankAndFashion_AreSeparate()
	{
		StarterKitSeeder.SeedItemTemplates(_db);
		InventoryRepository.AddItem(_db, "starter_head", 0, 1, 1, ItemContainer.Inventory, 0);
		InventoryRepository.AddItem(_db, "starter_head", 0, 1, 1, ItemContainer.Bank, 0);
		InventoryRepository.AddItem(_db, "starter_head", 0, 1, 1, ItemContainer.Fashion, 0);
		Assert.Equal(1, InventoryRepository.LoadAll(_db, ItemContainer.Inventory).Count);
		Assert.Equal(1, InventoryRepository.LoadAll(_db, ItemContainer.Bank).Count);
		Assert.Equal(1, InventoryRepository.LoadAll(_db, ItemContainer.Fashion).Count);
	}
}