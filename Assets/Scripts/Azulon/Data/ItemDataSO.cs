using UnityEngine;

namespace Azulon.Data
{
	[CreateAssetMenu(fileName = "New Item", menuName = "Azulon/Item Data", order = 1)]
	public class ItemDataSO : ScriptableObject
	{
		[Header("Item Configuration")]
		[SerializeField] private ItemData itemData = new ItemData();

		public ItemData ItemData => itemData;

		/// <summary>
		/// Returns a deep copy of the item data to prevent unwanted modifications
		/// </summary>
		public ItemData GetItemDataCopy()
		{
			return new ItemData(itemData);
		}

		private void OnValidate()
		{
			if (itemData != null && !itemData.IsValid())
			{
				Debug.LogWarning($"ItemDataSO '{name}' has invalid data. Please check the configuration.", this);
			}
		}

		// Helper method to create runtime item data with specific quantity
		public ItemData CreateRuntimeItemData(int quantity = 1)
		{
			var runtimeData = new ItemData(itemData);
			runtimeData.Quantity = quantity;
			return runtimeData;
		}
	}
}
