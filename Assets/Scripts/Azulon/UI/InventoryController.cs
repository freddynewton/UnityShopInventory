using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Azulon.Data;
using Azulon.Services;

namespace Azulon.UI
{
	public class InventoryController : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private GameObject _inventoryPanel;
		[SerializeField] private Transform _inventoryItemsContainer;
		[SerializeField] private GameObject _inventoryItemPrefab;
		[SerializeField] private ItemPreviewUI _itemPreviewUI;

		[Inject] private IItemService _itemService;
		[Inject] private DiContainer _diContainer;

		private readonly List<InventoryItemUI> _inventoryItemUIs = new List<InventoryItemUI>();
		private string _activeCategoryFilter = null;
		private InventoryItemUI _currentlySelectedItem = null;

		// Public Methods
		public void OpenInventory()
		{
			_inventoryPanel.SetActive(true);
			RefreshInventoryDisplay();
		}

		public void CloseInventory()
		{
			_inventoryPanel.SetActive(false);
		}

		public void ToggleInventory()
		{
			bool isActive = _inventoryPanel.activeSelf;
			if (isActive)
			{
				CloseInventory();
			}
			else
			{
				OpenInventory();
			}
		}

		public void FilterByCategory(string category)
		{
			_activeCategoryFilter = category;
			RefreshInventoryDisplay();
		}

		// Method to select an item and deselect all others
		public void SelectItem(InventoryItemUI selectedItem)
		{
			// Deselect current item if there is one and it's different from the new selection
			if (_currentlySelectedItem != null && _currentlySelectedItem != selectedItem)
			{
				_currentlySelectedItem.SetSelected(false);
			}

			// Update the current selection
			_currentlySelectedItem = selectedItem;
		}

		// Private Methods
		private void Start()
		{
			SetupEventListeners();
			RefreshInventoryDisplay();
		}

		private void OnDestroy()
		{
			RemoveEventListeners();
		}

		private void SetupEventListeners()
		{
			_itemService.OnInventoryChanged += RefreshInventoryDisplay;
			_itemService.OnItemAdded += OnItemChanged;
			_itemService.OnItemRemoved += OnItemChanged;
		}

		private void RemoveEventListeners()
		{
			if (_itemService != null)
			{
				_itemService.OnInventoryChanged -= RefreshInventoryDisplay;
				_itemService.OnItemAdded -= OnItemChanged;
				_itemService.OnItemRemoved -= OnItemChanged;
			}
		}

		private void OnItemChanged(ItemData item)
		{
			RefreshInventoryDisplay();
		}

		private void RefreshInventoryDisplay()
		{
			ClearInventoryDisplay();
			CreateInventoryItemUIs();
			UpdateInventoryTitle();
		}

		private void ClearInventoryDisplay()
		{
			foreach (var itemUI in _inventoryItemUIs)
			{
				if (itemUI != null)
				{
					Destroy(itemUI.gameObject);
				}
			}

			_inventoryItemUIs.Clear();
			_currentlySelectedItem = null; // Reset selected item when clearing the display
		}

		private void CreateInventoryItemUIs()
		{
			var inventoryItems = _itemService.GetInventoryItems();
			var filteredItems = new List<ItemData>();

			// Apply filtering
			if (string.IsNullOrEmpty(_activeCategoryFilter) || _activeCategoryFilter == "All")
			{
				filteredItems = new List<ItemData>(inventoryItems);
			}
			else
			{
				foreach (var item in inventoryItems)
				{
					if (item.ItemType.ToString() == _activeCategoryFilter)
					{
						filteredItems.Add(item);
					}
				}
			}

			foreach (var itemData in filteredItems)
			{
				CreateInventoryItemUI(itemData);
			}
		}

		private void CreateInventoryItemUI(ItemData itemData)
		{
			if (_inventoryItemPrefab == null || _inventoryItemsContainer == null || itemData == null)
			{
				return;
			}

			InventoryItemUI itemUI = _diContainer.InstantiatePrefabForComponent<InventoryItemUI>(_inventoryItemPrefab, _inventoryItemsContainer);

			if (itemUI != null)
			{
				itemUI.Initialize(itemData, this);
				_inventoryItemUIs.Add(itemUI);
			}
		}

		public void ShowItemPreview(ItemData itemData)
		{
			if (_itemPreviewUI == null || itemData == null)
			{
				return;
			}

			_itemPreviewUI.ShowPreview(itemData);
			_itemPreviewUI.Setup(_itemService, OnPreviewPurchase, OnPreviewSell);
		}

		private void OnPreviewPurchase(ItemData data)
		{
			if (data == null)
			{
				return;
			}

			if (_itemService.SpendCurrency(data.Price))
			{
				_itemService.AddItem(data);
				RefreshInventoryDisplay();
			}
			else
			{
				Debug.LogWarning($"Not enough currency to purchase {data.Name}");
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
				RefreshInventoryDisplay();

				if (_itemService.GetItemQuantity(itemData.Id) <= 0)
				{
					_itemPreviewUI.HidePreview();
				}
			}
		}

		private void UpdateInventoryTitle()
		{
			int totalItems = _itemService.GetInventoryItems().Count;
		}
	}
}
