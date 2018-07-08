using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonAttackTaskSender : AbstractTaskSender<SkeletonTaskHandler, SkeletonInfo> {

	private void FixedUpdate() {
		if (handler.GetCurrentState() == handler.AggresiveState) {
			GameObject target = info.observer.NearestTarget;
			float playerDistance = Utils.GetDistanceBetweenPlayer(target, transform.position);
			
			if (playerDistance < info.MaxAttackDistance)
				handler.AddTask(new SkeletonAttackTask(gameObject));
			else if (playerDistance > info.MaxAttackDistance && playerDistance < info.MaxShootDistance)
				handler.AddTask(new SkeletonShootTask(gameObject, Utils.ToVector2(target.transform.position - transform.position).normalized));
		}
	}
}
