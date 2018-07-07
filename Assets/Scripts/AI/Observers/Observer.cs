using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Играет роль датчика. Наблюдает за окружением и отсылает эвенты TaskSender'у при каком-либо изменении
/// </summary>
public abstract class Observer : MonoBehaviour, IEventProvider {
	private object[] events;
	private int timer;

	private void Awake() {
		events = GetEvents();
		timer = GetPause();
	}

	private void FixedUpdate() {
		if (timer <= 0) { 
			Observe();
			timer = GetPause();
		}
		else timer--;
	}

	/// <summary>
	/// Вызывается каждый раз через заданный период в GetPause
	/// </summary>
	protected abstract void Observe();
	
	/// <summary>
	/// Возвращает все возможные эвенты, прикрепленные к данному Observer
	/// </summary>
	protected abstract object[] GetEvents();
	
	/// <summary>
	/// Возвращает паузу, между вызовами Observe. Время измеряется в итерациях FixedUpdate 
	/// </summary>
	protected abstract int GetPause();
	
	/// <summary>
	/// Оповещает всех подписчиков данного эвента
	/// </summary>
	public void CallEvent<T>(T e) where T : EventBase {
		GetEventSystem<T>().CallListners(e);
	}

	public EventHandler<T> GetEventSystem<T>() where T : EventBase {
		return Utils.FindEventHandler<T>(events);
	}
}
