using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Azulon.Data;
using Azulon.Services;
using Zenject;
using System;

namespace Azulon.UI
{
	public class ItemPreviewUI : MonoBehaviour
	{
		[Header("Preview UI References")]
		[SerializeField] private GameObject _previewPanel;
		[SerializeField] private Image _previewItemIcon;
		[SerializeField] private ScrollRect _previewDescriptionScrollRect;
		[SerializeField] private TextMeshProUGUI _previewItemName;
		[SerializeField] private TextMeshProUGUI _previewItemPrice;
		[SerializeField] private TextMeshProUGUI _previewItemDescription;
		[SerializeField] private TextMeshProUGUI _previewPurchaseButtonText;
		[SerializeField] private TextMeshProUGUI _previewItemAmount;
		[SerializeField] private TextMeshProUGUI _previewSellButtonText;
		[SerializeField] private Button _previewPurchaseButton;
		[SerializeField] private Button _previewSellButton;

		private ItemData _selectedItem;
		private IItemService _itemService;
		
		// Custom action delegates for purchase and sell operations
		private Action<ItemData> _customPurchaseAction;
		private Action<ItemData> _customSellAction;
		private bool _useCustomActions = false;

		[Inject]
		public void Construct(IItemService itemService)
		{
			_itemService = itemService;
			
			// Subscribe to currency and inventory changes to update UI
			_itemService.OnCurrencyChanged += _ => UpdateButtonStates();
			_itemService.OnInventoryChanged += UpdateButtonStates;
		}

		private void Start()
		{
			// Set up button listeners
			_previewPurchaseButton.onClick.RemoveAllListeners();
			_previewPurchaseButton.onClick.AddListener(OnPreviewPurchaseClicked);

			_previewSellButton.onClick.RemoveAllListeners();
			_previewSellButton.onClick.AddListener(OnPreviewSellClicked);
			
			// Initially hide preview
			HidePreview();
		}

		public void ShowPreview(ItemData itemData)
		{
			_selectedItem = itemData;
			if (_selectedItem == null)
			{
				_previewPanel.SetActive(false);
				return;
			}

			_previewPanel.SetActive(true);

			// Set up the UI with the item data
			_previewItemIcon.sprite = itemData.Icon;
			_previewItemIcon.gameObject.SetActive(itemData.Icon != null);
			_previewItemName.text = itemData.Name;
			_previewItemPrice.text = $"{itemData.Price} Gold";
			_previewItemDescription.text = itemData.Description;
			_previewItemAmount.text = $"x{itemData.Quantity}";

			// Reset scroll position
			Canvas.ForceUpdateCanvases();
			LayoutRebuilder.ForceRebuildLayoutImmediate(_previewDescriptionScrollRect.content);
			_previewDescriptionScrollRect.verticalNormalizedPosition = 1f;

			// Update button states
			UpdateButtonStates();
		}

		public void HidePreview()
		{
			_previewPanel.SetActive(false);
			_selectedItem = null;
		}

		/// <summary>
		/// Sets up custom purchase and sell actions for the preview panel.
		/// This allows different controllers (Shop, Inventory) to customize the behavior.
		/// </summary>
		/// <param name="itemService">Reference to the item service</param>
		/// <param name="onPurchase">Custom action to execute when purchasing an item</param>
		/// <param name="onSell">Custom action to execute when selling an item</param>
		public void Setup(IItemService itemService, Action<ItemData> onPurchase, Action<ItemData> onSell)
		{
			// Store the custom actions
			_customPurchaseAction = onPurchase;
			_customSellAction = onSell;
			_useCustomActions = true;
			
			// Update the item service reference if provided
			if (itemService != null)
			{
				_itemService = itemService;
			}
			
			// Update UI states
			UpdateButtonStates();
		}

		/// <summary>
		/// Resets to default purchase and sell behavior
		/// </summary>
		public void ResetActions()
		{
			_customPurchaseAction = null;
			_customSellAction = null;
			_useCustomActions = false;
		}

		public void UpdateAffordability()
		{
			UpdateButtonStates();
		}

		public void UpdateButtonStates()
		{
			if (_selectedItem == null || _itemService == null)
			{
				return;
			}

			// Purchase button state
			bool canAfford = _itemService.CanPurchaseItem(_selectedItem);
			_previewPurchaseButton.interactable = canAfford;
			_previewPurchaseButtonText.text = canAfford ? "Buy" : "Can't Afford";

			// Sell button state
			int quantityInInventory = _itemService.GetItemQuantity(_selectedItem.Id);
			bool canSell = quantityInInventory > 0;
			_previewSellButton.interactable = canSell;
			_previewSellButtonText.text = canSell ? "Sell" : "None Owned";
		}

		private void OnPreviewPurchaseClicked()
		{
			if (_selectedItem == null || _itemService == null)
			{
				return;
			}

			if (_useCustomActions && _customPurchaseAction != null)
			{
				// Use custom purchase action if available
				_customPurchaseAction(_selectedItem);
			}
			else
			{
				// Default purchase behavior
				bool success = _itemService.PurchaseItem(_selectedItem, 1);

				if (success)
				{
					Debug.Log($"Successfully purchased: {_selectedItem.Name}");
					UpdateButtonStates();
				}
				else
				{
					Debug.Log($"Failed to purchase: {_selectedItem.Name}");
				}
			}
		}

		private void OnPreviewSellClicked()
		{
			if (_selectedItem == null || _itemService == null)
			{
				return;
			}

			if (_useCustomActions && _customSellAction != null)
			{
				// Use custom sell action if available
				_customSellAction(_selectedItem);
			}
			else
			{
				// Default sell behavior
				if (_itemService.RemoveItem(_selectedItem.Id, 1))
				{
					// Add the currency based on the item price
					_itemService.AddCurrency(_selectedItem.Price);
					Debug.Log($"Successfully sold: {_selectedItem.Name}");
					
					// Update the UI
					UpdateButtonStates();
					
					// If no more items of this type, hide the preview
					if (_itemService.GetItemQuantity(_selectedItem.Id) <= 0)
					{
						HidePreview();
					}
				}
				else
				{
					Debug.Log($"Failed to sell: {_selectedItem.Name}");
				}
			}
		}

		private void OnDestroy()
		{
			// Clean up event subscriptions
			if (_itemService != null)
			{
				_itemService.OnCurrencyChanged -= _ => UpdateButtonStates();
				_itemService.OnInventoryChanged -= UpdateButtonStates;
			}
		}
	}
}
