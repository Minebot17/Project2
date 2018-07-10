using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Health : NetworkBehaviour, IEventProvider {

	private readonly object[] eventHandlers = {
		new EventHandler<DamageEvent>(),
		new EventHandler<HealEvent>()
	};

	[SyncVar]
	[SerializeField]
	private int health;

	public int Healths {
		get { return health; }
	}

	[SyncVar]
	public Attribute MaxHealth = new Attribute("MaxHealth", 100);

	public int Heal(int heal) {
		if (!isServer)
			return 0;
			
		HealEvent e = GetEventSystem<HealEvent>().CallListners(new HealEvent(gameObject, heal));
		if (e.IsCancel)
			return 0;

		int preHealth = health;
		health += e.Heal;
		if (health > (int) MaxHealth.GetCalculated())
			health = (int) MaxHealth.GetCalculated();
		return health - preHealth;
	}

	public int Damage(DamageBase damage) {
		if (!isServer)
			return 0;
		
		DamageEvent e = GetEventSystem<DamageEvent>().CallListners(new DamageEvent(gameObject, damage));
		if (e.IsCancel)
			return 0;

		int preDamage = health;
		health -= e.Damage.Value;
		if (health <= 0) {
			health = 0;
			if (GetComponent<IDeath>() != null && !GetComponent<IDeath>().Death(damage)) {
				health = 1;
				preDamage = 0;
			}
		}
		return health - preDamage;
	}

	public EventHandler<T> GetEventSystem<T>() where T : EventBase {
		return Utils.FindEventHandler<T>(eventHandlers);
	}

	public class HealEvent : EventBase {
		public int Heal;
		
		public HealEvent(GameObject sender, int heal) : base(sender, true) {
			Heal = heal;
		}
	}

	public class DamageEvent : EventBase {
		public DamageBase Damage;
		
		public DamageEvent(GameObject sender, DamageBase damage) : base(sender, true) {
			Damage = damage;
		}
	}
}
