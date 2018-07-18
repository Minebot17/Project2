using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibleObserver : PlayerObserver {
	public Vector3 Offset;

	protected override void Observe() {
		List<GameObject> currentTargets = GameManager.Instance.Players.FindAll(player =>
			Vector3.Distance(player.transform.position, transform.position) < ObserveRadius &&
			Utils.IsFreeBetweenPlayer(player, transform.position + Offset));

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
}
