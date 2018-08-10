using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ItemManager : MonoBehaviour {

	public static List<GameObject> Items = new List<GameObject>();

	public static void Initialize() {
		Items = Resources.LoadAll<GameObject>("Prefabs/Items").ToList();
	}

	public static GameObject FindItem(string itemName) {
		return Items.Find(x => x.GetComponent<ItemInfo>().ItemName.Equals(itemName));
	}
	
	public static ItemInfo FindItemInfo(string itemName) {
		return FindItem(itemName).GetComponent<ItemInfo>();
	}

	public static void DropItemStack(ItemStack stack, Vector3 position, Vector3 force) {
		GameObject stackObject = ObjectsManager.SpawnRoomObject(
			new RoomObject("entityItem", new Vector3(position.x % 495, position.y % 277, position.z), GameManager.rnd.Next(), false, false, 0,
				new[] {stack.ItemName, stack.StackSize + ""}),
			Utils.GetRoomFromPosition(position).transform.Find("Objects"));
		stackObject.GetComponent<Rigidbody2D>().AddForce(force);
		NetworkServer.Spawn(stackObject);
	}
}
