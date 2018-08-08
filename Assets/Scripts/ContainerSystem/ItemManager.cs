using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour {

	public static List<GameObject> Items = new List<GameObject>();

	public static void Initialize() {
		Items = Resources.LoadAll<GameObject>("Prefabs/Items").ToList();
	}

	public static GameObject FindItem(string itemName) {
		return Items.Find(x => x.GetComponent<ItemInfo>().ItemName.Equals(itemName));
	}
}
