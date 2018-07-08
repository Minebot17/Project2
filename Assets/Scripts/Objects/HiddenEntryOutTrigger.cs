using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenEntryOutTrigger : MonoBehaviour {
	private GameObject hiddenWall;

	private void Start() {
		hiddenWall = transform.parent.parent.parent.Find("Meshes").Find("hiddenWall").gameObject;
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.tag.Equals("Player"))
			hiddenWall.SetActive(true);
	}
}
