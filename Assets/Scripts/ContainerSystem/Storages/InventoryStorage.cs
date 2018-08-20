using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryStorage : DefaultStorage {
	
	public override void SetItemStack(int slotId, ItemStack stack) {
		if (slotId == 0) {
			ItemStack oldStack = GetItemStack(slotId);
			ItemWeaponInfo oldItem = null;
			if (oldStack != null)
				oldItem = (ItemWeaponInfo) ItemManager.FindItemInfo(oldStack.ItemName);
			base.SetItemStack(slotId, stack);
			if (stack == null) {
				if (oldItem != null)
					ActiveWeapon(oldItem.WeaponIndex, false);

				return;
			}

			ItemWeaponInfo info = (ItemWeaponInfo) ItemManager.FindItemInfo(stack.ItemName);
			if (oldItem != null)
				ActiveWeapon(oldItem.WeaponIndex, false);
			ActiveWeapon(info.WeaponIndex, true);
		}
		else
			base.SetItemStack(slotId, stack);
	}
	
	public override void RemoveItemStack(int slotId) {
		if (slotId == 0) {
			ItemStack oldStack = GetItemStack(slotId);
			base.RemoveItemStack(slotId);
			if (oldStack != null)
				ActiveWeapon(((ItemWeaponInfo) ItemManager.FindItemInfo(oldStack.ItemName)).WeaponIndex, false);
		}
		else
			base.RemoveItemStack(slotId);
	}

	private void ActiveWeapon(int index, bool active) {
		Transform[] trsL = transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(1).GetChild(0).GetChild(1).GetChild(index - 1).GetComponentsInChildren<Transform>(true);
		Transform[] trsR = transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(4).GetChild(0).GetChild(1).GetChild(0).GetChild(1).GetChild(index - 1).GetComponentsInChildren<Transform>(true);
		foreach (Transform trs in trsL)
			if (trs.name.Equals("LARMOR_" + index))
				trs.gameObject.SetActive(active);
		foreach (Transform trs in trsR)
			if (trs.name.Equals("RARMOR_" + index))
				trs.gameObject.SetActive(active);
	}

	public override bool IsValidForSet(int slotId, ItemStack stack) {
		ItemInfo info = ItemManager.FindItemInfo(stack.ItemName);
		return stack == null || (info is ItemWeaponInfo ? (slotId == 0 || slotId == 1 || (slotId >= 6 && slotId < 12)) : true);
	}
}