using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDeleteTimer : MonoBehaviour {

	[SerializeField] 
	private GameObject toRemove;
	
	[SerializeField] 
	private float timeOfLife;
	
	private Timer.ITimer timer;
	
	private void Start() {
		timer = Timer.StartNewTimer("ProjectileLifeTimer", timeOfLife, 1, gameObject, timer0 => {
			Destroy(toRemove);
		});
	}

	private void OnDestroy() {
		try {
			timer.Remove();
		}
		catch (Exception ignore) {
			// ignored
		}
	}
}
