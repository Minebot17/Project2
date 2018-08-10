using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemStack : ISerializableObject {

	public string ItemName;
	public int StackSize;

	public ItemStack() {
	}
	
	public ItemStack(string itemName, int stackSize) {
		ItemName = itemName;
		StackSize = stackSize;
	}

	public GameObject GetItem() {
		return ItemManager.FindItem(ItemName);
	}

	public void Initialize() {
		throw new System.NotImplementedException();
	}

	public List<string> Serialize() {
		return new List<string>() {ItemName, StackSize + ""};
	}

	public int Deserialize(List<string> data) {
		ItemName = data[0];
		StackSize = int.Parse(data[1]);
		return 2;
	}
}
