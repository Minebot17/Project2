using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class GameProfile : NetworkBehaviour, ISerializableObject {

	private const float UseItemDistace = 100;
	[SyncVar(hook = nameof(OnNameChange))] 
	public string ProfileName;
	public int Level;
	//public List<string> Inventory = new List<string>();

	private void Awake() {
		GameManager.singleton.Players.Add(gameObject);
	}

	private void Start() {
		GetComponent<Health>().healthChangeEvent.SubcribeEvent(e => {
			TextMesh mesh = transform.Find("NameRender").GetComponent<TextMesh>();
			if (GetComponent<Health>().MaxHealth.GetCalculated() == 0)
				return;
			transform.Find("HealthRener").localScale = new Vector3((e.NewHealth / GetComponent<Health>().MaxHealth.GetCalculated()) * 30f, 1, 1);
		});
		
		if (!isLocalPlayer)
			return;
		Utils.SetLocalPlayer(gameObject);
		GameManager.singleton.PlayerHead =
			transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(1).gameObject;
		if (GameManager.singleton.doStartForce)
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 15000)); // tODO;
	}

	private void FixedUpdate() {
		if (!isLocalPlayer)
			return;
		
		if (GameSettings.SettingOpenInventoryKey.IsDown()) {
			if (!ContainerManager.IsOpen())
				ContainerManager.OpenContainer(GameManager.singleton.PlayerInventoryPrefab, GetComponent<IStorage>());
			else if (ContainerManager.IsOpen("Inventory"))
				ContainerManager.CloseContainer();
		}
		
		if (ContainerManager.IsOpen())
			return;
		
		if (GameSettings.SettingUseItemKey.IsDown()) {
			GameObject item = Utils.GetEntityItemOverMouse();
			if (!item)
				return;
			
			float distance = Vector3.Distance(item.transform.position, transform.position);
			if (distance > UseItemDistace)
				return;
			
			RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position,
				(item.transform.position - transform.position).normalized,
				distance, LayerMask.GetMask("Room"));
			if (hit.Any(x => x.collider.name.Equals("Wall")))
				return;
			
			bool success = GetComponent<IStorage>().AddItemStack(item.GetComponent<EntityItemInfo>().Stack);
			if (success)
				MessageManager.DestroyServerMessage.SendToServer(new NetworkIdentityMessage(item.GetComponent<NetworkIdentity>()));
		}
	}

	public void Initialize() {
		
	}

	public List<string> Serialize() {
		List<string> result = new List<string>();
		result.Add(ProfileName);
		result.Add(Level+"");
		//result.AddRange(Inventory);
		//result.Add("endInventory");

		return result;
	}

	public int Deserialize(List<string> data) {
		bool inGo;
		try {
			Vector3 a = transform.position;
			inGo = true;
		}
		catch (NullReferenceException e) {
			inGo = false;
		}
		ProfileName = data[0];
		Level = int.Parse(data[1]);
		/*Inventory = new List<string>();
		int count = 0;
		for (int i = 2; i < data.Count; i++) {
			count++;
			if (data[i].Equals("endInventory"))
				break;
			Inventory.Add(data[i]);
		}*/

		if (inGo) {
			transform.Find("NameRender").GetComponent<TextMesh>().text = ProfileName;
		}

		return 2/* + count*/;
	}

	public void OnNameChange(string name) {
		bool inGo;
		try {
			Vector3 a = transform.position;
			inGo = true;
		}
		catch (NullReferenceException e) {
			inGo = false;
		}

		if (inGo) {
			transform.Find("NameRender").GetComponent<TextMesh>().text = name;
		}
	}
}
