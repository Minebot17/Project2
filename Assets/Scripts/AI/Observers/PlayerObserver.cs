using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObserver : Observer {
	public readonly EventHandler<TargetEnter> targetEnter = new EventHandler<TargetEnter>();
	public readonly EventHandler<TargetStay> targetStay = new EventHandler<TargetStay>();
	public readonly EventHandler<TargetOut> targetOut = new EventHandler<TargetOut>();
	public readonly EventHandler<AllTargetsEnd> allTargetsEnd = new EventHandler<AllTargetsEnd>();
	public readonly EventHandler<FirstTarget> firstTarget = new EventHandler<FirstTarget>();
	
	public int ObserveRadius;
	public List<GameObject> Targets = new List<GameObject>();
	public GameObject NearestTarget;

	protected override void Observe() {
		List<GameObject> currentTargets = GameManager.singleton.Players.FindAll(player => Vector3.Distance(player.transform.position, transform.position) < ObserveRadius);

		foreach (GameObject newTarget in currentTargets) {
			if (Targets.Exists(x => x == newTarget))
				targetStay.CallListners(new TargetStay(gameObject, newTarget));
			else
				targetEnter.CallListners(new TargetEnter(gameObject, newTarget));
		}
		
		foreach (GameObject oldTarget in Targets)
			if (oldTarget != null && !currentTargets.Exists(x => x == oldTarget))
				targetOut.CallListners(new TargetOut(gameObject, oldTarget));

		if (currentTargets.Count == 0) {
			if (NearestTarget != null) {
				NearestTarget = null;
				allTargetsEnd.CallListners(new AllTargetsEnd(gameObject));
			}
		}
		else {
			GameObject target = Utils.FindNearestGameObject(currentTargets, transform.position);
			if (NearestTarget == null)
				firstTarget.CallListners(new FirstTarget(gameObject, target));
			NearestTarget = target;
		}
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
