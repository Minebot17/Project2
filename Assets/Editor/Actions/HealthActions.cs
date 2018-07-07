using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(Health))]
public class HealthActions : ActionEditor {
	private int damage;
	private int heal;

	protected override ActionBlock[] GetBlocks() {
		return new [] {
			new ActionBlock("Damage", () => {
				damage = EditorGUILayout.IntField("Value: ", damage);
				Health health = (Health)target;
				if(GUILayout.Button("Damage")) {
					health.Damage(new DamageBase(null, damage));
				}
			}), 
			new ActionBlock("Heal", () => {
				heal = EditorGUILayout.IntField("Value: ", heal);
				Health health = (Health)target;
				if(GUILayout.Button("Heal")) {
					health.Heal(heal);
				}
			}), 
		};
	}
}
