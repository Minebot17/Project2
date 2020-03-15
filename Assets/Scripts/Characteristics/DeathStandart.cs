using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class DeathStandart : MonoBehaviour, IDeath {

	public readonly EventHandler<DeathEvent> deathEvent = new EventHandler<DeathEvent>();

	public bool Death(DamageBase lastDamage) {
		DeathEvent result = deathEvent.CallListners(new DeathEvent(gameObject, lastDamage));
		
		if (!result.IsCancel)
			OnDeath(lastDamage);
		return !result.IsCancel;
	}

	protected virtual void OnDeath(DamageBase lastDamage) {
		Destroy(gameObject);
	}

	public class DeathEvent : EventBase {

		public DamageBase LastDamage;

		public DeathEvent(GameObject sender, DamageBase lastDamage) : base(sender, true) {
			LastDamage = lastDamage;
		}
	}
}
