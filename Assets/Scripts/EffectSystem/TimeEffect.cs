using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TimeEffect {

	public string Name;
	public float TimeLeft;
	public EffectHandler Handler;
	public bool IsDisplayed;
	public bool IsRemoved;

	/// <param name="name">Имя эффекта</param>
	/// <param name="timeLeft">Сколько будет длится эффект. В секундах. -1 - бесконечность</param>
	/// <param name="isDisplayed">Показывается ли эффект в GUI</param>
	public TimeEffect(string name, float timeLeft, bool isDisplayed) {
		Name = name;
		TimeLeft = timeLeft;
		IsDisplayed = isDisplayed;
	}

	/// <summary>
	/// Вызывается при добавлении эффекта к объекту
	/// </summary>
	/// <returns>Добавить ли эффект?</returns>
	public abstract bool OnAdded();
	
	/// <summary>
	/// Вызывается каждое обновление fixedupdate пока активен эффект
	/// </summary>
	public abstract void OnUpdate();
	
	/// <summary>
	/// Вызывается когда время эффекта вышло или же он захотел удалить себя. Если эффект удалил себя и вернул false, то желание удалится сбрасывается
	/// </summary>
	/// <returns>Удалить ли эффект?</returns>
	public abstract bool OnDeleted();

	/// <summary>
	/// Пометить эффект для удаления
	/// </summary>
	public void Remove() {
		IsRemoved = true;
	}
}
