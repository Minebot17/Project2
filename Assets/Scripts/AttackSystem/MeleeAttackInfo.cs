using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MeleeAttackInfo : ScriptableObject {

	public int Index;
	public int Damage;
	public Vector2 Kick;
	public Vector2 Point;
	public Vector2 Size;
	public bool Splash;
}
