using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMovableInfo : EntityGroundInfo {
	public Collider2D ForwardCollider;
	public Collider2D ForwarDownCollider;
	public Collider2D BackCollider;
	public Collider2D BackDownCollider;
	public bool IsRunForward;
	public bool IsRunBack;
	public bool BlockControl;
	public float RunSpeed;
	
	protected override void Start() {
		base.Start();
		
		addEvent(new EventHandler<RunEvent>());
		addEvent(new EventHandler<StandEvent>());
	}
	
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
