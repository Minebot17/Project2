using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackListEffectHandler : EffectHandler {

	[SerializeField]
	protected List<string> ListNames;
	
	public override bool AddEffect(TimeEffect effect) {
		return !ListNames.Contains(effect.Name) && base.AddEffect(effect);
	}
}
