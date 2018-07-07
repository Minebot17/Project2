using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonRunTaskSender : AbstractTaskSender<SkeletonTaskHandler, SkeletonInfo> {

	void Start () {
		base.Start();
		GetComponent<PlayerObserver>().GetEventSystem<PlayerObserver.PlayerEnter>().SubcribeEvent(e => {
			handler.ReceiveEvent(e);
		});
		GetComponent<PlayerObserver>().GetEventSystem<PlayerObserver.PlayerOut>().SubcribeEvent(e => {
			handler.ReceiveEvent(e);
		});
	}

	private void FixedUpdate() {
		if (handler.GetCurrentState() == handler.CalmState && info.OnGround) {
			int random = InitScane.rnd.Next(20);
			
			if (random == 0 && !(handler.GetPrevTask() is RotateTask))
				handler.AddTask(new RotateTask(gameObject));
			else if (random == 1)
				AddRandomRun();
		}
		else if (handler.GetCurrentState() == handler.FearState) {
			handler.AddTask(new RunToPlayerTask(gameObject, info.ForwardCollider, info.ForwarDownCollider, info.BackDownCollider, 50, false));
		}
		else if (handler.GetCurrentState() == handler.AggresiveState) {
			if (!Utils.IsFreeBetweenPlayer(transform.position))
				AddRandomRun();
			else if (Utils.IsOnEqualsYWithPlayer(transform.position.y, 25)) {
				float playerDistance = Utils.GetDistanceBetweenPlayer(transform.position);

				if (playerDistance < info.MaxAttackDistance) {
					if (!Utils.IsRotatedToPlayer(transform))
						handler.AddTask(new RotateTask(gameObject));
				}
				else if (playerDistance > info.MaxAttackDistance && playerDistance < info.MinShootDistance) {
					handler.AddTask(new RunToPlayerTask(gameObject, info.ForwardCollider, info.ForwarDownCollider, info.BackDownCollider, (info.MaxShootDistance + info.MinShootDistance)/2f - playerDistance, false));
				}
				else if (playerDistance > info.MinShootDistance && playerDistance < info.MaxShootDistance) {
					if (!Utils.IsRotatedToPlayer(transform))
						handler.AddTask(new RotateTask(gameObject));
				}
				else
					handler.AddTask(new RunToPlayerTask(gameObject, info.ForwardCollider, info.ForwarDownCollider, info.BackDownCollider, playerDistance - (info.MaxShootDistance + info.MinShootDistance)/2f, true));

			}
		}
	}

	private void AddRandomRun() {
		handler.AddTask(new RunForwardTask(gameObject, info.ForwardCollider, info.ForwarDownCollider, (int) (InitScane.rnd.NextDouble() * 200f)));
	}
}
