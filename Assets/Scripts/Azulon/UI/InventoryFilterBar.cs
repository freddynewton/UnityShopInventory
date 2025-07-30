using System.Collections.Generic;
using UnityEngine;
using Azulon.Data;
using UnityEngine.UI;
using Zenject;
using TMPro;

namespace Azulon.UI
{
	public class InventoryFilterBar : MonoBehaviour
	{
		[SerializeField] private GameObject _filterButtonPrefab;
		[SerializeField] private InventoryController _inventoryController;

		[Inject] private UIColorSettingsSO _uiColorSettings;

		private Dictionary<string, Button> _categoryButtons = new();
		private string _selectedCategory = "All";

		private void Start()
		{
			var categories = new List<string> { "All" };
			categories.AddRange(System.Enum.GetNames(typeof(ItemType)));

			foreach (var category in categories)
			{
				var buttonGO = Instantiate(_filterButtonPrefab, transform);
				var button = buttonGO.GetComponent<Button>();
				_categoryButtons[category] = button;

				// Set button text
				var text = buttonGO.GetComponentInChildren<TMP_Text>();
				if (text != null)
				{
					text.text = category;
				}

				// Subscribe to click event
				string capturedCategory = category;
				button.onClick.AddListener(() => OnButtonClick(capturedCategory));
			}

			UpdateButtonColors();
		}

		private void OnButtonClick(string category)
		{
			_selectedCategory = category;
			_inventoryController.FilterByCategory(category);
			UpdateButtonColors();
		}

		private void UpdateButtonColors()
		{
			foreach (var kvp in _categoryButtons)
			{
				var button = kvp.Value;
				var image = button.GetComponent<Image>();

				if (image != null)
				{
					image.color = kvp.Key == _selectedCategory ? _uiColorSettings.accentColor : _uiColorSettings.primaryColor;
				}
			}
		}
	}
}