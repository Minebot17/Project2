using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogAttackTaskSender : AbstractTaskSender<DogTaskHandler, DogInfo> {

	private void FixedUpdate() {
		if (handler.GetCurrentState() == handler.AggresiveState) {
			float playerDistance = Utils.GetDistanceBetweenPlayer(transform.position);
			
			if (playerDistance < info.MaxAttackDistance)
				handler.AddTask(new DogAttackTask(gameObject));
		}
	}
}
