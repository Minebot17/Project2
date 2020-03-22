using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogTaskHandler : AbstractTaskHandler<DogInfo> {

	public EntityState CalmState = new EntityState("Calm", new Type[] {
		typeof(RotateTask),
		typeof(RunForwardTask)
	});
	
	public EntityState AggresiveState = new EntityState("Aggresive", new Type[] {
		typeof(RotateTask),
		typeof(RunToPlayerTask),
		typeof(DogAttackTask),
		typeof(RunForwardTask)
	});

	public override void ReceiveEvent(EventBase e) {
		if (e is PlayerObserver.AllTargetsEnd && currentState == AggresiveState)
			SetState(CalmState);
		else if (e is PlayerObserver.FirstTarget && currentState == CalmState)
			SetState(AggresiveState);
	}

	public override EntityState GetDefaultState() {
		return CalmState;
	}
}
