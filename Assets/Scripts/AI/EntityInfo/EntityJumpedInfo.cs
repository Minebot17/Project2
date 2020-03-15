using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityJumpedInfo : EntityMovableInfo {
	public readonly EventHandler<JumpEvent> jumpEvent = new EventHandler<JumpEvent>();
	public float JumpPower;

	/// <summary>
	/// Вызывается когда Entity даётся толчек для прыжка
	/// </summary>
	public class JumpEvent : EventBase {
		public float JumpPower;
		
		public JumpEvent(GameObject sender, float jumpPower) : base(sender, true) {
			JumpPower = jumpPower;
		}
	}
}
