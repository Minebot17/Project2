using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemInventoryContainer : ItemContainer {
	protected List<GameObject> slots = new List<GameObject>();
	
	public override void OnOpen(IStorage storage) {
		base.OnOpen(storage);
		GetComponent<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
		
		Transform parent = transform.Find("Slots");
		for (int i = 0; i < parent.childCount; i++) {
			ItemSlot slot = parent.GetChild(i).GetComponent<ItemSlot>();
			slots.Add(slot.gameObject);
			if (!storage.IsEmpty(slot.SlotIndex))
				slot.SetStack(storage.GetItemStack(slot.SlotIndex));
		}

		slots = slots.OrderBy(x => x.GetComponent<ItemSlot>().SlotIndex).ToList();
	}

	public override void OnClose() {
		Destroy(gameObject);
		InputManager.DisponseTimers();
	}

	public override GameObject GetSlot(int id) {
		return slots[id];
	}
}
