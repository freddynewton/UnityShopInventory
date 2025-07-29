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
        [SerializeField] private TextMeshProUGUI itemPriceText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TextMeshProUGUI purchaseButtonText;
        [SerializeField] private Button selectButton;

        [Header("Visual Settings")]
        [SerializeField] private Color affordableColor = Color.white;
        [SerializeField] private Color unaffordableColor = Color.gray;
        [SerializeField] private Color selectedColor = Color.yellow;

        public ItemDataSO ItemDataSO { get; private set; }
        
        private ShopController _shopController;
        private IItemService _itemService;
        private bool _isSelected = false;

        public void Initialize(ItemDataSO itemDataSO, IItemService itemService, ShopController shopController)
        {
            ItemDataSO = itemDataSO;
            _itemService = itemService;
            _shopController = shopController;

            if (ItemDataSO == null || ItemDataSO.ItemData == null)
            {
                Debug.LogError("ShopItemUI: Invalid ItemDataSO provided!");
                return;
            }

            SetupUI();
            SetupButtons();
            UpdateAffordability();
        }

        private void SetupUI()
        {
            var itemData = ItemDataSO.ItemData;

            if (itemIcon != null)
            {
                itemIcon.sprite = itemData.Icon;
                itemIcon.gameObject.SetActive(itemData.Icon != null);
            }

            if (itemNameText != null)
                itemNameText.text = itemData.Name;

            if (itemPriceText != null)
                itemPriceText.text = $"{itemData.Price} Gold";
        }

        private void SetupButtons()
        {
            // Setup purchase button
            if (purchaseButton != null)
            {
                purchaseButton.onClick.RemoveAllListeners();
                purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }

            // Setup select button (for clicking the item to show preview)
            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(OnSelectClicked);
            }
            else
            {
                // If no separate select button, use the whole item as clickable
                Button itemButton = GetComponent<Button>();
                if (itemButton == null)
                {
                    itemButton = gameObject.AddComponent<Button>();
                    // Ensure button doesn't interfere with layout
                    itemButton.transition = Selectable.Transition.None;
                }
                    
                itemButton.onClick.RemoveAllListeners();
                itemButton.onClick.AddListener(OnSelectClicked);
            }

            if (purchaseButtonText != null)
                purchaseButtonText.text = "Buy";
        }

        public void UpdateAffordability()
        {
            if (_itemService == null || ItemDataSO == null)
                return;

            bool canAfford = _itemService.CanPurchaseItem(ItemDataSO);

            // Update button interactability
            if (purchaseButton != null)
                purchaseButton.interactable = canAfford;

            // Update visual appearance based on selection and affordability
            Color targetColor = _isSelected ? selectedColor : (canAfford ? affordableColor : unaffordableColor);

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

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateAffordability(); // This will update colors based on selection state
        }

        private void OnSelectClicked()
        {
            if (_shopController != null && ItemDataSO != null)
            {
                _shopController.OnItemSelected(ItemDataSO);
            }
        }

        private void OnPurchaseClicked()
        {
            if (_itemService == null || ItemDataSO == null)
                return;

            bool success = _itemService.PurchaseItem(ItemDataSO, 1);

            if (success)
            {
                Debug.Log($"Successfully purchased: {ItemDataSO.ItemData.Name}");
                StartCoroutine(PurchaseFeedback());
            }
            else
            {
                Debug.Log($"Failed to purchase: {ItemDataSO.ItemData.Name}");
            }
        }

        private System.Collections.IEnumerator PurchaseFeedback()
        {
            if (purchaseButtonText != null)
            {
                string originalText = purchaseButtonText.text;
                purchaseButtonText.text = "Purchased!";
                purchaseButtonText.color = Color.green;

                yield return new WaitForSeconds(0.5f);

                purchaseButtonText.text = originalText;
                UpdateAffordability();
            }
        }

        private void OnDestroy()
        {
            if (purchaseButton != null)
                purchaseButton.onClick.RemoveAllListeners();
                
            if (selectButton != null)
                selectButton.onClick.RemoveAllListeners();
        }
	}
}
