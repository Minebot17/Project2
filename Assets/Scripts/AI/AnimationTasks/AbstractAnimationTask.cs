using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Задача анимации данного Entity
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class AbstractAnimationTask<T> : IAnimationTask where T : EntityInfo {
	protected GameObject gameObject;
	protected T info;
	private bool removed;

	public AbstractAnimationTask(GameObject gameObject) {
		this.gameObject = gameObject;
		info = (T) gameObject.GetComponent<EntityInfo>();
	}

	/// <summary>
	/// Вызывается при начале анимации
	/// </summary>
	public abstract void Handle();
	
	/// <summary>
	/// Вызывается каждый тик анимации
	/// </summary>
	public abstract void Tick();
	
	/// <summary>
	/// Получить имя анимации. Это нужно для того, что бы одинаковые анимации не работали одновременно
	/// </summary>
	public abstract string GetName();
	
	/// <summary>
	/// Надо вызывать только тогда, когда хотите, что бы анимация закончилась
	/// </summary>
	protected virtual void End() {
		removed = true;
	}

	public bool IsRemoved() {
		return removed;
	}
}
