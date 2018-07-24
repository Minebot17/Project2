using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObserver : Observer {
	public int ObserveRadius;
	public List<GameObject> Targets = new List<GameObject>();
	public GameObject NearestTarget;

	protected override void Observe() {
		List<GameObject> currentTargets = GameManager.singleton.Players.FindAll(player => Vector3.Distance(player.transform.position, transform.position) < ObserveRadius);

		foreach (GameObject newTarget in currentTargets) {
			if (Targets.Exists(x => x == newTarget))
				CallEvent(new TargetStay(gameObject, newTarget));
			else
				CallEvent(new TargetEnter(gameObject, newTarget));
		}
		
		foreach (GameObject oldTarget in Targets)
			if (oldTarget != null && !currentTargets.Exists(x => x == oldTarget))
				CallEvent(new TargetOut(gameObject, oldTarget));

		if (currentTargets.Count == 0) {
			if (NearestTarget != null) {
				NearestTarget = null;
				CallEvent(new AllTargetsEnd(gameObject));
			}
		}
		else {
			GameObject target = Utils.FindNearestGameObject(currentTargets, transform.position);
			if (NearestTarget == null)
				CallEvent(new FirstTarget(gameObject, target));
			NearestTarget = target;
		}
	}

	protected override object[] GetEvents() {
		return new object[]{
			new EventHandler<TargetEnter>(),
			new EventHandler<TargetStay>(),
			new EventHandler<TargetOut>(),
			new EventHandler<AllTargetsEnd>(), 
		};
	}

	protected override int GetPause() {
		return 50;
	}

	public class TargetEvent : EventBase {
		public GameObject Target;

		public TargetEvent(GameObject sender, GameObject target) : base(sender, false) {
			Target = target;
		}
	}

	public class TargetEnter : TargetEvent {
		public TargetEnter(GameObject sender, GameObject target) : base(sender, target) { }
	}
	
	public class TargetStay : TargetEvent {
		public TargetStay(GameObject sender, GameObject target) : base(sender, target) { }
	}
	
	public class TargetOut : TargetEvent {
		public TargetOut(GameObject sender, GameObject target) : base(sender, target) { }
	}
	
	public class AllTargetsEnd : EventBase {
		public AllTargetsEnd(GameObject sender) : base(sender, false) { }
	}

	public class FirstTarget : TargetEvent {
		public FirstTarget(GameObject sender, GameObject target) : base(sender, target) { }
	}
}
