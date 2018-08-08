using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack {

	public string ItemName;
	public int StackSize;

	public ItemStack(string itemName, int stackSize) {
		ItemName = itemName;
		StackSize = stackSize;
	}

	public GameObject GetItem() {
		return ItemManager.FindItem(ItemName);
	}
}
