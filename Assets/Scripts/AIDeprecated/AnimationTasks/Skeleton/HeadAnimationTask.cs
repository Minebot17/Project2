using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadAnimationTask : TemporarilyAnimationTask<SkeletonInfo> {

	public HeadAnimationTask(GameObject gameObject, int iterations) : base(gameObject, iterations) { }
	
	public override void Handle() {
		
	}

	public override void Tick() {
		base.Tick();
		Vector2 playerDirection = Utils.ToVector2(info.observer.NearestTarget.transform.position - gameObject.transform.position);
		float angle = Mathf.Rad2Deg * Mathf.Atan2(playerDirection.y, Mathf.Abs(playerDirection.x)) - info.Head.transform.eulerAngles.z + info.Head.transform.localEulerAngles.z;
		if (Math.Abs(angle) < 60)
			info.Head.transform.localRotation = Quaternion.Lerp(info.Head.transform.localRotation, Quaternion.Euler(new Vector3(0, 0, angle)), 0.25f);
		else
			End();
	}

	protected override void End() {
		base.End();
		Timer.StartNewTimer("SkeletonHeadNormalize", 0.02f, 10, gameObject, timer1 => {
			info.Head.transform.localRotation = Quaternion.Lerp(info.Head.transform.localRotation, Quaternion.Euler(new Vector3(0, 0, 0)), 0.25f);
		});
	}

	public override string GetName() {
		return "HeadWatchOnPlayer";
	}
}
