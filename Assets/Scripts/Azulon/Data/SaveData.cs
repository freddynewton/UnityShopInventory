using System;
using System.Collections.Generic;

namespace Azulon.Data
{
	[Serializable]
	public class SaveData
	{
		public int currency;
		public List<ItemData> inventoryItems;

		public SaveData()
		{
			currency = 100; // Default starting currency
			inventoryItems = new List<ItemData>();
		}

		public SaveData(int currency, List<ItemData> inventoryItems)
		{
			this.currency = currency;
			this.inventoryItems = inventoryItems ?? new List<ItemData>();
		}

		public bool IsValid()
		{
			return currency >= 0 && inventoryItems != null;
		}
	}
}
