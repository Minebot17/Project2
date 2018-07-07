using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nickname : MonoBehaviour {

	[SerializeField] 
	private string nick;

	public string Nick {
		get { return nick; }
		set { nick = value; }
	}
}
