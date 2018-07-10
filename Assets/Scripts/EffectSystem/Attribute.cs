using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Аттрибут - изменяемая эффектами или другим кодом характеристика объекта
/// </summary>
[System.Serializable]
public class Attribute {

	[SerializeField]
	private string name;
	
	/// <summary>
	/// Оригинальное значение характеристики
	/// </summary>
	[SerializeField]
	private float originalValue;
	
	/// <summary>
	/// Просчитанное/новое значение характеристики
	/// </summary>
	[SerializeField]
	private float calculatedValue;
	
	/// <summary>
	/// Процент увеличения характеристики
	/// </summary>
	[SerializeField]
	private float effectProcent;
	
	/// <summary>
	/// Значение увеличения характеристики
	/// </summary>
	[SerializeField]
	private float effectValue;

	public float OriginalValue {
		get { return originalValue; }
		set {
			originalValue = value;
			Calculate(value);
		}
	}
	
	public Attribute() {}

	public Attribute(string name, float originalValue) {
		this.name = name;
		this.originalValue = originalValue;
		calculatedValue = originalValue;
	}

	/// <summary>
	/// Просчитать характеристику на основе новых данных. Это надо вызывать каждый раз, после того, как вы изменяете значение или процент
	/// </summary>
	/// <param name="originalValue"></param>
	public void Calculate(float originalValue) {
		this.originalValue = originalValue;
		calculatedValue = (originalValue + effectValue) * (effectProcent + 1);
	}

	/// <summary>
	/// Сбрасывает модификаторы характеристики
	/// </summary>
	/// <param name="originalValue">Оригинальное значение характеристики</param>
	public void Reset(float originalValue) {
		this.originalValue = originalValue;
		calculatedValue = originalValue;
		effectProcent = 0;
		effectValue = 0;
	}

	public void AddEffectValue(float value) {
		effectValue += value;
	}

	public void AddEffectProcent(float procent) {
		effectProcent += procent;
	}

	public float GetCalculated() {
		return calculatedValue;
	}

	public string GetName() {
		return name;
	}
}
