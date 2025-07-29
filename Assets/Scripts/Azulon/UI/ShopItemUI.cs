using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Azulon.Data;
using Azulon.Services;

namespace Azulon.UI
{
	public class ShopItemUI : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private Image itemIcon;
		[SerializeField] private TextMeshProUGUI itemNameText;
		[SerializeField] private TextMeshProUGUI itemDescriptionText;
		[SerializeField] private TextMeshProUGUI itemPriceText;
		[SerializeField] private Button purchaseButton;
		[SerializeField] private TextMeshProUGUI purchaseButtonText;

		[Header("Visual Settings")]
		[SerializeField] private Color affordableColor = Color.white;
		[SerializeField] private Color unaffordableColor = Color.gray;

		private ItemDataSO _itemDataSO;
		private IItemService _itemService;

		public void Initialize(ItemDataSO itemDataSO, IItemService itemService)
		{
			_itemDataSO = itemDataSO;
			_itemService = itemService;

			if (_itemDataSO == null || _itemDataSO.ItemData == null)
			{
				Debug.LogError("ShopItemUI: Invalid ItemDataSO provided!");
				return;
			}

			SetupUI();
			SetupButton();
			UpdateAffordability();
		}

		private void SetupUI()
		{
			var itemData = _itemDataSO.ItemData;

			if (itemIcon != null)
			{
				itemIcon.sprite = itemData.Icon;
				itemIcon.gameObject.SetActive(itemData.Icon != null);
			}

			if (itemNameText != null)
				itemNameText.text = itemData.Name;

			if (itemDescriptionText != null)
				itemDescriptionText.text = itemData.Description;

			if (itemPriceText != null)
				itemPriceText.text = $"{itemData.Price} Gold";
		}

		private void SetupButton()
		{
			if (purchaseButton != null)
			{
				purchaseButton.onClick.RemoveAllListeners();
				purchaseButton.onClick.AddListener(OnPurchaseClicked);
			}

			if (purchaseButtonText != null)
				purchaseButtonText.text = "Buy";
		}

		public void UpdateAffordability()
		{
			if (_itemService == null || _itemDataSO == null)
				return;

			bool canAfford = _itemService.CanPurchaseItem(_itemDataSO);

			// Update button interactability
			if (purchaseButton != null)
				purchaseButton.interactable = canAfford;

			// Update visual appearance
			Color targetColor = canAfford ? affordableColor : unaffordableColor;

			if (itemIcon != null)
				itemIcon.color = targetColor;

			if (itemNameText != null)
				itemNameText.color = targetColor;

			if (itemPriceText != null)
				itemPriceText.color = targetColor;

			// Update button text
			if (purchaseButtonText != null)
			{
				purchaseButtonText.text = canAfford ? "Buy" : "Can't Afford";
				purchaseButtonText.color = canAfford ? Color.white : Color.red;
			}
		}

		private void OnPurchaseClicked()
		{
			if (_itemService == null || _itemDataSO == null)
				return;

			bool success = _itemService.PurchaseItem(_itemDataSO, 1);

			if (success)
			{
				Debug.Log($"Successfully purchased: {_itemDataSO.ItemData.Name}");

				// Visual feedback could be added here
				StartCoroutine(PurchaseFeedback());
			}
			else
			{
				Debug.Log($"Failed to purchase: {_itemDataSO.ItemData.Name}");
			}
		}

		private System.Collections.IEnumerator PurchaseFeedback()
		{
			// Simple purchase feedback animation
			if (purchaseButtonText != null)
			{
				string originalText = purchaseButtonText.text;
				purchaseButtonText.text = "Purchased!";
				purchaseButtonText.color = Color.green;

				yield return new WaitForSeconds(0.5f);

				purchaseButtonText.text = originalText;
				UpdateAffordability(); // Reset color based on affordability
			}
		}

		private void OnDestroy()
		{
			if (purchaseButton != null)
				purchaseButton.onClick.RemoveAllListeners();
		}
	}
}
