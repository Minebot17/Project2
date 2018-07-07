using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WhiteListEffectHandler : EffectHandler {

	[SerializeField]
	protected List<string> ListNames;
	
	public override bool AddEffect(TimeEffect effect) {
		return ListNames.Contains(effect.Name) && base.AddEffect(effect);
	}
}
