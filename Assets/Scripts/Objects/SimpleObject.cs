using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleObject : MonoBehaviour {
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
