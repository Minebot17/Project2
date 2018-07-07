using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpineAnimationTask : TemporarilyAnimationTask<SkeletonInfo> {
	
	public SpineAnimationTask(GameObject gameObject, int iterations) : base(gameObject, iterations) { }
	
	public override void Handle() {
		
	}

	public override void Tick() {
		base.Tick();
		Vector2 playerDirection = Utils.ToVector2(InitScane.instance.Player.transform.position - gameObject.transform.position);
		float angle = Mathf.Rad2Deg * Mathf.Atan2(playerDirection.y, Mathf.Abs(playerDirection.x))/2f;
		if (Math.Abs(angle) < 22.5f)
			info.Body.transform.localRotation = Quaternion.Lerp(info.Body.transform.localRotation, Quaternion.Euler(new Vector3(0, 0, angle)), 0.25f);
		else
			End();
	}
	
	protected override void End() {
		base.End();
		Timer.StartNewTimer("SkeletonLeanOverNormalize", 0.02f, 20, gameObject, timer1 => {
			info.Body.transform.localRotation = Quaternion.Lerp(info.Body.transform.localRotation, Quaternion.Euler(new Vector3(0, 0, 0)), 0.25f);
		});
	}

	public override string GetName() {
		return "SpineWatchOnPlayer";
	}
}
