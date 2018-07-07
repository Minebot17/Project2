using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stamina : MonoBehaviour, IEventProvider {
	public const int MaxProcent = 10100;
	
	private readonly object[] eventHandlers = {
		new EventHandler<SpendStaminaEvent>()
	};

	[SerializeField] 
	private int procent;

	public int Procent {
		get { return procent; }
	}

	[SerializeField] 
	private int regeneration;

	public int Regeneration {
		get { return regeneration; }
		set { regeneration = value; }
	}

	private void FixedUpdate() {
		if (procent < MaxProcent)
			procent += regeneration;
	}

	public int Spend(int procent) {
		SpendStaminaEvent e = GetEventSystem<SpendStaminaEvent>().CallListners(new SpendStaminaEvent(gameObject, procent));
		if (e.IsCancel)
			return 0;
		
		int preProcent = procent;
		procent -= e.Procent;
		if (procent <= 0)
			procent = 0;
		
		return this.procent - preProcent;
	}

	public EventHandler<T> GetEventSystem<T>() where T : EventBase {
		return Utils.FindEventHandler<T>(eventHandlers);
	}
	
	public class SpendStaminaEvent : EventBase {
		public int Procent;
		
		public SpendStaminaEvent(GameObject sender, int procent) : base(sender, true) {
			Procent = procent;
		}
	}
}
