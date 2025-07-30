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

		[Inject] private IItemService _itemService;

		private UIState _currentState = UIState.None;

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

			inventoryController.CloseInventory();
			shopController.OpenShop();
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

			Debug.Log("UIManager: Opened Inventory");
		}

		// Private Methods
		private void Start()
		{
			SetupNavigation();
			SetInitialState();
		}

		private void OnDestroy()
		{
			RemoveNavigationListeners();
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
			OpenShop();
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
