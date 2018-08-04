using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Health : NetworkBehaviour, IEventProvider {

	private readonly object[] eventHandlers = {
		new EventHandler<DamageEvent>(),
		new EventHandler<HealEvent>(),
		new EventHandler<HealthChangeEvent>() 
	};

	[SyncVar(hook = nameof(OnHealthChange))]
	private int healthValue;

	public int HealthValue {
		set { OnHealthChange(value); }
		get { return healthValue; }
	}

	[SyncVar]
	public Attribute MaxHealth = new Attribute("MaxHealth", 100);

	public void OnHealthChange(int newHealth) {
		HealthChangeEvent e = GetEventSystem<HealthChangeEvent>()
			.CallListners(new HealthChangeEvent(gameObject, healthValue, newHealth));
		if (e.IsCancel)
			return;
		healthValue = e.NewHealth;
	}

	public int Heal(int heal) {
		if (!isServer)
			return 0;
			
		HealEvent e = GetEventSystem<HealEvent>().CallListners(new HealEvent(gameObject, heal));
		if (e.IsCancel)
			return 0;

		int preHealth = HealthValue;
		HealthValue += e.Heal;
		if (HealthValue > (int) MaxHealth.GetCalculated())
			HealthValue = (int) MaxHealth.GetCalculated();
		return HealthValue - preHealth;
	}

	public int Damage(DamageBase damage) {
		if (!NetworkManagerCustom.IsServer)
			return 0;
		
		DamageEvent e = GetEventSystem<DamageEvent>().CallListners(new DamageEvent(gameObject, damage));
		if (e.IsCancel)
			return 0;

		int preDamage = HealthValue;
		HealthValue -= e.Damage.Value;
		if (HealthValue <= 0) {
			HealthValue = 0;
			if (GetComponent<IDeath>() != null && !GetComponent<IDeath>().Death(damage)) {
				HealthValue = 1;
				preDamage = 0;
			}
		}
		return HealthValue - preDamage;
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

	public class HealthChangeEvent : EventBase {
		public int OldHealth;
		public int NewHealth;

		public HealthChangeEvent(GameObject sender, int oldHealth, int newHealth) : base(sender, true) {
			OldHealth = oldHealth;
			NewHealth = newHealth;
		}
	}
}
