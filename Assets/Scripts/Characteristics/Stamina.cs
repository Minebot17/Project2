﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Stamina : NetworkBehaviour {
	
	public readonly EventHandler<SpendStaminaEvent> spendStaminaEvent = new EventHandler<SpendStaminaEvent>();
	
	[SyncVar]
	public int StaminaValue;
	
	[SyncVar]
	public Attribute Regeneration = new Attribute("Regeneration", 100);
	
	[SyncVar]
	public Attribute MaxStamina = new Attribute("MaxHealth", 10100);

	private void FixedUpdate() {
		if (StaminaValue < MaxStamina.GetCalculated())
			StaminaValue += (int) Regeneration.GetCalculated();
	}

	public int Spend(int value) {
		SpendStaminaEvent e = spendStaminaEvent.CallListners(new SpendStaminaEvent(gameObject, value));
		if (e.IsCancel)
			return 0;
		
		int preValue = StaminaValue;
		StaminaValue -= e.Value;
		if (StaminaValue <= 0)
			StaminaValue = 0;
		
		return this.StaminaValue - preValue;
	}

	public class SpendStaminaEvent : EventBase {
		public int Value;
		
		public SpendStaminaEvent(GameObject sender, int value) : base(sender, true) {
			Value = value;
		}
	}
}
