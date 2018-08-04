using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class DeathStandart : MonoBehaviour, IDeath, IEventProvider {

	private readonly object[] eventHandlers = {
		new EventHandler<DeathEvent>()
	};

	public bool Death(DamageBase lastDamage) {
		DeathEvent result = GetEventSystem<DeathEvent>().CallListners(new DeathEvent(gameObject, lastDamage));
		
		if (!result.IsCancel)
			OnDeath(lastDamage);
		return !result.IsCancel;
	}

	protected virtual void OnDeath(DamageBase lastDamage) {
		Destroy(gameObject);
	}

	public EventHandler<T> GetEventSystem<T>() where T : EventBase {
		return Utils.FindEventHandler<T>(eventHandlers);
	}
	
	public class DeathEvent : EventBase {

		public DamageBase LastDamage;

		public DeathEvent(GameObject sender, DamageBase lastDamage) : base(sender, true) {
			LastDamage = lastDamage;
		}
	}
}
