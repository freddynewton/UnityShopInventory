using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using Azulon.Data;
using Azulon.Services;
using System;

namespace Azulon.UI
{
	public class ShopController : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private GameObject shopPanel;
		[SerializeField] private ScrollRect shopScrollRect;
		[SerializeField] private Transform shopItemsContainer;
		[SerializeField] private GameObject shopItemPrefab;
		[SerializeField] private TextMeshProUGUI currencyText;

		[Header("Shop Preview")]
		[SerializeField] private ItemPreviewUI itemPreviewUI;

		private ItemData _selectedItem;
		private List<ItemData> _loadedShopItems = new List<ItemData>();

		[Inject] private IItemService _itemService;
		[Inject] private DiContainer _diContainer;

		private readonly List<ShopItemUI> _shopItemUIs = new List<ShopItemUI>();

		private string _activeCategoryFilter = null;

		public void OnItemSelected(ItemData itemData)
		{
			_selectedItem = itemData;

			// Update selection state of all shop item UIs
			foreach (var itemUI in _shopItemUIs)
			{
				itemUI.SetSelected(itemUI.ItemData.Id == _selectedItem.Id);
			}

			if (_selectedItem != null)
			{
				itemPreviewUI.ShowPreview(_selectedItem);
				// Set up custom purchase and sell actions for the shop context
				itemPreviewUI.Setup(_itemService, OnPreviewPurchase, OnPreviewSell);
			}
			else
			{
				itemPreviewUI.HidePreview();
			}
		}

		public void OpenShop()
		{
			if (shopPanel != null)
			{
				shopPanel.SetActive(true);
			}

			UpdateCurrencyDisplay(_itemService.Currency);

			// Update all shop item UIs
			foreach (var itemUI in _shopItemUIs)
			{
				itemUI.UpdateAffordability();
			}
		}

		public void CloseShop()
		{
			if (shopPanel != null)
			{
				shopPanel.SetActive(false);
			}
		}

		public void ToggleShop()
		{
			if (shopPanel != null)
			{
				bool isActive = shopPanel.activeSelf;
				if (isActive)
				{
					CloseShop();
				}
				else
				{
					OpenShop();
				}
			}
		}

		private void Start()
		{
			InitializeShop();
			SetupEventListeners();
			UpdateCurrencyDisplay(_itemService.Currency);
		}

		private void OnDestroy()
		{
			RemoveEventListeners();
		}

		private void InitializeShop()
		{
			LoadShopItemsFromResources();
			_itemService.SetupShop(_loadedShopItems);
			CreateShopItemUIs();
			itemPreviewUI.HidePreview();
		}

		private void LoadShopItemsFromResources()
		{
			_loadedShopItems.Clear();

			// Load all ItemData from Resources folder
			ItemDataSO[] itemDataSOs = Resources.LoadAll<ItemDataSO>("Items");

			foreach (var itemSO in itemDataSOs)
			{
				if (itemSO != null && itemSO.ItemData != null && itemSO.ItemData.IsValid())
				{
					_loadedShopItems.Add(itemSO.ItemData);
				}
			}

			Debug.Log($"ShopController: Loaded {_loadedShopItems.Count} items from Resources");
		}

		private void SetupEventListeners()
		{
			_itemService.OnCurrencyChanged += UpdateCurrencyDisplay;
			_itemService.OnItemPurchased += OnItemPurchased;
		}

		private void RemoveEventListeners()
		{
			_itemService.OnCurrencyChanged -= UpdateCurrencyDisplay;
			_itemService.OnItemPurchased -= OnItemPurchased;
		}

		private void CreateShopItemUIs()
		{
			// Clear existing items
			foreach (var itemUI in _shopItemUIs)
			{
				if (itemUI != null)
				{
					Destroy(itemUI.gameObject);
				}
			}

			_shopItemUIs.Clear();

			// Create new shop item UIs
			var shopItems = _itemService.GetShopItems();
			foreach (var itemSO in shopItems)
			{
				CreateShopItemUI(itemSO);
			}

			// After creating all items, ensure proper content sizing for scrolling
			EnsureScrollableContent();
		}

		private void EnsureScrollableContent()
		{
			if (shopScrollRect != null && shopScrollRect.content != null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(shopScrollRect.content);
				shopScrollRect.verticalNormalizedPosition = 1f; // Reset scroll position to top
			}
			else
			{
				Debug.LogWarning("ShopController: shopScrollRect or its content is not set up correctly!");
			}
		}

		private void CreateShopItemUI(ItemData itemSO)
		{
			if (shopItemPrefab == null || shopItemsContainer == null || itemSO == null)
			{
				return;
			}

			ShopItemUI itemUI = _diContainer.InstantiatePrefabForComponent<ShopItemUI>(shopItemPrefab, shopItemsContainer);

			if (itemUI == null)
			{
				Debug.LogError("ShopController: ShopItemUI component not found on prefab!");
				return;
			}

			itemUI.Initialize(itemSO, _itemService, this);
			_shopItemUIs.Add(itemUI);
		}

		private void UpdateCurrencyDisplay(int newCurrency)
		{
			currencyText.text = $"Gold: {newCurrency}";
		}

		private void OnItemPurchased(ItemData purchasedItem)
		{
			Debug.Log($"Purchased: {purchasedItem.Name} x{purchasedItem.Quantity}");

			// Update all shop item UIs to reflect current affordability
			foreach (var itemUI in _shopItemUIs)
			{
				itemUI.UpdateAffordability();
			}

			// Update preview if the purchased item is currently selected
			if (_selectedItem != null)
			{
				itemPreviewUI.UpdateAffordability();
			}
		}

		private void OnPreviewPurchase(ItemData itemData)
		{
			if (itemData == null)
			{
				return;
			}

			bool success = _itemService.PurchaseItem(itemData, 1);
			if (success)
			{
				Debug.Log($"ShopController: Successfully purchased {itemData.Name}");
			}
			else
			{
				Debug.LogWarning($"ShopController: Failed to purchase {itemData.Name}");
			}
		}

		private void OnPreviewSell(ItemData itemData)
		{
			if (itemData == null)
			{
				return;
			}

			if (_itemService.RemoveItem(itemData.Id, 1))
			{
				_itemService.AddCurrency(itemData.Price);
				Debug.Log($"ShopController: Successfully sold {itemData.Name}");

				// If no more items of this type, hide the preview
				if (_itemService.GetItemQuantity(itemData.Id) <= 0)
				{
					itemPreviewUI.HidePreview();
				}
			}
			else
			{
				Debug.LogWarning($"ShopController: Failed to sell {itemData.Name}");
			}
		}

		public void FilterByCategory(string category)
		{
			_activeCategoryFilter = category;
			UpdateShopItemsByCategory();
		}

		private void UpdateShopItemsByCategory()
		{
			// Remove existing shop item UIs
			foreach (var itemUI in _shopItemUIs)
			{
				if (itemUI != null)
				{
					Destroy(itemUI.gameObject);
				}
			}
			_shopItemUIs.Clear();

			// Filter items
			List<ItemData> filteredItems;
			if (string.IsNullOrEmpty(_activeCategoryFilter) || _activeCategoryFilter == "All")
			{
				filteredItems = new List<ItemData>(_loadedShopItems);
			}
			else
			{
				filteredItems = _loadedShopItems.FindAll(item => item.ItemType.ToString() == _activeCategoryFilter);
			}

			foreach (var itemSO in filteredItems)
			{
				CreateShopItemUI(itemSO);
			}
			EnsureScrollableContent();
		}
	}
}
