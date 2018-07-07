using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Banana : MonoBehaviour {
	private void OnTriggerEnter2D(Collider2D other) {
		EffectHandler handler = other.gameObject.GetComponent<EffectHandler>();
		
		if (handler != null) {
			handler.AddEffect(new UnconsciousEffect(float.Parse(GetComponent<SpawnedData>().spawnedData[0])));
			Destroy(gameObject);
		}
	}
}
