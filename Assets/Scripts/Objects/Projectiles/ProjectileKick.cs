using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileKick : MonoBehaviour {

	[SerializeField] 
	private Rigidbody2D kickSource;

	[SerializeField] 
	private float kickPower;

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.GetComponent<Kickable>() != null && !transform.IsChildOf(other.gameObject.transform)) {
			other.gameObject.GetComponent<Kickable>().Kick(kickSource.velocity*kickPower);
			Destroy(transform.parent.gameObject);
		}
	}
}
