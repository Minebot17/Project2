using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunForwardTask : AbstractTask<EntityMovableInfo> {
	
	protected readonly Collider2D forwardCollider;
	protected readonly Collider2D forwardDownCollider;
	protected readonly Collider2D backCollider;
	protected readonly Collider2D backDownCollider;
	protected readonly Rigidbody2D rigidbody2D;
	protected float distance;

	public RunForwardTask(GameObject gameObject, Collider2D forwardCollider, Collider2D forwardDownCollider, Collider2D backCollider, Collider2D backDownCollider, float distance) : base(gameObject) {
		this.forwardCollider = forwardCollider;
		this.forwardDownCollider = forwardDownCollider;
		this.backCollider = backCollider;
		this.backDownCollider = backDownCollider;
		rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
		this.distance = distance;
	}
	
	public override bool Handle() {
		if (info.BlockControl)
			return false;
		
		bool forwardTouch = Utils.IsTouchRoom(forwardCollider);
		bool downTouch = Utils.IsTouchRoom(forwardDownCollider);
		if (forwardTouch || !downTouch || distance <= 0)
			return false;
		
		EntityMovableInfo.RunEvent result = info.GetEventSystem<EntityMovableInfo.RunEvent>().CallListners(new EntityMovableInfo.RunEvent(gameObject, gameObject.transform.localScale.x > 0 ? 1 : -1, info.RunSpeed));
		if (result.IsCancel)
			return false;
		
		info.IsRunForward = true;
		info.IsRunBack = false;
		return true;
	}

	public override void Tick() {
		bool forwardTouch = Utils.IsTouchRoom(forwardCollider);
		bool downTouch = Utils.IsTouchRoom(forwardDownCollider);
		if (forwardTouch || !downTouch || distance <= 0)
			End();
		else {
			rigidbody2D.velocity = new Vector2((gameObject.transform.localScale.x > 0 ? 1 : -1) * info.RunSpeed, rigidbody2D.velocity.y);
			distance -= info.RunSpeed * 0.02f;
		}
	}
	
	public override void End() {
		base.End();
		info.GetEventSystem<EntityMovableInfo.StandEvent>().CallListners(new EntityMovableInfo.StandEvent(gameObject));
		info.IsRunForward = false;
		info.IsRunBack = false;
	}
}
