using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonAttackTaskSender : AbstractTaskSender<SkeletonTaskHandler, SkeletonInfo> {

	private void FixedUpdate() {
		if (handler.GetCurrentState() == handler.AggresiveState) {
			float playerDistance = Utils.GetDistanceBetweenPlayer(transform.position);
			
			if (playerDistance < info.MaxAttackDistance)
				handler.AddTask(new SkeletonAttackTask(gameObject));
			else if (playerDistance > info.MaxAttackDistance && playerDistance < info.MaxShootDistance)
				handler.AddTask(new SkeletonShootTask(gameObject, Utils.ToVector2(InitScane.instance.Player.transform.position - transform.position).normalized));
		}
	}
}
