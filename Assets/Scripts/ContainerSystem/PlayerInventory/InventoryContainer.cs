using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryContainer : ItemContainer {
	
	public override void OnOpen(IStorage storage) {
		GetComponent<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
		// spawn and initialize slots
	}

	public override void OnClose() {
		Destroy(gameObject);
		InputManager.DisponseTimers();
	}
}
