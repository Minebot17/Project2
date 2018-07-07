using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulavaEnd : MonoBehaviour {

	public int Damage;
	public int KickPower;
	private Vector2 prePos;
	private Vector2 delta;

	private void Start() {
		prePos = transform.position;
	}

	private void FixedUpdate() {
		delta = Utils.ToVector2(transform.position) - prePos;
		prePos = transform.position;
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.GetComponent<EffectHandler>() == null ||
		    other.gameObject.GetComponent<EffectHandler>().Contains("Invulnerability"))
			return;
		
		if (other.gameObject.GetComponent<Health>() != null)
			other.gameObject.GetComponent<Health>().Damage(new DamageBase(transform.parent.gameObject, Damage));
		if (other.gameObject.GetComponent<Kickable>() != null)
			other.gameObject.GetComponent<Kickable>().Kick(delta * KickPower);

		other.gameObject.GetComponent<EffectHandler>().AddEffect(new InvulnerabilityEffect(0.5f));
	}
}
