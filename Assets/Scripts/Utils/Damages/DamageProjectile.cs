using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageProjectile : DamageBase {
	public GameObject Projectile;

	public DamageProjectile(GameObject sourse, GameObject projectile, int value) : base(sourse, value) {
		Projectile = projectile;
	}
}
