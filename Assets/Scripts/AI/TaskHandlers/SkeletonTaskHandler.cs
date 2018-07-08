using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonTaskHandler : AbstractTaskHandler<SkeletonInfo> {
	
	public EntityState CalmState = new EntityState("Calm", new Type[] {
		typeof(RotateTask),
		typeof(RunForwardTask)
	});
	
	public EntityState AggresiveState = new EntityState("Aggresive", new Type[] {
		typeof(RotateTask),
		typeof(RunToPlayerTask),
		typeof(SkeletonShootTask),
		typeof(SkeletonAttackTask),
		typeof(RunForwardTask)
	});
	
	public EntityState FearState = new EntityState("Fear", new Type[] {
		typeof(RotateTask),
		typeof(RunForwardTask)
	});

	protected override void FixedUpdate() {
		base.FixedUpdate();
		
		if (currentState != FearState && info.Health.Healths < info.ProcentHpToFear * info.Health.MaxHealth.GetCalculated())
			SetState(FearState);
		else if (currentState == FearState && info.Health.Healths > info.ProcentHpToFear * info.Health.MaxHealth.GetCalculated())
			SetState(Utils.IsFreeBetweenPlayer(info.observer.NearestTarget, transform.position)
				? AggresiveState
				: CalmState);
	}

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
