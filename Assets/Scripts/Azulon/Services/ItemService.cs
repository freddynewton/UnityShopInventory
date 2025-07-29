using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Azulon.Data;

namespace Azulon.Services
{
	public class ItemService : IItemService
	{
		// Events
		public event Action<ItemData> OnItemAdded;
		public event Action<ItemData> OnItemRemoved;
		public event Action OnInventoryChanged;
		public event Action<ItemData> OnItemPurchased;
		public event Action<int> OnCurrencyChanged;

		// Private fields
		private readonly List<ItemData> _inventory = new List<ItemData>();
		private readonly List<ItemDataSO> _shopItems = new List<ItemDataSO>();
		private int _currency = 100; // Starting currency

		// Properties
		public int Currency => _currency;

		// Currency Management
		public bool SpendCurrency(int amount)
		{
			if (amount < 0 || _currency < amount)
			{
				return false;
			}

			_currency -= amount;
			OnCurrencyChanged?.Invoke(_currency);
			return true;
		}

		public void AddCurrency(int amount)
		{
			if (amount < 0)
			{
				return;
			}

			_currency += amount;
			OnCurrencyChanged?.Invoke(_currency);
		}

		// Inventory Operations
		public IReadOnlyList<ItemData> GetInventoryItems()
		{
			return _inventory.AsReadOnly();
		}

		public bool HasItem(string itemId)
		{
			return _inventory.Any(item => item.Id == itemId);
		}

		public int GetItemQuantity(string itemId)
		{
			var item = _inventory.FirstOrDefault(item => item.Id == itemId);
			return item?.Quantity ?? 0;
		}

		public bool AddItem(ItemData item)
		{
			if (item == null || !item.IsValid())
				return false;

			var existingItem = _inventory.FirstOrDefault(i => i.Id == item.Id);

			if (existingItem != null)
			{
				existingItem.Quantity += item.Quantity;
			}
			else
			{
				_inventory.Add(new ItemData(item));
			}

			OnItemAdded?.Invoke(item);
			OnInventoryChanged?.Invoke();
			return true;
		}

		public bool RemoveItem(string itemId, int quantity = 1)
		{
			var item = _inventory.FirstOrDefault(i => i.Id == itemId);

			if (item == null || item.Quantity < quantity)
				return false;

			item.Quantity -= quantity;

			if (item.Quantity <= 0)
			{
				_inventory.Remove(item);
			}

			OnItemRemoved?.Invoke(item);
			OnInventoryChanged?.Invoke();
			return true;
		}

		public void ClearInventory()
		{
			_inventory.Clear();
			OnInventoryChanged?.Invoke();
		}

		// Shop Operations
		public IReadOnlyList<ItemDataSO> GetShopItems()
		{
			return _shopItems.AsReadOnly();
		}

		public bool CanPurchaseItem(ItemDataSO itemSO)
		{
			if (itemSO == null || itemSO.ItemData == null)
				return false;

			return _currency >= itemSO.ItemData.Price;
		}

		public bool PurchaseItem(ItemDataSO itemSO, int quantity = 1)
		{
			if (itemSO == null || itemSO.ItemData == null || quantity <= 0)
				return false;

			int totalCost = itemSO.ItemData.Price * quantity;

			if (!SpendCurrency(totalCost))
				return false;

			var purchasedItem = itemSO.CreateRuntimeItemData(quantity);
			AddItem(purchasedItem);

			OnItemPurchased?.Invoke(purchasedItem);
			return true;
		}

		public void SetupShop(List<ItemDataSO> shopItems)
		{
			_shopItems.Clear();
			if (shopItems != null)
			{
				_shopItems.AddRange(shopItems.Where(item => item != null));
			}
		}

		// Data Persistence
		public void SaveData()
		{
			var saveData = new SaveData(_currency, _inventory.ToList());

			bool success = JSONController.SaveData(saveData, "ItemServiceData");

			if (success)
			{
				Debug.Log("ItemService: Data saved successfully.");
			}
			else
			{
				Debug.LogError("ItemService: Failed to save data.");
			}
		}

		public void LoadData()
		{
			var defaultSaveData = new SaveData();
			var saveData = JSONController.LoadData("ItemServiceData", defaultSaveData);

			if (saveData != null && saveData.IsValid())
			{
				_currency = saveData.currency;
				_inventory.Clear();
				_inventory.AddRange(saveData.inventoryItems);

				OnCurrencyChanged?.Invoke(_currency);
				OnInventoryChanged?.Invoke();

				Debug.Log("ItemService: Data loaded successfully.");
			}
			else
			{
				Debug.LogWarning("ItemService: Invalid save data found, using default values.");

				// Set default values
				_currency = 100;
				_inventory.Clear();

				OnCurrencyChanged?.Invoke(_currency);
				OnInventoryChanged?.Invoke();
			}
		}
	}
}
