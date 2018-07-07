using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateTrigger : MonoBehaviour, IEventProvider {

	private readonly object[] eventHandlers = {
		new EventHandler<EnterGateEvent>()
	};
	
	private void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.name.Equals("Player") && transform.parent.GetComponent<GateObject>().Enable &&
		    !other.gameObject.GetComponent<EffectHandler>().Contains<InvulnerabilityEffect>()
		    ) {
			GetEventSystem<EnterGateEvent>().CallListners(new EnterGateEvent(gameObject));
			other.gameObject.GetComponent<EffectHandler>().AddEffect(new InvulnerabilityEffect(2f));
		}
	}
	
	public EventHandler<T> GetEventSystem<T>() where T : EventBase {
		return Utils.FindEventHandler<T>(eventHandlers);
	}
	
	public class EnterGateEvent : EventBase {
		public EnterGateEvent(GameObject sender) : base(sender, true) { } // TODO call this event
	}
}
