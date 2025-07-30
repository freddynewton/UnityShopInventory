using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Azulon.Data;
using Azulon.Services;
using System;

namespace Azulon.UI
{
	public class ItemPreviewUI : MonoBehaviour
	{
		[Header("Preview UI References")]
		[SerializeField] private GameObject _previewPanel;
		[SerializeField] private Image _previewItemIcon;
		[SerializeField] private TextMeshProUGUI _previewItemName;
		[SerializeField] private TextMeshProUGUI _previewItemPrice;
		[SerializeField] private ScrollRect _previewDescriptionScrollRect;
		[SerializeField] private TextMeshProUGUI _previewItemDescription;
		[SerializeField] private Button _previewPurchaseButton;
		[SerializeField] private TextMeshProUGUI _previewPurchaseButtonText;
		[SerializeField] private TextMeshProUGUI _previewItemAmount;

		private ItemDataSO _selectedItemSO;
		private IItemService _itemService;
		private Action<ItemDataSO> _onPurchase;

		public void Setup(IItemService itemService, Action<ItemDataSO> onPurchase)
		{
			_itemService = itemService;
			_onPurchase = onPurchase;
			if (_previewPurchaseButton != null)
			{
				_previewPurchaseButton.onClick.RemoveAllListeners();
				_previewPurchaseButton.onClick.AddListener(OnPreviewPurchaseClicked);
			}
		}

		public void ShowPreview(ItemDataSO itemSO)
		{
			_selectedItemSO = itemSO;
			if (_selectedItemSO == null || _selectedItemSO.ItemData == null)
			{
				if (_previewPanel != null)
					_previewPanel.SetActive(false);
				return;
			}
			_previewPanel.SetActive(true);

			var itemData = _selectedItemSO.ItemData;
			_previewItemIcon.sprite = itemData.Icon;
			_previewItemIcon.gameObject.SetActive(itemData.Icon != null);
			_previewItemName.text = itemData.Name;
			_previewItemPrice.text = $"{itemData.Price} Gold";
			_previewItemDescription.text = itemData.Description;
			_previewItemAmount.text = $"x{itemData.Quantity}";

			Canvas.ForceUpdateCanvases();
			LayoutRebuilder.ForceRebuildLayoutImmediate(_previewDescriptionScrollRect.content);
			_previewDescriptionScrollRect.verticalNormalizedPosition = 1f;

			UpdateAffordability();
		}

		public void HidePreview()
		{
			_previewPanel.SetActive(false);
		}

		public void UpdateAffordability()
		{
			if (_selectedItemSO == null || _previewPurchaseButton == null || _itemService == null)
			{
				return;
			}

			bool canAfford = _itemService.CanPurchaseItem(_selectedItemSO);

			_previewPurchaseButton.interactable = canAfford;
			_previewPurchaseButtonText.text = canAfford ? "Buy" : "Can't Afford";
		}

		private void OnPreviewPurchaseClicked()
		{
			if (_selectedItemSO == null || _itemService == null)
			{
				return;
			}

			bool success = _itemService.PurchaseItem(_selectedItemSO, 1);
			_onPurchase?.Invoke(_selectedItemSO);

			if (success)
			{
				Debug.Log($"Successfully purchased from preview: {_selectedItemSO.ItemData.Name}");
			}
			else
			{
				Debug.Log($"Failed to purchase from preview: {_selectedItemSO.ItemData.Name}");
			}
		}
	}
}
