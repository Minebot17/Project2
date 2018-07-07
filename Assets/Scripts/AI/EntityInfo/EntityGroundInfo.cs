using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityGroundInfo : EntityInfo, IEventProvider{
	
	// Settings
	public Collider2D GroundTrigger;
	public bool OnGround;
	public float MaxFallVelocity;

	protected Rigidbody2D rigidbody2D;

	protected virtual void Start() {
		rigidbody2D = GetComponent<Rigidbody2D>();
		
		addEvent(new EventHandler<FallEvent>());
		addEvent(new EventHandler<LandingEvent>());
	}

	protected virtual void FixedUpdate() {
		bool oldGroundCheck = OnGround;
		
		OnGround = Utils.IsTouchRoom(GroundTrigger) && rigidbody2D.velocity.y <= 10;
		
		if (!oldGroundCheck && OnGround)
			GetEventSystem<LandingEvent>().CallListners(new LandingEvent(gameObject));
		else if (oldGroundCheck && !OnGround)
			GetEventSystem<FallEvent>().CallListners(new FallEvent(gameObject));

		if (rigidbody2D.velocity.y > MaxFallVelocity)
			rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, MaxFallVelocity);
		else if (rigidbody2D.velocity.y < -MaxFallVelocity)
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
