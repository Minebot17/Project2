using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class DeathDisableColliderHealth : DeathStandart {

	protected override void OnDeath(DamageBase lastDamage) {
		GetComponent<Health>().enabled = false;
		GetComponent<Collider2D>().enabled = false;
	}
}
