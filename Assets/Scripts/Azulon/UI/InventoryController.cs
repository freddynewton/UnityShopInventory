using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using Azulon.Data;
using Azulon.Services;

namespace Azulon.UI
{
	public class InventoryController : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private GameObject inventoryPanel;
		[SerializeField] private Transform inventoryItemsContainer;
		[SerializeField] private GameObject inventoryItemPrefab;
		[SerializeField] private Button closeInventoryButton;
		[SerializeField] private TextMeshProUGUI inventoryTitleText;

		[Inject] private IItemService _itemService;

		private readonly List<InventoryItemUI> _inventoryItemUIs = new List<InventoryItemUI>();

		// Public Methods
		public void OpenInventory()
		{
			if (inventoryPanel != null)
			{
				inventoryPanel.SetActive(true);
				RefreshInventoryDisplay();
			}
		}

		public void CloseInventory()
		{
			if (inventoryPanel != null)
				inventoryPanel.SetActive(false);
		}

		public void ToggleInventory()
		{
			if (inventoryPanel != null)
			{
				bool isActive = inventoryPanel.activeSelf;
				if (isActive)
					CloseInventory();
				else
					OpenInventory();
			}
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

			if (closeInventoryButton != null)
				closeInventoryButton.onClick.AddListener(CloseInventory);
		}

		private void RemoveEventListeners()
		{
			if (_itemService != null)
			{
				_itemService.OnInventoryChanged -= RefreshInventoryDisplay;
				_itemService.OnItemAdded -= OnItemChanged;
				_itemService.OnItemRemoved -= OnItemChanged;
			}

			if (closeInventoryButton != null)
				closeInventoryButton.onClick.RemoveListener(CloseInventory);
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
					Destroy(itemUI.gameObject);
			}
			_inventoryItemUIs.Clear();
		}

		private void CreateInventoryItemUIs()
		{
			var inventoryItems = _itemService.GetInventoryItems();

			foreach (var itemData in inventoryItems)
			{
				CreateInventoryItemUI(itemData);
			}
		}

		private void CreateInventoryItemUI(ItemData itemData)
		{
			if (inventoryItemPrefab == null || inventoryItemsContainer == null || itemData == null)
				return;

			GameObject itemUIObj = Instantiate(inventoryItemPrefab, inventoryItemsContainer);
			InventoryItemUI itemUI = itemUIObj.GetComponent<InventoryItemUI>();

			if (itemUI != null)
			{
				itemUI.Initialize(itemData, _itemService);
				_inventoryItemUIs.Add(itemUI);
			}
		}

		private void UpdateInventoryTitle()
		{
			if (inventoryTitleText != null)
			{
				int totalItems = _itemService.GetInventoryItems().Count;
				inventoryTitleText.text = $"Inventory ({totalItems} items)";
			}
		}

		// Editor helper
		[ContextMenu("Refresh Inventory Display")]
		private void RefreshInventoryDisplayEditor()
		{
			if (Application.isPlaying)
				RefreshInventoryDisplay();
		}
	}
}
