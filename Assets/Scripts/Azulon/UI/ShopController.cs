using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using Azulon.Data;
using Azulon.Services;

namespace Azulon.UI
{
	public class ShopController : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private GameObject shopPanel;
		[SerializeField] private Transform shopItemsContainer;
		[SerializeField] private GameObject shopItemPrefab;
		[SerializeField] private TextMeshProUGUI currencyText;
		[SerializeField] private Button closeShopButton;

		[Header("Shop Configuration")]
		[SerializeField] private List<ItemDataSO> availableShopItems = new List<ItemDataSO>();

		[Inject] private IItemService _itemService;

		private readonly List<ShopItemUI> _shopItemUIs = new List<ShopItemUI>();

		private void Start()
		{
			InitializeShop();
			SetupEventListeners();
			UpdateCurrencyDisplay();
		}

		private void OnDestroy()
		{
			RemoveEventListeners();
		}

		private void InitializeShop()
		{
			_itemService.SetupShop(availableShopItems);
			CreateShopItemUIs();
		}

		private void SetupEventListeners()
		{
			_itemService.OnCurrencyChanged += UpdateCurrencyDisplay;
			_itemService.OnItemPurchased += OnItemPurchased;

			if (closeShopButton != null)
				closeShopButton.onClick.AddListener(CloseShop);
		}

		private void RemoveEventListeners()
		{
			if (_itemService != null)
			{
				_itemService.OnCurrencyChanged -= UpdateCurrencyDisplay;
				_itemService.OnItemPurchased -= OnItemPurchased;
			}

			if (closeShopButton != null)
				closeShopButton.onClick.RemoveListener(CloseShop);
		}

		private void CreateShopItemUIs()
		{
			// Clear existing items
			foreach (var itemUI in _shopItemUIs)
			{
				if (itemUI != null)
					Destroy(itemUI.gameObject);
			}
			_shopItemUIs.Clear();

			// Create new shop item UIs
			var shopItems = _itemService.GetShopItems();
			foreach (var itemSO in shopItems)
			{
				CreateShopItemUI(itemSO);
			}
		}

		private void CreateShopItemUI(ItemDataSO itemSO)
		{
			if (shopItemPrefab == null || shopItemsContainer == null || itemSO == null)
				return;

			GameObject itemUIObj = Instantiate(shopItemPrefab, shopItemsContainer);
			ShopItemUI itemUI = itemUIObj.GetComponent<ShopItemUI>();

			if (itemUI != null)
			{
				itemUI.Initialize(itemSO, _itemService);
				_shopItemUIs.Add(itemUI);
			}
		}

		private void UpdateCurrencyDisplay(int newCurrency)
		{
			if (currencyText != null)
				currencyText.text = $"Gold: {newCurrency}";
		}

		private void UpdateCurrencyDisplay()
		{
			UpdateCurrencyDisplay(_itemService.Currency);
		}

		private void OnItemPurchased(ItemData purchasedItem)
		{
			Debug.Log($"Purchased: {purchasedItem.Name} x{purchasedItem.Quantity}");

			// Update all shop item UIs to reflect current affordability
			foreach (var itemUI in _shopItemUIs)
			{
				itemUI.UpdateAffordability();
			}
		}

		public void OpenShop()
		{
			if (shopPanel != null)
				shopPanel.SetActive(true);

			UpdateCurrencyDisplay();

			// Update all shop item UIs
			foreach (var itemUI in _shopItemUIs)
			{
				itemUI.UpdateAffordability();
			}
		}

		public void CloseShop()
		{
			if (shopPanel != null)
				shopPanel.SetActive(false);
		}

		public void ToggleShop()
		{
			if (shopPanel != null)
			{
				bool isActive = shopPanel.activeSelf;
				if (isActive)
					CloseShop();
				else
					OpenShop();
			}
		}

		// Editor helper to setup shop items
		[ContextMenu("Refresh Shop Items")]
		private void RefreshShopItems()
		{
			if (Application.isPlaying && _itemService != null)
			{
				_itemService.SetupShop(availableShopItems);
				CreateShopItemUIs();
			}
		}
	}
}
