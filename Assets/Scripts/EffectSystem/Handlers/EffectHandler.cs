using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Обработчик системы эффектов. Должен быть на объекте, если он хочет иметь на себе эффекты
/// </summary>
public class EffectHandler : NetworkBehaviour, IEventProvider {
	
	private readonly object[] eventHandlers = {
		new EventHandler<AddEffectEvent>()
	};
	
	/// <summary>
	/// Контейнер активных эффектов
	/// </summary>
	[SerializeField]
	protected List<TimeEffect> timeEffects = new List<TimeEffect>();
	
	protected virtual void FixedUpdate () {
		timeEffects.RemoveAll(x => {
			bool result = x.IsRemoved || (x.TimeLeft < 0 && x.TimeLeft != -1);
			if (result) {
				bool onDelete = x.OnDeleted();
				if (!onDelete) {
					x.IsRemoved = false;
				}

				return onDelete;
			}

			return false;
		});
		foreach (TimeEffect effect in timeEffects) {
			effect.OnUpdate();
			effect.TimeLeft -= Time.deltaTime;
		}
	}

	private void OnDestroy() {
		foreach (TimeEffect effect in timeEffects)
			effect.OnDeleted();
	}

	/// <summary>
	/// Добавить новый эффект к объекту
	/// </summary>
	/// <param name="effect">Эффект</param>
	/// <returns>Добавился ли эффект?</returns>
	public virtual bool AddEffect(TimeEffect effect) {
		TimeEffect activeEqualsEffect = timeEffects.Find(x => x.Name.Equals(effect.Name));

		LeveledEffect leveledEffect = activeEqualsEffect as LeveledEffect;
		if (leveledEffect != null && leveledEffect.Stackable)
			leveledEffect.AddLevel(((LeveledEffect)effect).Level);
		else if (activeEqualsEffect != null)
			return false;

		effect.Handler = this;
		bool result = effect.OnAdded() && !GetEventSystem<AddEffectEvent>().CallListners(new AddEffectEvent(gameObject, effect)).IsCancel;
		if (result)
			timeEffects.Add(effect);
		return result;
	}

	/// <summary>
	/// Содержит ли контейнер в себе эффект с заданным именем
	/// </summary>
	public bool Contains(string name) {
		return timeEffects.Exists(x => x.Name.Equals(name));
	}
	
	/// <summary>
	/// Содержит ли контейнер в себе эффект с заданным типом
	/// </summary>
	public bool Contains<T>() {
		return timeEffects.Exists(x => x is T);
	}
	
	/// <summary>
	/// Удалить эффект с заданным именем
	/// </summary>
	public void Remove(string name) {
		timeEffects.RemoveAll(x => x.Name.Equals(name));
	}

	/// <summary>
	/// Удалить эффект с заданным типом
	/// </summary>
	public void Remove<T>() {
		timeEffects.RemoveAll(x => x is T);
	}

	/// <summary>
	/// Удалить все активные эффекты с объекта
	/// </summary>
	public void RemoveAll() {
		timeEffects.Clear();
	}

	public class AddEffectEvent : EventBase {
		public TimeEffect Effect;

		public AddEffectEvent(GameObject sender, TimeEffect effect) : base(sender, true) {
			Effect = effect;
		}
	}

	public EventHandler<T> GetEventSystem<T>() where T : EventBase {
		return Utils.FindEventHandler<T>(eventHandlers);
	}
}
