using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPlayer : NetworkBehaviour {

	[SyncVar] 
	public string NickName;

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
}
