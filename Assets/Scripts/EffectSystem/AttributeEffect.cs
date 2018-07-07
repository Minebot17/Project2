using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Эффект, который на время изменяет значение заданной характеристики
/// </summary>
public abstract class AttributeEffect : TimeEffect {
	private Attribute attribute;
	private float value;
	private float procent;

	/// <param name="name">Имя эффекта</param>
	/// <param name="timeLeft">Сколько будет длится эффект. В секундах. -1 - бесконечность</param>
	/// <param name="value">На сколько прибавится значение характеристики</param>
	/// <param name="procent">На сколько прибавится процент характеристики. Формула: (оригинал + значение) * (1 + процент)</param>
	/// <param name="isDisplayed">Показывается ли эффект в GUI</param>
	public AttributeEffect(string name, int timeLeft, EffectHandler handler, bool isDisplayed, float value, float procent) : base(name,
		timeLeft, isDisplayed) {
		// ReSharper disable once VirtualMemberCallInConstructor
		attribute = GetAttribute();
		this.value = value;
		this.procent = procent;
	}

	public override bool OnAdded() {
		attribute.AddEffectValue(value);
		attribute.AddEffectProcent(procent);
		attribute.Calculate(attribute.OriginalValue);
		return true;
	}

	public override bool OnDeleted() {
		attribute.AddEffectValue(-value);
		attribute.AddEffectProcent(-procent);
		attribute.Calculate(attribute.OriginalValue);
		return true;
	}

	public abstract Attribute GetAttribute();
}
