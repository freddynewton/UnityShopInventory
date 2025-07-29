using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Azulon.Services;
using UnityEngine.InputSystem;

namespace Azulon.UI
{
	public class UIManager : MonoBehaviour
	{
		[Header("UI Controllers")]
		[SerializeField] private ShopController shopController;
		[SerializeField] private InventoryController inventoryController;

		[Header("Navigation Buttons")]
		[SerializeField] private Button shopButton;
		[SerializeField] private Button inventoryButton;

		[Header("Input Actions")]
		[SerializeField] private InputActionAsset inputActions;

		[Header("Visual Feedback")]
		[SerializeField] private Color selectedButtonColor = Color.yellow;
		[SerializeField] private Color normalButtonColor = Color.white;

		[Inject] private IItemService _itemService;

		private UIState _currentState = UIState.Shop;
		private InputActionMap _uiActionMap;
		private InputAction _shopAction;
		private InputAction _inventoryAction;

		// Public Properties
		public bool IsShopOpen => _currentState == UIState.Shop;
		public bool IsInventoryOpen => _currentState == UIState.Inventory;

		// Public Methods
		public void OpenShop()
		{
			if (_currentState == UIState.Shop)
			{
				return;
			}

			_currentState = UIState.Shop;

			// Close inventory first
			if (inventoryController != null)
			{
				inventoryController.CloseInventory();
			}

			// Open shop
			if (shopController != null)
			{
				shopController.OpenShop();
			}

			UpdateButtonVisuals();
			Debug.Log("UIManager: Opened Shop");
		}

		public void OpenInventory()
		{
			if (_currentState == UIState.Inventory)
			{
				return;
			}

			_currentState = UIState.Inventory;
			shopController?.CloseShop();
			inventoryController?.OpenInventory();

			UpdateButtonVisuals();
			Debug.Log("UIManager: Opened Inventory");
		}

		// Private Methods
		private void Start()
		{
			SetupInputActions();
			SetupNavigation();
			SetInitialState();
		}

		private void OnEnable()
		{
			EnableInputActions();
		}

		private void OnDisable()
		{
			DisableInputActions();
		}

		private void OnDestroy()
		{
			RemoveNavigationListeners();
		}

		private void SetupInputActions()
		{
			if (inputActions == null)
			{
				return;
			}

			_uiActionMap = inputActions.FindActionMap("UI");

			if (_uiActionMap == null)
			{
				return;
			}

			_inventoryAction = _uiActionMap.FindAction("OpenInventory");
			_shopAction = _uiActionMap.FindAction("OpenShop");
		}

		private void EnableInputActions()
		{
			if (_shopAction != null)
			{
				_shopAction.performed += OnShopActionPerformed;
				_shopAction.Enable();
			}

			if (_inventoryAction != null)
			{
				_inventoryAction.performed += OnInventoryActionPerformed;
				_inventoryAction.Enable();
			}
		}

		private void DisableInputActions()
		{
			_shopAction.performed -= OnShopActionPerformed;
			_shopAction.Disable();

			_inventoryAction.performed -= OnInventoryActionPerformed;
			_inventoryAction.Disable();
		}

		private void OnShopActionPerformed(InputAction.CallbackContext context)
		{
			OpenShop();
		}

		private void OnInventoryActionPerformed(InputAction.CallbackContext context)
		{
			OpenInventory();
		}

		private void SetupNavigation()
		{
			shopButton?.onClick.AddListener(OpenShop);
			inventoryButton?.onClick.AddListener(OpenInventory);
		}

		private void RemoveNavigationListeners()
		{
			shopButton?.onClick.RemoveListener(OpenShop);
			inventoryButton?.onClick.RemoveListener(OpenInventory);
		}

		private void SetInitialState()
		{
			// Start with Shop open
			OpenShop();
		}

		private void UpdateButtonVisuals()
		{
			// Update shop button
			if (shopButton != null)
			{
				var shopButtonImage = shopButton.GetComponent<Image>();
				if (shopButtonImage != null)
				{
					shopButtonImage.color = _currentState == UIState.Shop ? selectedButtonColor : normalButtonColor;
				}
			}

			// Update inventory button
			if (inventoryButton != null)
			{
				var inventoryButtonImage = inventoryButton.GetComponent<Image>();
				if (inventoryButtonImage != null)
				{
					inventoryButtonImage.color = _currentState == UIState.Inventory ? selectedButtonColor : normalButtonColor;
				}
			}
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus && _itemService != null)
			{
				_itemService.SaveData();
			}
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if (!hasFocus && _itemService != null)
			{
				_itemService.SaveData();
			}
		}
	}
}
