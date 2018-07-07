using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObserver : Observer {
	public int ObserveRadius;
	protected float prevDistance;

	protected override void Observe() {
		float currentDistance = Vector3.Distance(InitScane.instance.Player.transform.position, transform.position);

		if (prevDistance == 0) {
			if (currentDistance < ObserveRadius)
				CallEvent(new PlayerEnter(gameObject));
		}
		else {
			if (prevDistance > ObserveRadius && currentDistance < ObserveRadius)
				CallEvent(new PlayerEnter(gameObject));
			else if (prevDistance < ObserveRadius && currentDistance > ObserveRadius)
				CallEvent(new PlayerOut(gameObject));
			else if (currentDistance < ObserveRadius)
				CallEvent(new PlayerStay(gameObject));
		}

		prevDistance = currentDistance;
	}

	protected override object[] GetEvents() {
		return new object[]{
			new EventHandler<PlayerEnter>(),
			new EventHandler<PlayerStay>(),
			new EventHandler<PlayerOut>()
		};
	}

	protected override int GetPause() {
		return 50;
	}

	public class PlayerEnter : EventBase {
		public PlayerEnter(GameObject sender) : base(sender, false) { }
	}
	
	public class PlayerStay : EventBase {
		public PlayerStay(GameObject sender) : base(sender, false) { }
	}
	
	public class PlayerOut : EventBase {
		public PlayerOut(GameObject sender) : base(sender, false) { }
	}
}
