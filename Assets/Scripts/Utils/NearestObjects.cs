﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NearestObjects : MonoBehaviour, IEventProvider {
	
	private readonly object[] eventHandlers = {
		new EventHandler<OnNearestEnter>(),
		new EventHandler<OnNearestExit>()
	};

	public List<Collider2D> NearestColliders = new List<Collider2D>();

	private void Start() {
		Collider2D[] colliders = GetComponents<Collider2D>();
		foreach (Collider2D collider in colliders) {
			if (collider.isTrigger) {
				Collider2D[] overlaped = new Collider2D[100];
				int count = Physics2D.OverlapCollider(collider, new ContactFilter2D(), overlaped);
				for (int i = 0; i < count; i++)
					if (!overlaped[i].gameObject.transform.IsChildOf(transform))
						NearestColliders.Add(overlaped[i]);
				break;
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D other) {
		NearestColliders.Add(other);

		GetEventSystem<OnNearestEnter>().CallListners(new OnNearestEnter(gameObject, other));
	}
	private void OnTriggerExit2D(Collider2D other) {
		NearestColliders.Remove(other);
		
		GetEventSystem<OnNearestExit>().CallListners(new OnNearestExit(gameObject, other));
	}

	public class OnNearestEnter : EventBase {
     		public Collider2D Collider;
     
     		public OnNearestEnter(GameObject sender, Collider2D collider) : base(sender, false) {
     			Collider = collider;
     		}
    }
	
	public class OnNearestExit : EventBase {
		public Collider2D Collider;

		public OnNearestExit(GameObject sender, Collider2D collider) : base(sender, false) {
			Collider = collider;
		}
	}
	public EventHandler<T> GetEventSystem<T>() where T : EventBase {
		return Utils.FindEventHandler<T>(eventHandlers);
	}
}
