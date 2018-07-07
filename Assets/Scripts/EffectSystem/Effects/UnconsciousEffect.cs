using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnconsciousEffect : TimeEffect {
	
	public UnconsciousEffect(float timeLeft) : base("Unconscious", timeLeft, true) { }
	
	public override bool OnAdded() {
		bool haveEntityInfo = Handler.gameObject.GetComponent<EntityInfo>() != null;
		if (haveEntityInfo)
			Handler.gameObject.GetComponent<EntityInfo>().EnableAI = false;

		return haveEntityInfo;
	}

	public override void OnUpdate() {
		
	}

	public override bool OnDeleted() {
		Handler.gameObject.GetComponent<EntityInfo>().EnableAI = true;
		return true;
	}
}
