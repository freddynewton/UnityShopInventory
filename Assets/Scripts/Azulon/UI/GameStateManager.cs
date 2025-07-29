using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Azulon.Services;

namespace Azulon.UI
{
	public enum GameState
	{
		MainMenu,
		Shop,
		Inventory
	}

	public class GameStateManager : MonoBehaviour
	{
		[Header("UI Controllers")]
		[SerializeField] private ShopController shopController;
		[SerializeField] private InventoryController inventoryController;

		[Header("Navigation Buttons")]
		[SerializeField] private Button shopButton;
		[SerializeField] private Button inventoryButton;
		[SerializeField] private Button mainMenuButton;

		[Header("Main Menu")]
		[SerializeField] private GameObject mainMenuPanel;

		[Inject] private IItemService _itemService;

		private GameState _currentState = GameState.MainMenu;

		public GameState CurrentState => _currentState;

		private void Start()
		{
			SetupButtons();
			SetInitialState();
		}

		private void SetupButtons()
		{
			if (shopButton != null)
				shopButton.onClick.AddListener(() => ChangeState(GameState.Shop));

			if (inventoryButton != null)
				inventoryButton.onClick.AddListener(() => ChangeState(GameState.Inventory));

			if (mainMenuButton != null)
				mainMenuButton.onClick.AddListener(() => ChangeState(GameState.MainMenu));
		}

		private void SetInitialState()
		{
			ChangeState(GameState.MainMenu);
		}

		public void ChangeState(GameState newState)
		{
			if (_currentState == newState)
				return;

			ExitCurrentState();
			_currentState = newState;
			EnterNewState();
		}

		private void ExitCurrentState()
		{
			switch (_currentState)
			{
				case GameState.MainMenu:
					if (mainMenuPanel != null)
						mainMenuPanel.SetActive(false);
					break;

				case GameState.Shop:
					if (shopController != null)
						shopController.CloseShop();
					break;

				case GameState.Inventory:
					if (inventoryController != null)
						inventoryController.CloseInventory();
					break;
			}
		}

		private void EnterNewState()
		{
			switch (_currentState)
			{
				case GameState.MainMenu:
					if (mainMenuPanel != null)
						mainMenuPanel.SetActive(true);
					break;

				case GameState.Shop:
					if (shopController != null)
						shopController.OpenShop();
					break;

				case GameState.Inventory:
					if (inventoryController != null)
						inventoryController.OpenInventory();
					break;
			}

			Debug.Log($"Changed to state: {_currentState}");
		}

		// Keyboard shortcuts for testing
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
				ChangeState(GameState.Shop);
			else if (Input.GetKeyDown(KeyCode.Alpha2))
				ChangeState(GameState.Inventory);
			else if (Input.GetKeyDown(KeyCode.Escape))
				ChangeState(GameState.MainMenu);
		}

		// Public methods for external access
		public void OpenShop() => ChangeState(GameState.Shop);
		public void OpenInventory() => ChangeState(GameState.Inventory);
		public void OpenMainMenu() => ChangeState(GameState.MainMenu);

		private void OnDestroy()
		{
			if (shopButton != null)
				shopButton.onClick.RemoveAllListeners();

			if (inventoryButton != null)
				inventoryButton.onClick.RemoveAllListeners();

			if (mainMenuButton != null)
				mainMenuButton.onClick.RemoveAllListeners();
		}

		// Save data when changing states
		private void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus && _itemService != null)
				_itemService.SaveData();
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if (!hasFocus && _itemService != null)
				_itemService.SaveData();
		}
	}
}
