using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameProfile : NetworkBehaviour {

	[SyncVar] 
	public string ProfileName;

	private void Awake() {
		GameManager.Instance.Players.Add(gameObject);
	}

	private void Start() {
		if (!isLocalPlayer)
			return;
		Utils.SetLocalPlayer(gameObject);
		if (GenerationManager.currentGeneration != null)
			GenerationManager.TeleportPlayerToStart(gameObject);
		if (GameManager.Instance.doStartForce)
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 30000)); // tODO;
	}

	public string Serialize() {
		bool notInGO = false;
		try {
			Transform t = transform;
		}
		catch (NullReferenceException e) {
			notInGO = true;
		}

		List<string> list = new List<string>();
		list.Add(ProfileName);
		list.Add(notInGO ? "0" : transform.position.x+"");
		list.Add(notInGO ? "0" : transform.position.y+"");
		list.Add(notInGO ? "0" : transform.position.z+"");
		string result = "";
		foreach (string data in list) {
			result += data + ";";
		}

		return result;
	}

	public void Deserialize(string data) {
		bool notInGO = false;
		try {
			Transform t = transform;
		}
		catch (NullReferenceException e) {
			notInGO = true;
		}
		string[] list = data.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
		ProfileName = list[0];
		if (!notInGO)
			transform.position = new Vector3(float.Parse(list[1]), float.Parse(list[2]), float.Parse(list[3]));
	}
}
