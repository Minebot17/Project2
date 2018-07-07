using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonAttackTask : AbstractTask<SkeletonInfo> {

	public SkeletonAttackTask(GameObject gameObject) : base(gameObject) {
		
	}
	
	public override bool Handle() {
		SkeletonInfo.AttackEvent result = info.GetEventSystem<SkeletonInfo.AttackEvent>()
			.CallListners(new SkeletonInfo.AttackEvent(gameObject, true));

		if (!result.IsCancel) 
			AddAnimation(new HeadAnimationTask(gameObject, 20));

		return !result.IsCancel;
	}

	public override void Tick() {
		
	}
}
