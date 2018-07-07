using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(EffectHandler), true)]
public class EffectsHandlerActions : ActionEditor {
	
	protected override ActionBlock[] GetBlocks() {
		return new [] {
			new ActionBlock("Add Effect", () => {
				EffectHandler handler = (EffectHandler) target;
				if (GUILayout.Button("UnconsciousEffect"))
					handler.AddEffect(new UnconsciousEffect(3));
				if (GUILayout.Button("BurnEffect"))
					handler.AddEffect(new BurnEffect());
			})
		};
	}
}
