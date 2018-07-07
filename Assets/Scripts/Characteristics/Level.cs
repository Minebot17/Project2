using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour, IEventProvider { // Уровень != опыт. Опыт - отдельный компонент

	private readonly object[] eventHandlers = {
		new EventHandler<LevelUpEvent>()
	};

	[SerializeField] 
	private int level;

	public int Levels {
		get { return level; }
		set { level = value; }
	}

	public bool LevelUp() {
		LevelUpEvent e = GetEventSystem<LevelUpEvent>().CallListners(new LevelUpEvent(gameObject, level));
		if (!e.IsCancel)
			level++;
		return e.IsCancel;
	}
	
	public EventHandler<T> GetEventSystem<T>() where T : EventBase {
		return Utils.FindEventHandler<T>(eventHandlers);
	}
	
	public class LevelUpEvent : EventBase {
		public int Level; // Уровень до повышения

		public LevelUpEvent(GameObject sender, int level) : base(sender, true) {
			Level = level;
		}
	}
}
