using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSlot : MonoBehaviour {
	public int SlotIndex;
	public Vector2 SlotSize;

	public void Initialize(ItemStack stack) {
		ItemInfo itemInfo = ItemManager.FindItem(stack.ItemName).GetComponent<ItemInfo>();
		GameObject itemObject = new GameObject(itemInfo.ItemName);
		itemObject.transform.parent = transform;
		itemObject.transform.localPosition = new Vector3(SlotSize.x/2f, SlotSize.y/2f, 0);
		//itemObject.
	}

	public void OnClick() {
		
	}
}
