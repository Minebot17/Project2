using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DogAttackHandler : AttackHandler {
	
	public override void EndAttack() {
		@switch = !@switch;
		if (@switch)
			return;
		
		((AbstractTask<DogInfo>)GetComponent<DogTaskHandler>().GetActiveTask()).End();
	}
}
