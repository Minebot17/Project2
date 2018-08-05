using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityJumpedInfo : EntityMovableInfo {
	public float JumpPower;
	
	public override void Start() {
		base.Start();
		
		addEvent(new EventHandler<JumpEvent>());
	}
	
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
