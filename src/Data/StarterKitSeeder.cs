using System;
using System.Linq;
using Odysseus.Core;
using Odysseus.Data;

namespace Odysseus.Data;

public static class StarterKitSeeder
{
	public const int InventorySize = 240;
	public const int BankSize = 400;
	public const int FashionPouchSize = 240;

	public static void SeedItemTemplates(Database db)
	{
		int idx = 0;
		void Add(ItemTemplate t) { ItemTemplateRepository.Upsert(db, t); idx++; }

		AddArmor(db, EquipSlot.Head, "Casque", idx++);
		AddArmor(db, EquipSlot.Chest, "Cuirasse", idx++);
		AddArmor(db, EquipSlot.Legs, "Jambières", idx++);
		AddArmor(db, EquipSlot.Boots, "Bottes", idx++);
		AddArmor(db, EquipSlot.Gloves, "Gantelets", idx++);
		AddShield(db, "Bouclier", idx++);
		AddWeapon(db, WeaponType.Sword1H, "Épée", idx++);
		AddWeapon(db, WeaponType.Axe1H, "Hache", idx++);
		AddWeapon(db, WeaponType.Mace1H, "Masse", idx++);
		AddWeapon(db, WeaponType.Sword2H, "Épée deux mains", idx++);
		AddWeapon(db, WeaponType.Axe2H, "Hache deux mains", idx++);
		AddWeapon(db, WeaponType.Mace2H, "Masse deux mains", idx++);
		AddWeapon(db, WeaponType.Staff, "Bâton", idx++);
		AddWeapon(db, WeaponType.Spear, "Lance", idx++);
		AddJewelry(db, EquipSlot.Ring1, "Anneau", idx++);
		AddJewelry(db, EquipSlot.Ring2, "Anneau", idx++);
		AddJewelry(db, EquipSlot.Earring1, "Boucle d'oreille", idx++);
		AddJewelry(db, EquipSlot.Earring2, "Boucle d'oreille", idx++);
		AddJewelry(db, EquipSlot.Necklace, "Collier", idx++);
		AddCape(db, "Cape", idx++);
	}

	public static void GrantToPlayer(Database db, ItemContainer container = ItemContainer.Inventory)
	{
		int slot = 0;
		foreach (var slotKind in new[] { EquipSlot.Head, EquipSlot.Chest, EquipSlot.Legs, EquipSlot.Boots, EquipSlot.Gloves, EquipSlot.Offhand })
		{
			string id = ArmorIdForSlot(slotKind);
			InventoryRepository.AddItem(db, id, (int)Quality.White, 1, 1, container, slot++);
		}
		foreach (WeaponType wt in Enum.GetValues<WeaponType>().Cast<WeaponType>())
		{
			string id = WeaponId(wt);
			InventoryRepository.AddItem(db, id, (int)Quality.White, 1, 1, container, slot++);
		}
		foreach (var jslot in new[] { EquipSlot.Ring1, EquipSlot.Ring2, EquipSlot.Earring1, EquipSlot.Earring2, EquipSlot.Necklace, EquipSlot.Cape })
		{
			string id = JewelryIdForSlot(jslot);
			InventoryRepository.AddItem(db, id, (int)Quality.White, 1, 1, container, slot++);
		}
	}

	private static string ArmorIdForSlot(EquipSlot s) => s switch
	{
		EquipSlot.Head => "starter_head",
		EquipSlot.Chest => "starter_chest",
		EquipSlot.Legs => "starter_legs",
		EquipSlot.Boots => "starter_boots",
		EquipSlot.Gloves => "starter_gloves",
		EquipSlot.Offhand => "starter_shield",
		_ => throw new ArgumentException(s.ToString()),
	};

	private static string WeaponId(WeaponType wt) => $"starter_{wt.ToString().ToLowerInvariant()}";
	private static string JewelryIdForSlot(EquipSlot s) => $"starter_{s.ToString().ToLowerInvariant()}";

	private static void AddArmor(Database db, EquipSlot slot, string name, int _)
	{
		ItemTemplateRepository.Upsert(db, new ItemTemplate
		{
			Id = ArmorIdForSlot(slot),
			Name = $"{name} de Novice",
			Kind = "Armor",
			EquipSlot = slot.ToString(),
			Quality = (int)Quality.White,
			Rank = 1,
			DefenseBonus = 1,
			Sellable = false,
			Stackable = false,
			BaseStats = new BaseAttributes { Constitution = 1 },
		});
	}

	private static void AddShield(Database db, string name, int _)
	{
		ItemTemplateRepository.Upsert(db, new ItemTemplate
		{
			Id = "starter_shield",
			Name = $"{name} de Novice",
			Kind = "Shield",
			EquipSlot = EquipSlot.Offhand.ToString(),
			Quality = (int)Quality.White,
			Rank = 1,
			DefenseBonus = 2,
			Sellable = false,
			Stackable = false,
			BaseStats = new BaseAttributes { Constitution = 1 },
		});
	}

	private static void AddWeapon(Database db, WeaponType wt, string name, int _)
	{
		bool is1H = wt.ToString().EndsWith("1H");
		double speed = is1H ? 1.5 : 3.8;
		int atk = is1H ? 3 : 8;
		ItemTemplateRepository.Upsert(db, new ItemTemplate
		{
			Id = WeaponId(wt),
			Name = $"{name} de Novice",
			Kind = "Weapon",
			EquipSlot = EquipSlot.Weapon.ToString(),
			WeaponType = wt.ToString(),
			AttackSpeed = speed,
			Quality = (int)Quality.White,
			Rank = 1,
			AttackBonus = atk,
			Sellable = false,
			Stackable = false,
			BaseStats = new BaseAttributes { Force = is1H ? 1 : 2 },
		});
	}

	private static void AddJewelry(Database db, EquipSlot slot, string name, int _)
	{
		ItemTemplateRepository.Upsert(db, new ItemTemplate
		{
			Id = JewelryIdForSlot(slot),
			Name = $"{name} de Novice",
			Kind = "Jewelry",
			EquipSlot = slot.ToString(),
			Quality = (int)Quality.White,
			Rank = 1,
			Sellable = false,
			Stackable = false,
			BaseStats = new BaseAttributes { Spirit = 1 },
		});
	}

	private static void AddCape(Database db, string name, int _)
	{
		ItemTemplateRepository.Upsert(db, new ItemTemplate
		{
			Id = JewelryIdForSlot(EquipSlot.Cape),
			Name = $"{name} de Novice",
			Kind = "Cape",
			EquipSlot = EquipSlot.Cape.ToString(),
			Quality = (int)Quality.White,
			Rank = 1,
			Sellable = false,
			Stackable = false,
			BaseStats = new BaseAttributes { Dexterity = 1 },
		});
	}
}