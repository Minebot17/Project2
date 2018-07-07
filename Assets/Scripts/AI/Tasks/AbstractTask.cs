using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Реализация задачи, отправляемая ITaskSender'ом ITaskHandler'у для её обработки
/// </summary>
public abstract class AbstractTask<T> : ITask where T : EntityInfo {
	protected GameObject gameObject;
	protected T info;
	private bool removed;
	
	public AbstractTask(GameObject gameObject) {
		this.gameObject = gameObject;
		info = (T) gameObject.GetComponent<EntityInfo>();
	}

	/// <summary>
	/// Инициализация задачи. Проверка её возможности выполнения
	/// </summary>
	/// <returns>Возможно ли выполнить задачу в данный момент</returns>
	public abstract bool Handle();
	
	/// <summary>
	/// Вызывается каждый тик при выполнение задачи
	/// </summary>
	public abstract void Tick();

	/// <summary>
	/// Добавление анимационной задачи
	/// </summary>
	public void AddAnimation(IAnimationTask animationTask) {
		gameObject.GetComponent<ITaskHandler>().AddAnimationTask(animationTask);
	}

	/// <summary>
	/// Окончание данной задачи
	/// </summary>
	protected virtual void End() {
		removed = true;
	}

	public bool IsRemoved() {
		return removed;
	}
}
