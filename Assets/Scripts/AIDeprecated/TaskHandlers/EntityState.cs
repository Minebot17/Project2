using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Состояние вашего Entity. Состояние определяет поведение ITaskSender и приоритет задач в ITaskHandler
/// </summary>
public class EntityState {
	public String name;
	public Type[] priority; // Типы ITask выстроенные по порядку в качестве их приоритета

	public EntityState(String name, Type[] priority) {
		this.name = name;
		this.priority = priority;
	}

	public static bool operator ==(EntityState state0, EntityState state1) {
		return state0.name.Equals(state1.name);
	}
	
	public static bool operator !=(EntityState state0, EntityState state1) {
		return !state0.name.Equals(state1.name);
	}
}
