using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMovableInfo : EntityGroundInfo {
	public readonly EventHandler<RunEvent> runEvent = new EventHandler<RunEvent>();
	public readonly EventHandler<StandEvent> standEvent = new EventHandler<StandEvent>();

	public Collider2D ForwardCollider;
	public Collider2D ForwarDownCollider;
	public Collider2D BackCollider;
	public Collider2D BackDownCollider;
	public bool IsRunForward;
	public bool IsRunBack;
	public bool BlockControl;
	public float RunSpeed;

	/// <summary>
	/// Вызывается в начале перемещения
	/// </summary>
	public class RunEvent : EventBase {
		public int Direction;
		public float Speed;

		public RunEvent(GameObject sender, int direction, float speed) : base(sender, true) {
			Direction = direction;
			Speed = speed;
		}
	}

	/// <summary>
	/// Вызывается в конце перемещения
	/// </summary>
	public class StandEvent : EventBase {
		public StandEvent(GameObject sender) : base(sender, false) { }
	}
}
