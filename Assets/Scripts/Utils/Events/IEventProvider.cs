using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEventProvider {
	EventHandler<T> GetEventSystem<T>() where T : EventBase;
}