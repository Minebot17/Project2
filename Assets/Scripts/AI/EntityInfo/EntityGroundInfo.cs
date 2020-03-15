using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityGroundInfo : EntityInfo {

	public readonly EventHandler<FallEvent> fallEvent = new EventHandler<FallEvent>();
	public readonly EventHandler<LandingEvent> landingEvent = new EventHandler<LandingEvent>();
	
	// Settings
	public Collider2D GroundTrigger;
	public bool OnGround;
	public float MaxFallVelocity;

	protected Rigidbody2D rigidbody2D;
	
	public override void Start() {	
		base.Start();
		rigidbody2D = GetComponent<Rigidbody2D>();
	}

	protected virtual void FixedUpdate() {
		bool oldGroundCheck = OnGround;
		
		OnGround = Utils.IsTouchRoom(GroundTrigger) && rigidbody2D.velocity.y <= 10;
		
		if (!oldGroundCheck && OnGround)
			landingEvent.CallListners(new LandingEvent(gameObject));
		else if (oldGroundCheck && !OnGround)
			fallEvent.CallListners(new FallEvent(gameObject));

		/*if (rigidbody2D.velocity.y > MaxFallVelocity)
			rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, MaxFallVelocity);*/
		if (rigidbody2D.velocity.y < -MaxFallVelocity)
			rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, -MaxFallVelocity);
	}

	/// <summary>
	/// Вызывается когда Entity перестает быть на земле
	/// </summary>
	public class FallEvent : EventBase {
		public FallEvent(GameObject sender) : base(sender, false) { }
	}

	/// <summary>
	/// Вызывается когда Entity приземляется на землю
	/// </summary>
	public class LandingEvent : EventBase {
		public LandingEvent(GameObject sender) : base(sender, false) { }
	}
}
