using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogAttackTask : AbstractTask<DogInfo> {

	public DogAttackTask(GameObject gameObject) : base(gameObject) {
		
	}
	
	public override bool Handle() {
		DogInfo.AttackEvent result = info.GetEventSystem<DogInfo.AttackEvent>()
			.CallListners(new DogInfo.AttackEvent(gameObject, true));

		if (!result.IsCancel) {
			Timer.StartNewTimer("AttackSkeletonCD", 0.75f, 1, gameObject,
				timer => {
					info.GetEventSystem<DogInfo.AttackEvent>().CallListners(new DogInfo.AttackEvent(gameObject, false));
					End();
				}, (timer, f0, f1, f2) => {
					gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(-25f * gameObject.transform.localScale.x, 0);
				}
			);
		}

		return !result.IsCancel;
	}

	public override void Tick() {
		
	}
}
