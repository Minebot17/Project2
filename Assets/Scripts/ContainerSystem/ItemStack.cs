using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack : MonoBehaviour {

	public int ItemID;
	public int StackSize;

	public ItemInfo GetItem() {
		return ItemManager.Items[ItemID];
	}
}
