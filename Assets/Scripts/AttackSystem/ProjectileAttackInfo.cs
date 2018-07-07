using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileAttackInfo : ScriptableObject {

	public int Index;
	public GameObject Projectile;
	public int TrustForce;

	public string Name {
		set { Projectile = (GameObject) Utils.FindResource("Prefabs/Projectiles", value); }
	}
}
