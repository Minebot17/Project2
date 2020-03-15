using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateTrigger : MonoBehaviour {
	
	public readonly EventHandler<EnterGateEvent> enterGateEvent = new EventHandler<EnterGateEvent>();
	
	private void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.tag.Equals("Player") && transform.parent.GetComponent<GateObject>().Enable &&
		    !other.gameObject.GetComponent<EffectHandler>().Contains<InvulnerabilityEffect>()
		    ) {
			enterGateEvent.CallListners(new EnterGateEvent(transform.parent.gameObject, other.gameObject));
			other.gameObject.GetComponent<EffectHandler>().AddEffect(new InvulnerabilityEffect(0.25f));
		}
	}

	public class EnterGateEvent : EventBase {
		public GameObject Player;

		public EnterGateEvent(GameObject sender, GameObject player) : base(sender, true) {
			Player = player;
		}
	}
}
