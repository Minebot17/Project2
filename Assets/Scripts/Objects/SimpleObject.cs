using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleObject : MonoBehaviour {
	[SerializeField]
	private int id;

	public bool NotMirrorChildrensOnSpawn;

	public int Id {
		get { return id; }
		set { id = value; }
	}
	
	public void Initialize() {
		id = GameManager.rnd.Next();
	}
}