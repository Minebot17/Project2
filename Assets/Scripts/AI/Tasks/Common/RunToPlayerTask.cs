using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunToPlayerTask : RunForwardTask {
	private readonly GameObject player;
	private readonly bool toPlayer;
	
	public RunToPlayerTask(GameObject gameObject, GameObject player, Collider2D forwardCollider, Collider2D forwardDownCollider, Collider2D backCollider, Collider2D backDownCollider,
		float distance, bool toPlayer) : base(gameObject, forwardCollider, forwardDownCollider, backCollider, backDownCollider, distance) {
		this.toPlayer = toPlayer;
		this.player = player;
	}

	public override bool Handle() {
		if (info.BlockControl)
			return false;

		bool rotated = Utils.IsRotatedToPlayer(player, gameObject.transform);
		bool willRotate = rotated ? !toPlayer : toPlayer;
		bool forwardTouch = Utils.IsTouchRoom(forwardCollider);
		bool backTouch = Utils.IsTouchRoom(backCollider);
		bool downTouch = Utils.IsTouchRoom(willRotate ? backDownCollider : forwardDownCollider);
		if (!downTouch || distance <= 0)
			return false;

		if (willRotate) {
			if (backTouch)
				return false;
			
			gameObject.transform.localScale = new Vector3(-gameObject.transform.localScale.x,
				gameObject.transform.localScale.y, gameObject.transform.localScale.z);
		}
		else if (forwardTouch)
			return false;

		EntityMovableInfo.RunEvent result = info.GetEventSystem<EntityMovableInfo.RunEvent>().CallListners(new EntityMovableInfo.RunEvent(gameObject, gameObject.transform.localScale.x > 0 ? 1 : -1, info.RunSpeed));
		if (result.IsCancel)
			return false;

		info.IsRunForward = true;
		info.IsRunBack = false;
		return true;
	}
}
