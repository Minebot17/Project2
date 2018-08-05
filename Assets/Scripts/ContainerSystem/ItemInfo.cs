using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemInfo {

	public int ID; // ID for item type
	public int MaxStackSize;
	public string UnlocalizedName;

	protected ItemInfo(int maxStackSize, string unlocalizedName) {
		ID = ItemManager.Items.Count;
		MaxStackSize = maxStackSize;
		UnlocalizedName = unlocalizedName;
		ItemManager.Items.Add(this);
	}
}
