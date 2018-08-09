using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityItemInfo : EntityGroundInfo {
	public ItemInfo ItemInfo;
	public ItemStack Stack;

	public override void Start() {	
		base.Start();
		string[] data = GetComponent<SpawnedData>().spawnedData;
		Stack = new ItemStack(data[0], int.Parse(data[1]));
		Instantiate(ItemManager.FindItem(Stack.ItemName), transform.position, new Quaternion(), transform);
		ItemInfo = transform.GetChild(0).GetComponent<ItemInfo>();
		if (!ItemInfo.SpecialRenderer) {
			ItemInfo.gameObject.transform.localScale = new Vector3(16, 16, 1);
			ItemInfo.GetComponent<MeshFilter>().mesh = GameManager.singleton.OnePlaneCenter;
			ItemInfo.GetComponent<MeshRenderer>().material = new Material(GameManager.singleton.IconMaterial) {mainTexture = ItemInfo.Icon};
			ItemInfo.ColliderSize = new Vector2(16, 16);
		}

		GetComponent<BoxCollider2D>().size = ItemInfo.ColliderSize;
	}
}