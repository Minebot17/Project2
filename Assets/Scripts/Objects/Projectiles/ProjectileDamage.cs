using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDamage : MonoBehaviour {

	[SerializeField]
	private GameObject owner;
	
	public int Damage;
	

	public GameObject Owner {
		set { owner = value; }
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.GetComponent<Health>() != null) {
			other.gameObject.GetComponent<Health>().Damage(new DamageProjectile(owner, gameObject, Damage));
			Destroy(transform.parent.gameObject);
		}
	}
}
