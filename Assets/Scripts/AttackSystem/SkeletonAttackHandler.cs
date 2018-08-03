using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonAttackHandler : AttackHandler {

	public override void EndAttack() {
		@switch = !@switch;
		if (@switch)
			return;
		
		((AbstractTask<SkeletonInfo>)GetComponent<SkeletonTaskHandler>().GetActiveTask()).End();
	}
}
