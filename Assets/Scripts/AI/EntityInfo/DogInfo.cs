﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(Rigidbody2D), typeof(VisibleObserver))]
public class DogInfo : EntityJumpedInfo {
	public int MaxAttackDistance;
	public VisibleObserver observer;

	public override void Start() {
		base.Start();
		observer = GetComponent<VisibleObserver>();
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
