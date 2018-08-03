using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(Rigidbody2D), typeof(VisibleObserver))]
public class SkeletonInfo : EntityJumpedInfo {
	public GameObject Head;
	public GameObject Body;
	public GameObject Projectile;
	public GameObject BoneRender;
	public bool IsAttack;
	public bool IsShoot;
	public float ProcentHpToFear;
	public int MaxAttackDistance;
	public int MinShootDistance;
	public int MaxShootDistance;
	public VisibleObserver observer;

	private Health health;

	public Health Health {
		get { return health; }
	}

	protected override void Start() {
		base.Start();
		observer = GetComponent<VisibleObserver>();
		health = GetComponent<Health>();
		Animator animator = GetComponent<Animator>();
		
		addEvent(new EventHandler<AttackEvent>());
		addEvent(new EventHandler<ShootEvent>());

		GetEventSystem<RunEvent>().SubcribeEvent(x => animator.SetBool("Run", true));
		GetEventSystem<StandEvent>().SubcribeEvent(x => animator.SetBool("Run", false));
		GetEventSystem<FallEvent>().SubcribeEvent(x => animator.SetBool("Fall", true));
		GetEventSystem<LandingEvent>().SubcribeEvent(x => animator.SetBool("Fall", false));
		GetEventSystem<AttackEvent>().SubcribeEvent(x => animator.SetInteger("Attack", x.Begin ? 1 : -1));
		GetEventSystem<ShootEvent>().SubcribeEvent(x => animator.SetInteger("Attack", x.Begin ? 0 : -1));
	}

	public class AttackEvent : EventBase {
		public bool Begin;

		public AttackEvent(GameObject sender, bool begin) : base(sender, begin) {
			Begin = begin;
		}
	}
	
	public class ShootEvent : EventBase {
		public bool Begin;

		public ShootEvent(GameObject sender, bool begin) : base(sender, begin) {
			Begin = begin;
		}
	}
}
