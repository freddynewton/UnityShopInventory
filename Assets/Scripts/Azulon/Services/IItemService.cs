using System;
using System.Collections.Generic;
using Azulon.Data;

namespace Azulon.Services
{
	public interface IItemService
	{
		// Inventory Management
		event Action<ItemData> OnItemAdded;
		event Action<ItemData> OnItemRemoved;
		event Action OnInventoryChanged;

		// Shop Management
		event Action<ItemData> OnItemPurchased;
		event Action<int> OnCurrencyChanged;

		// Currency
		int Currency { get; }
		bool SpendCurrency(int amount);
		void AddCurrency(int amount);

		// Inventory Operations
		IReadOnlyList<ItemData> GetInventoryItems();
		bool HasItem(string itemId);
		int GetItemQuantity(string itemId);
		bool AddItem(ItemData item);
		bool RemoveItem(string itemId, int quantity = 1);
		void ClearInventory();

		// Shop Operations
		IReadOnlyList<ItemData> GetShopItems();
		bool CanPurchaseItem(ItemData item);
		bool PurchaseItem(ItemData item, int quantity = 1);
		void SetupShop(List<ItemData> shopItems);

		// Data Persistence
		void SaveData();
		void LoadData();
	}
}
