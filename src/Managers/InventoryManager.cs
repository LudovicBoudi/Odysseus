using System.Collections.Generic;
using System.Linq;
using Godot;
using Odysseus.Core;
using Odysseus.Data;

namespace Odysseus.Managers;

public sealed partial class InventoryManager : Node
{
	public static InventoryManager Instance { get; private set; } = null!;

	public const int InventorySize = StarterKitSeeder.InventorySize;
	public const int BankSize = StarterKitSeeder.BankSize;
	public const int FashionPouchSize = StarterKitSeeder.FashionPouchSize;

	private readonly Dictionary<ItemContainer, InventoryEntry?[]> _containers = new()
	{
		[ItemContainer.Inventory] = new InventoryEntry?[InventorySize],
		[ItemContainer.Bank] = new InventoryEntry?[BankSize],
		[ItemContainer.Fashion] = new InventoryEntry?[FashionPouchSize],
	};

	private Database? _db;
	private Database Db => _db ?? SaveManager.Instance.Db;

	public override void _EnterTree() => Instance = this;

	public override void _Ready()
	{
		_db = SaveManager.Instance.Db;
		foreach (var c in new[] { ItemContainer.Inventory, ItemContainer.Bank, ItemContainer.Fashion })
		{
			var entries = InventoryRepository.LoadAll(Db, c);
			var arr = _containers[c];
			foreach (var e in entries)
			{
				if (e.SlotIndex >= 0 && e.SlotIndex < arr.Length)
					arr[e.SlotIndex] = e;
			}
		}
		GD.Print($"[Inventory] Loaded: {Count(ItemContainer.Inventory)}/{InventorySize} inv, {Count(ItemContainer.Bank)}/{BankSize} bank, {Count(ItemContainer.Fashion)}/{FashionPouchSize} fashion.");
	}

	public InventoryEntry? GetAt(ItemContainer container, int slot) =>
		_containers[container][slot];

	public bool TryAdd(string itemId, int quality, int rank, int qty, ItemContainer container)
	{
		var arr = _containers[container];
		for (int i = 0; i < arr.Length; i++)
		{
			if (arr[i] == null)
			{
				long id = InventoryRepository.AddItem(Db, itemId, quality, rank, qty, container, i);
				arr[i] = new InventoryEntry { Id = id, ItemId = itemId, Quality = quality, Rank = rank, SlotIndex = i, Qty = qty, Container = container };
				return true;
			}
		}
		return false;
	}

	public void Remove(ItemContainer container, int slot)
	{
		var arr = _containers[container];
		if (slot < 0 || slot >= arr.Length) return;
		if (arr[slot] == null) return;
		InventoryRepository.Remove(Db, arr[slot]!.Id);
		arr[slot] = null;
	}

	public int Count(ItemContainer container) => _containers[container].Count(e => e != null);

	public void GrantStarterKit()
	{
		if (Count(ItemContainer.Inventory) > 0) return;
		StarterKitSeeder.SeedItemTemplates(Db);
		StarterKitSeeder.GrantToPlayer(Db, ItemContainer.Inventory);
		for (int i = 0; i < InventorySize; i++)
		{
			if (_containers[ItemContainer.Inventory][i] == null)
			{
				_containers[ItemContainer.Inventory][i] = InventoryRepository.LoadAll(Db, ItemContainer.Inventory)
					.FirstOrDefault(e => e.SlotIndex == i);
			}
		}
		GD.Print("[Inventory] Starter kit granted.");
	}
}