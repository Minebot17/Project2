using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Играет роль датчика. Наблюдает за окружением и отсылает эвенты TaskSender'у при каком-либо изменении
/// </summary>
public abstract class Observer : NetworkBehaviour {
	private int timer;

	private void Awake() {
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
	/// Возвращает паузу, между вызовами Observe. Время измеряется в итерациях FixedUpdate 
	/// </summary>
	protected abstract int GetPause();
}
