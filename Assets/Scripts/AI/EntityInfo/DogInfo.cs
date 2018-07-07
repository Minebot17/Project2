using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogInfo : EntityJumpedInfo {
	public int MaxAttackDistance;
	public Collider2D ForwardCollider;
	public Collider2D ForwarDownCollider;
	public Collider2D BackDownCollider;

	protected override void Start() {
		base.Start();
		Animator animator = GetComponent<Animator>();
		
		addEvent(new EventHandler<AttackEvent>());

		GetEventSystem<RunEvent>().SubcribeEvent(x => animator.SetBool("Run", true));
		GetEventSystem<StandEvent>().SubcribeEvent(x => animator.SetBool("Run", false));
		GetEventSystem<FallEvent>().SubcribeEvent(x => animator.SetBool("Fall", true));
		GetEventSystem<LandingEvent>().SubcribeEvent(x => animator.SetBool("Fall", false));
		GetEventSystem<AttackEvent>().SubcribeEvent(x => animator.SetBool("Attack", x.Begin));
	}
	
	public class AttackEvent : EventBase {
		public bool Begin;

		public AttackEvent(GameObject sender, bool begin) : base(sender, begin) {
			Begin = begin;
		}
	}
}
