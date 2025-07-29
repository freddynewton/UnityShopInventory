using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Azulon.Data;
using Azulon.Services;

namespace Azulon.UI
{
	public class InventoryItemUI : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private Image itemIcon;
		[SerializeField] private TextMeshProUGUI itemNameText;
		[SerializeField] private TextMeshProUGUI itemDescriptionText;
		[SerializeField] private TextMeshProUGUI itemQuantityText;
		[SerializeField] private TextMeshProUGUI itemTypeText;
		[SerializeField] private Button useButton;
		[SerializeField] private Button removeButton;

		[Header("Visual Settings")]
		[SerializeField]
		private Color[] itemTypeColors = new Color[4]
		{
			Color.green,    // Consumable
            Color.blue,     // Equipment
            Color.yellow,   // Material
            Color.magenta   // Valuable
        };

		private ItemData _itemData;
		private IItemService _itemService;

		public void Initialize(ItemData itemData, IItemService itemService)
		{
			_itemData = itemData;
			_itemService = itemService;

			if (_itemData == null)
			{
				Debug.LogError("InventoryItemUI: Invalid ItemData provided!");
				return;
			}

			SetupUI();
			SetupButtons();
		}

		private void SetupUI()
		{
			if (itemIcon != null)
			{
				itemIcon.sprite = _itemData.Icon;
				itemIcon.gameObject.SetActive(_itemData.Icon != null);
			}

			if (itemNameText != null)
				itemNameText.text = _itemData.Name;

			if (itemDescriptionText != null)
				itemDescriptionText.text = _itemData.Description;

			if (itemQuantityText != null)
				itemQuantityText.text = $"x{_itemData.Quantity}";

			if (itemTypeText != null)
			{
				itemTypeText.text = _itemData.ItemType.ToString();

				// Set color based on item type
				int typeIndex = (int)_itemData.ItemType;
				if (typeIndex >= 0 && typeIndex < itemTypeColors.Length)
				{
					itemTypeText.color = itemTypeColors[typeIndex];
				}
			}
		}

		private void SetupButtons()
		{
			if (useButton != null)
			{
				useButton.onClick.RemoveAllListeners();
				useButton.onClick.AddListener(OnUseClicked);

				// Only show use button for consumable items
				useButton.gameObject.SetActive(_itemData.ItemType == ItemType.Consumable);
			}

			if (removeButton != null)
			{
				removeButton.onClick.RemoveAllListeners();
				removeButton.onClick.AddListener(OnRemoveClicked);
			}
		}

		private void OnUseClicked()
		{
			if (_itemService == null || _itemData == null)
				return;

			// For consumable items, remove one from inventory when used
			if (_itemData.ItemType == ItemType.Consumable)
			{
				bool success = _itemService.RemoveItem(_itemData.Id, 1);

				if (success)
				{
					Debug.Log($"Used: {_itemData.Name}");

					// Here you could add item effect logic
					ApplyItemEffect();
				}
				else
				{
					Debug.Log($"Failed to use: {_itemData.Name}");
				}
			}
		}

		private void OnRemoveClicked()
		{
			if (_itemService == null || _itemData == null)
				return;

			// Remove one item from inventory
			bool success = _itemService.RemoveItem(_itemData.Id, 1);

			if (success)
			{
				Debug.Log($"Removed: {_itemData.Name}");
			}
			else
			{
				Debug.Log($"Failed to remove: {_itemData.Name}");
			}
		}

		private void ApplyItemEffect()
		{
			// This is where you would implement specific item effects
			// For now, just log the effect
			switch (_itemData.ItemType)
			{
				case ItemType.Consumable:
					Debug.Log($"Applied consumable effect for {_itemData.Name}");
					// Example: Restore health, gain experience, etc.
					break;
				case ItemType.Equipment:
					Debug.Log($"Equipped {_itemData.Name}");
					// Example: Change player stats, appearance, etc.
					break;
				default:
					Debug.Log($"No specific effect for {_itemData.ItemType} items");
					break;
			}
		}

		public void UpdateDisplay()
		{
			if (_itemData != null)
			{
				SetupUI();
			}
		}

		private void OnDestroy()
		{
			if (useButton != null)
				useButton.onClick.RemoveAllListeners();

			if (removeButton != null)
				removeButton.onClick.RemoveAllListeners();
		}
	}
}
