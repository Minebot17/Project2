using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameProfile : NetworkBehaviour {

	[SyncVar] 
	public string ProfileName;

	private void Awake() {
		GameManager.singleton.Players.Add(gameObject);
	}

	private void Start() {
		if (!isLocalPlayer)
			return;
		Utils.SetLocalPlayer(gameObject);
		if (GameManager.singleton.doStartForce)
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 30000)); // tODO;
	}

	public string Serialize() {
		bool inGo;
		try {
			Vector3 a = transform.position;
			inGo = true;
		}
		catch (NullReferenceException e) {
			inGo = false;
		}
		List<string> list = new List<string>();
		list.Add(ProfileName);
		if (inGo) {
			transform.Find("NameRender").GetComponent<TextMesh>().text = ProfileName;
		}
		string result = "";
		foreach (string data in list) {
			result += data + ";";
		}

		return result;
	}

	public void Deserialize(string data) {
		string[] list = data.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
		ProfileName = list[0];
	}
}
