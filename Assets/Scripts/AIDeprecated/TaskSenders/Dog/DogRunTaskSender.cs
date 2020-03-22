using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogRunTaskSender : AbstractTaskSender<DogTaskHandler, DogInfo> {

	void Start () {
		base.Start();
		GetComponent<PlayerObserver>().allTargetsEnd.SubcribeEvent(e => {
			handler.ReceiveEvent(e);
		});
		GetComponent<PlayerObserver>().firstTarget.SubcribeEvent(e => {
			handler.ReceiveEvent(e);
		});
	}
	
	private void FixedUpdate() {
		if (handler.GetCurrentState() == handler.CalmState && info.OnGround) {
			int random = GameManager.rnd.Next(20);
			
			if (random == 0 && !(handler.GetPrevTask() is RotateTask))
				handler.AddTask(new RotateTask(gameObject));
			else if (random == 1)
				AddRandomRun();
		}
		else if (handler.GetCurrentState() == handler.AggresiveState) {
			GameObject target = info.observer.NearestTarget;
			if (!Utils.IsFreeBetweenPlayer(target, transform.position))
				AddRandomRun();
			else if (Utils.IsOnEqualsYWithPlayer(target, transform.position.y, 25)) {
				float playerDistance = Utils.GetDistanceBetweenPlayer(target, transform.position);

				if (playerDistance < info.MaxAttackDistance) {
					if (!Utils.IsRotatedToPlayer(target, transform))
						handler.AddTask(new RotateTask(gameObject));
				}
				else
					handler.AddTask(new RunToPlayerTask(gameObject, target, info.ForwardCollider, info.ForwarDownCollider, info.BackCollider, info.BackDownCollider, playerDistance - info.MaxAttackDistance/1.1f, true));

			}
		}
	}
	
	private void AddRandomRun() {
		handler.AddTask(new RunForwardTask(gameObject, info.ForwardCollider, info.ForwarDownCollider, info.BackCollider, info.BackDownCollider, (int) (GameManager.rnd.NextDouble() * 200f)));
	}
}
