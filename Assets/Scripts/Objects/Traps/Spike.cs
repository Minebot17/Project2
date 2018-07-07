using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour {
	public int Damage;
	
	private void OnCollisionEnter2D(Collision2D other) {
		if (other.gameObject.GetComponent<Health>() != null && other.gameObject.GetComponent<EffectHandler>() != null && !other.gameObject.GetComponent<EffectHandler>().Contains("Invulnerability")) {
			other.gameObject.GetComponent<Health>().Damage(new DamageBase(gameObject, Damage));
			other.gameObject.GetComponent<EffectHandler>().AddEffect(new InvulnerabilityEffect(0.5f));
		}
	}
}
