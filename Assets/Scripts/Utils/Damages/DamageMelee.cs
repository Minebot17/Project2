using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageMelee : DamageBase {
	public MeleeAttackInfo Info;

	public DamageMelee(GameObject sourse, MeleeAttackInfo info) : base(sourse, info.Damage) {
		Info = info;
	}
}
