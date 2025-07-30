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
		[SerializeField] private GameObject shopItemPreview;
		[SerializeField] private Image previewItemIcon;
		[SerializeField] private TextMeshProUGUI previewItemName;
		[SerializeField] private TextMeshProUGUI previewItemPrice;
		[SerializeField] private ScrollRect previewDescriptionScrollRect;
		[SerializeField] private TextMeshProUGUI previewItemDescription;
		[SerializeField] private Button previewPurchaseButton;
		[SerializeField] private TextMeshProUGUI previewPurchaseButtonText;

		private ItemDataSO _selectedItemSO;
		private List<ItemDataSO> _loadedShopItems = new List<ItemDataSO>();

		[Inject] private IItemService _itemService;
		[Inject] private DiContainer _diContainer;

		private readonly List<ShopItemUI> _shopItemUIs = new List<ShopItemUI>();

		private string _activeCategoryFilter = null;

		public void OnItemSelected(ItemDataSO itemSO)
		{
			_selectedItemSO = itemSO;

			// Update selection state of all shop item UIs
			foreach (var itemUI in _shopItemUIs)
			{
				itemUI.SetSelected(itemUI.ItemDataSO == itemSO);
			}

			UpdatePreview();
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
			SetupPreview();
		}

		private void LoadShopItemsFromResources()
		{
			_loadedShopItems.Clear();

			// Load all ItemDataSO from Resources folder
			ItemDataSO[] itemDataSOs = Resources.LoadAll<ItemDataSO>("");

			foreach (var itemSO in itemDataSOs)
			{
				if (itemSO != null && itemSO.ItemData != null && itemSO.ItemData.IsValid())
				{
					_loadedShopItems.Add(itemSO);
				}
			}

			Debug.Log($"ShopController: Loaded {_loadedShopItems.Count} items from Resources");
		}

		private void SetupPreview()
		{
			shopItemPreview.SetActive(false);

			previewPurchaseButton.onClick.RemoveAllListeners();
			previewPurchaseButton.onClick.AddListener(OnPreviewPurchaseClicked);

			LayoutRebuilder.ForceRebuildLayoutImmediate(shopScrollRect.content);
			LayoutRebuilder.ForceRebuildLayoutImmediate(previewDescriptionScrollRect.content);
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
				LayoutRebuilder.ForceRebuildLayoutImmediate(previewDescriptionScrollRect.content);
				shopScrollRect.verticalNormalizedPosition = 1f; // Reset scroll position to top
				previewDescriptionScrollRect.verticalNormalizedPosition = 1f; // Reset preview scroll position to top
			}
			else
			{
				Debug.LogWarning("ShopController: shopScrollRect or its content is not set up correctly!");
			}
		}

		private void CreateShopItemUI(ItemDataSO itemSO)
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
			if (_selectedItemSO != null)
			{
				UpdatePreviewAffordability();
			}
		}

		private void UpdatePreview()
		{
			if (_selectedItemSO == null || _selectedItemSO.ItemData == null)
			{
				if (shopItemPreview != null)
				{
					shopItemPreview.SetActive(false);
				}

				return;
			}

			shopItemPreview.SetActive(true);

			var itemData = _selectedItemSO.ItemData;

			previewItemIcon.sprite = itemData.Icon;
			previewItemIcon.gameObject.SetActive(itemData.Icon != null);

			previewItemName.text = itemData.Name;
			previewItemPrice.text = $"{itemData.Price} Gold";
			previewItemDescription.text = itemData.Description;

			Canvas.ForceUpdateCanvases();
			LayoutRebuilder.ForceRebuildLayoutImmediate(previewDescriptionScrollRect.content);

			UpdatePreviewAffordability();
		}

		private void UpdatePreviewAffordability()
		{
			if (_selectedItemSO == null || previewPurchaseButton == null)
			{
				return;
			}

			bool canAfford = _itemService.CanPurchaseItem(_selectedItemSO);
			previewPurchaseButton.interactable = canAfford;

			if (previewPurchaseButtonText != null)
			{
				previewPurchaseButtonText.text = canAfford ? "Buy" : "Can't Afford";
			}
		}

		private void OnPreviewPurchaseClicked()
		{
			if (_selectedItemSO == null || _itemService == null)
			{
				return;
			}

			bool success = _itemService.PurchaseItem(_selectedItemSO, 1);

			if (success)
			{
				Debug.Log($"Successfully purchased from preview: {_selectedItemSO.ItemData.Name}");
			}
			else
			{
				Debug.Log($"Failed to purchase from preview: {_selectedItemSO.ItemData.Name}");
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
					Destroy(itemUI.gameObject);
			}
			_shopItemUIs.Clear();

			// Filter items
			List<ItemDataSO> filteredItems;
			if (string.IsNullOrEmpty(_activeCategoryFilter) || _activeCategoryFilter == "All")
			{
				filteredItems = new List<ItemDataSO>(_loadedShopItems);
			}
			else
			{
				filteredItems = _loadedShopItems.FindAll(item => item.ItemData.ItemType.ToString() == _activeCategoryFilter);
			}

			foreach (var itemSO in filteredItems)
			{
				CreateShopItemUI(itemSO);
			}
			EnsureScrollableContent();
		}

		// Editor helper to setup shop items
		[ContextMenu("Refresh Shop Items")]
		private void RefreshShopItems()
		{
			if (Application.isPlaying && _itemService != null)
			{
				LoadShopItemsFromResources();
				_itemService.SetupShop(_loadedShopItems);
				CreateShopItemUIs();
			}
		}
	}
}
