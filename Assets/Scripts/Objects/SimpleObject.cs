using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SimpleObject : NetworkBehaviour {
	[SyncVar]
	[SerializeField]
	private int id;

	public bool NotMirrorChildrensOnSpawn;

	public int Id {
		get { return id; }
	}

	public void Initialize() {
		id = InitScane.rnd.Next();
	}
}
