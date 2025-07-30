using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Azulon.Data;
using Azulon.Services;
using Zenject;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Azulon.UI
{
	public class InventoryItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[Header("UI References")]
		[SerializeField] private Image itemIcon;
		[SerializeField] private TextMeshProUGUI itemNameText;
		[SerializeField] private TextMeshProUGUI itemQuantityText;
		[SerializeField] private TextMeshProUGUI itemTypeText;
		[SerializeField] private Image backgroundImage;

		private ItemData _itemData;
		private InventoryController _inventoryController;
		private bool _isSelected = false;

		[Inject] private IItemService _itemService;
		[Inject] private UIColorSettingsSO _colorSettings;

		// Colors for item types (example, can be set in inspector or code)
		[SerializeField] private Color[] itemTypeColors;

		public void Initialize(ItemData itemData, InventoryController controller = null)
		{
			_itemData = itemData;
			_inventoryController = controller;

			if (_itemData == null)
			{
				Debug.LogError("InventoryItemUI: Invalid ItemData provided!");
				return;
			}

			SetupUI();
			UpdateSelectionVisual();
		}

		private void SetupUI()
		{
			if (itemIcon != null)
			{
				itemIcon.sprite = _itemData.Icon;
				itemIcon.gameObject.SetActive(_itemData.Icon != null);
			}

			itemNameText.text = _itemData.Name;
			itemQuantityText.text = $"x{_itemData.Quantity}";
			itemTypeText.text = _itemData.ItemType.ToString();

			// Set color based on item type
			int typeIndex = (int)_itemData.ItemType;
			if (itemTypeColors != null && typeIndex >= 0 && typeIndex < itemTypeColors.Length)
			{
				itemTypeText.color = itemTypeColors[typeIndex];
			}
			else
			{
				itemTypeText.color = _colorSettings.primaryColor;
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			// Notify the inventory controller of this selection
			// This will handle deselecting other items
			if (_inventoryController != null)
			{
				_inventoryController.SelectItem(this);
			}
			
			// Mark this item as selected
			SetSelected(true);
			
			// Show the item preview
			_inventoryController?.ShowItemPreview(_itemData);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			transform.DOKill();
			transform.DOScale(1.1f, 0.25f).SetEase(Ease.OutBack);

			backgroundImage.color = _colorSettings.accentColor;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			transform.DOKill();
			transform.DOScale(1.0f, 0.2f).SetEase(Ease.OutBack);
			UpdateSelectionVisual();
		}

		public void SetSelected(bool selected)
		{
			_isSelected = selected;
			UpdateSelectionVisual();
		}

		private void UpdateSelectionVisual()
		{
			backgroundImage.color = _isSelected ? _colorSettings.accentColor : _colorSettings.primaryColor;
		}
	}
}
