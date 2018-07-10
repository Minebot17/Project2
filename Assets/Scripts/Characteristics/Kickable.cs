using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody2D))]
public class Kickable : NetworkBehaviour, IEventProvider {
	
	private readonly object[] eventHandlers = {
		new EventHandler<KickEvent>()
	};

	public virtual bool Kick(Vector2 vector) {
		KickEvent result = GetEventSystem<KickEvent>().CallListners(new KickEvent(gameObject, vector));
		if (result.IsCancel || result.Vector.Equals(Vector2.zero))
			return false;

		GetComponent<Rigidbody2D>().AddForce(vector);
		return true;
	}

	public EventHandler<T> GetEventSystem<T>() where T : EventBase {
		return Utils.FindEventHandler<T>(eventHandlers);
	}

	public class KickEvent : EventBase {
		public Vector2 Vector;

		public KickEvent(GameObject sender, Vector2 vector) : base(sender, true) {
			Vector = vector;
		}
	}
}
