using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAttackHandler : AttackHandler {

	public override void EndAttack() {
		@switch = !@switch;
		if (@switch)
			return;
		AbstractTask<EntityInfo> macke = ((AbstractTask<EntityInfo>)GetComponent<AbstractTaskHandler<EntityInfo>>().GetActiveTask());
		macke.End();
	}
}
