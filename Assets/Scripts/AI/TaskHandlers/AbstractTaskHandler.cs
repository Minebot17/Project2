using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Центральный обработчик ITast, присланных из ITaskSender. Так же обрабатывает IAnimationTaks
/// Если активной задачи нет, обработчик получает присланные задачи в данной итерации и выбирает их исходя из заданного IEntityState приоритета
/// Если выбранная задача может выполнятся, то она становится активной, если нет, то дальше по приоритету выбирается другая
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class AbstractTaskHandler<T> : MonoBehaviour, ITaskHandler where T : EntityInfo {
	private readonly Dictionary<Type, byte> priorityMap = new Dictionary<Type, byte>(); 
	private readonly List<ITask> taskBuffer = new List<ITask>();
	private readonly List<IAnimationTask> animationTasks = new List<IAnimationTask>();
	private ITask activeTask = null;
	private ITask prevTask = null;
	protected EntityState currentState;
	protected T info;

	protected virtual void Start() {
		SetState(GetDefaultState());
		info = (T) GetComponent<EntityInfo>();
	}

	protected virtual void FixedUpdate() {
		if (!info.EnableAI)
			return;
		
		if (taskBuffer.Count != 0) {
			taskBuffer.Sort((t0, t1) => priorityMap[t0.GetType()] - priorityMap[t1.GetType()]);

			for (int i = 0; i < taskBuffer.Count; i++) {
				if (taskBuffer[i].Handle()) {
					activeTask = taskBuffer[i];
					break;
				}
			}
			
			taskBuffer.Clear();
		}

		if (activeTask != null) {
			activeTask.Tick();
			
			if (activeTask.IsRemoved()) {
				prevTask = activeTask;
				activeTask = null;
			}
		}

		if (animationTasks.Count != 0) {
			animationTasks.RemoveAll(x => x.IsRemoved());
			animationTasks.ForEach(x => x.Tick());
		}
	}

	/// <summary>
	/// Добавление новой задачи на обработку
	/// </summary>
	/// <param name="task"></param>
	public void AddTask(ITask task) {
		if (activeTask == null)
			taskBuffer.Add(task);
	}

	/// <summary>
	/// Добавить IAnimationTask для обработки. Если такой IAnimationTask уже обрабатывается, то новый не добавляется
	/// </summary>
	public void AddAnimationTask(IAnimationTask animationTask) {
		if (!IsAnimationExists(animationTask.GetName())) {
			animationTasks.Add(animationTask);
			animationTask.Handle();
		}
	}

	/// <summary>
	/// Возвращает последний активный ITask
	/// </summary>
	public ITask GetPrevTask() {
		return prevTask;
	}

	/// <summary>
	/// Возвращает акстивный ITask 
	/// </summary>
	public ITask GetActiveTask() {
		return activeTask;
	}

	private bool IsAnimationExists(string name) {
		foreach (IAnimationTask animationTask in animationTasks)
			if (animationTask.GetName().Equals(name))
				return true;
		return false;
	}

	/// <summary>
	/// Устанавливает указанное состояние
	/// </summary>
	protected void SetState(EntityState state) {
		currentState = state;
		SetPriority(state.priority);
	}

	/// <summary>
	/// Задает приоритеты для выбора задач
	/// </summary>
	/// <param name="priority">Массив из типов ITask</param>
	protected void SetPriority(Type[] priority) {
		priorityMap.Clear();
		for (byte i = 0; i < priority.Length; i++)
			priorityMap.Add(priority[i], i);
	}

	/// <summary>
	/// Возвращает текущее состояние Entity
	/// </summary>
	public EntityState GetCurrentState() {
		return currentState;
	}
	
	/// <summary>
	/// Вызывается при получении эвента от ITaskSender'ов. Тут вы должны реализовать логику замены состояний через SetState
	/// </summary>
	/// <param name="e"></param>
	public abstract void ReceiveEvent(EventBase e);
	
	/// <summary>
	/// Возвращает стандартное состояние вашего Entity
	/// </summary>
	public abstract EntityState GetDefaultState();
}
