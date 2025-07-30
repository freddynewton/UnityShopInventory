using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Azulon.Data;
using Azulon.Services;
using UnityEngine.EventSystems;
using System.Collections;
using Zenject;
using DG.Tweening;

namespace Azulon.UI
{
	public class ShopItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[Header("UI References")]
		[SerializeField] private Image _itemIcon;
		[SerializeField] private Image _backgroundImage;
		[SerializeField] private TextMeshProUGUI _itemNameText;
		[SerializeField] private TextMeshProUGUI _itemPriceText;
		[SerializeField] private Button _purchaseButton;
		[SerializeField] private TextMeshProUGUI _purchaseButtonText;
		[SerializeField] private TextMeshProUGUI _itemAmountText;

		public ItemData ItemData { get; private set; }

		private ShopController _shopController;
		private IItemService _itemService;
		private bool _isSelected = false;

		[Inject] private UIColorSettingsSO _uiColorSettings;

		public void Initialize(ItemData itemDataSO, IItemService itemService, ShopController shopController)
		{
			ItemData = itemDataSO;
			_itemService = itemService;
			_shopController = shopController;

			if (ItemData == null)
			{
				Debug.LogError("ShopItemUI: Invalid ItemData provided!");
				return;
			}

			SetupUI();
			SetupButtons();
			UpdateAffordability();
		}

		private void SetupUI()
		{
			_itemIcon.sprite = ItemData.Icon;
			_itemIcon.gameObject.SetActive(ItemData.Icon != null);

			if (_itemIcon.rectTransform != null)
			{
				_itemIcon.rectTransform.sizeDelta = new Vector2(128, 128);
			}

			_itemNameText.text = ItemData.Name;
			_itemPriceText.text = $"{ItemData.Price} Gold";
			_itemAmountText.text = $"x{ItemData.Quantity}";
		}

		private void SetupButtons()
		{
			_purchaseButton.onClick.RemoveAllListeners();
			_purchaseButton.onClick.AddListener(OnPurchaseClicked);
			_purchaseButtonText.text = "Buy";
		}

		public void UpdateAffordability()
		{
			if (_itemService == null || ItemData == null)
			{
				return;
			}

			bool canAfford = _itemService.CanPurchaseItem(ItemData);

			// Update button interactability
			_purchaseButton.interactable = canAfford;

			// Update visual appearance based on selection and affordability
			Color targetColor = _isSelected ? _uiColorSettings.accentColor : (canAfford ? _uiColorSettings.primaryColor : _uiColorSettings.disabledColor);
			_purchaseButtonText.text = canAfford ? "Buy" : "Can't Afford";

			_backgroundImage.color = targetColor;
		}

		public void SetSelected(bool selected)
		{
			_isSelected = selected;
			UpdateAffordability(); // This will update colors based on selection state
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (_shopController == null && ItemData == null)
			{
				return;
			}

			_shopController.OnItemSelected(ItemData);
		}

		private void OnPurchaseClicked()
		{
			if (_itemService == null || ItemData == null)
			{
				return;
			}

			bool success = _itemService.PurchaseItem(ItemData, 1);

			if (success)
			{
				Debug.Log($"Successfully purchased: {ItemData.Name}");
				StartCoroutine(PurchaseFeedback());
			}
			else
			{
				Debug.Log($"Failed to purchase: {ItemData.Name}");
			}
		}

		private IEnumerator PurchaseFeedback()
		{
			if (_purchaseButtonText != null)
			{
				string originalText = _purchaseButtonText.text;
				_purchaseButtonText.text = "Purchased!";

				yield return new WaitForSeconds(0.5f);

				_purchaseButtonText.text = originalText;
				UpdateAffordability();
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			transform.DOKill();
			transform.DOScale(1.1f, 0.25f).SetEase(Ease.OutBack);

			_itemIcon.DOKill();
			_itemIcon.transform.DOScale(0.9f, 0.25f).SetEase(Ease.OutBack);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			transform.DOKill();
			transform.DOScale(1.0f, 0.2f).SetEase(Ease.OutBack);

			_itemIcon.DOKill();
			_itemIcon.transform.DOScale(1, 0.25f).SetEase(Ease.OutBack);
		}

		private void OnDestroy()
		{
			if (_purchaseButton != null)
			{
				_purchaseButton.onClick.RemoveAllListeners();
			}
		}
	}
}
