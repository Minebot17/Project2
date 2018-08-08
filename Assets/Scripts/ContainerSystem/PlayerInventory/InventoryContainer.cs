using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryContainer : ItemContainer {
	
	public override void OnOpen(List<string> data) {
		GetComponent<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
	}

	public override List<string> OnClose() {
		Destroy(gameObject);
		InputManager.DisponseTimers();
		return null;
	}
}
