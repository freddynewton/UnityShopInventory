using System;
using UnityEngine;

namespace Azulon.Data
{
	[Serializable]
	public class ItemData
	{
		[SerializeField] private string id;
		[SerializeField] private string itemName;
		[SerializeField] private Sprite icon;
		[SerializeField] private string description;
		[SerializeField] private int price;
		[SerializeField] private int quantity;
		[SerializeField] private ItemType itemType;

		public string Id => id;
		public string Name => itemName;
		public Sprite Icon => icon;
		public string Description => description;
		public int Price => price;
		public int Quantity { get => quantity; set => quantity = value; }
		public ItemType ItemType => itemType;

		public ItemData(string id, string name, Sprite icon, string description, int price, ItemType itemType, int quantity = 1)
		{
			this.id = id;
			this.itemName = name;
			this.icon = icon;
			this.description = description;
			this.price = price;
			this.itemType = itemType;
			this.quantity = quantity;
		}

		// Default constructor for serialization
		public ItemData()
		{
			id = string.Empty;
			itemName = string.Empty;
			description = string.Empty;
			price = 0;
			quantity = 1;
			itemType = ItemType.Consumable;
		}

		// Copy constructor
		public ItemData(ItemData other)
		{
			id = other.id;
			itemName = other.itemName;
			icon = other.icon;
			description = other.description;
			price = other.price;
			quantity = other.quantity;
			itemType = other.itemType;
		}

		public bool IsValid()
		{
			return !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(itemName) && price >= 0;
		}
	}

	public enum ItemType
	{
		Consumable,
		Equipment,
		Material,
		Valuable
	}
}
