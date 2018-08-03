using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Хранилище параметров вашего Entity. Их можно задать в редакторе
/// Так же от сюда вы можете брать/записывать информацию из любого места архитектуры. Информация может быть модифицированная эффектами TODO проперти вместо переменных для эффектов
/// Отсюда происходит регулирование анимаций, путем поимки эвентов. Например так GetEventSystem(RunEvent)().SubcribeEvent(x => animator.SetBool("Run", true));
/// </summary>
public class EntityInfo : SimpleObject, IEventProvider {
	protected readonly List<object> eventHandlers = new List<object>();
	
	/// <summary>
	/// Если AI выключенно, то TaskHandler не будет обрабатывать задачи
	/// </summary>
	public bool EnableAI = true;
	
	void Start () {
		Initialize();
		if (!NetworkManagerCustom.IsServer) {
			foreach (var c in GetComponents<Observer>())
				c.enabled = false;
			GetComponent<AbstractTaskHandler<EntityInfo>>().enabled = false;
			foreach (var c in GetComponents<AbstractTaskSender<AbstractTaskHandler<EntityInfo>, EntityInfo>>())
				c.enabled = false;

			GetComponent<EntityInfo>().enabled = false;
		}
	}
	
	/// <summary>
	/// Добавить связанный с Entity эвент
	/// </summary>
	/// <param name="eventHandler">EventHandler вашего эвента</param>
	public void addEvent(object eventHandler) {
		eventHandlers.Add(eventHandler);
	}

	public EventHandler<T> GetEventSystem<T>() where T : EventBase {
		return Utils.FindEventHandler<T>(eventHandlers.ToArray());
	}
}
