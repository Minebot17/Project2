using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SkeletonAttackHandler : AttackHandler {

	public override void AttackProjectile(Object args) {
		if (!isServer)
			return;
		/*@switch = !@switch;
		if (@switch)
			return;*/
		
		ProjectileAttackInfo info = (ProjectileAttackInfo) args;
		GameObject projectile = Instantiate(info.Projectile);
		projectile.transform.position = spawnProjectileObject.position;
		if (projectile.GetComponent<Rigidbody2D>() != null) {
			Vector2 target = Utils.ToVector2(GetComponent<VisibleObserver>().NearestTarget.transform.position);
			projectile.GetComponent<Rigidbody2D>().AddForce((target - Utils.ToVector2(spawnProjectileObject.position) + new Vector2(0, 50)) * info.TrustForce);
		}

		NetworkServer.Spawn(projectile);
	}
	
	public override void EndAttack() {
		@switch = !@switch;
		if (@switch)
			return;
		
		((AbstractTask<SkeletonInfo>)GetComponent<SkeletonTaskHandler>().GetActiveTask()).End();
	}
}
