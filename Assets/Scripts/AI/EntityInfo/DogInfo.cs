using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(Rigidbody2D), typeof(VisibleObserver))]
public class DogInfo : EntityJumpedInfo {
	public readonly EventHandler<AttackEvent> attackEvent = new EventHandler<AttackEvent>();
	public int MaxAttackDistance;
	public VisibleObserver observer;

	public override void Start() {
		base.Start();
		observer = GetComponent<VisibleObserver>();
		Animator animator = GetComponent<Animator>();

		runEvent.SubcribeEvent(x => animator.SetBool("Run", true));
		standEvent.SubcribeEvent(x => animator.SetBool("Run", false));
		fallEvent.SubcribeEvent(x => animator.SetBool("Fall", true));
		landingEvent.SubcribeEvent(x => animator.SetBool("Fall", false));
		attackEvent.SubcribeEvent(x => animator.SetBool("Attack", x.Begin));
	}
	
	public class AttackEvent : EventBase {
		public bool Begin;

		public AttackEvent(GameObject sender, bool begin) : base(sender, begin) {
			Begin = begin;
		}
	}
}
