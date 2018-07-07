using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBase {

	public GameObject Sourse;
	public int Value;
	
	public DamageBase(GameObject sourse, int value) {
		Sourse = sourse;
		Value = value;
	}
}
