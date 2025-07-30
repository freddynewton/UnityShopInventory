using UnityEngine;

namespace Azulon.Data
{
	[CreateAssetMenu(fileName = "New Item", menuName = "Azulon/Item Data", order = 1)]
	public class ItemDataSO : ScriptableObject
	{
		[Header("Item Configuration")]
		[SerializeField] private ItemData itemData = new ItemData();

		public ItemData ItemData => itemData;

		private void OnValidate()
		{
			if (itemData != null && !itemData.IsValid())
			{
				Debug.LogWarning($"ItemDataSO '{name}' has invalid data. Please check the configuration.", this);
			}
		}
	}
}
