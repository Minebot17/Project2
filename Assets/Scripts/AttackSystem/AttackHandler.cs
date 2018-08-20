using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class AttackHandler : NetworkBehaviour {
	
	[SerializeField] 
	protected Transform spawnProjectileObject;
	protected bool @switch;

	public virtual void AttackMelee(Object args) {
		if (!isServer)
			return;
		
		/*@switch = !@switch;
		if (@switch)
			return;*/

		MeleeAttackInfo info = (MeleeAttackInfo) args;
		List<GameObject> gos = Physics2D
			.OverlapBoxAll(
				new Vector2(info.Point.x * (int) transform.localScale.x, info.Point.y) +
				new Vector2(gameObject.transform.position.x, gameObject.transform.position.y), info.Size, 0
			).ToList().ConvertAll(x => x.gameObject);
		
		gos.RemoveAll(x => x.gameObject.transform.IsChildOf(gameObject.transform));
		if (info.Splash) {
			gos.ForEach(x => AttackTarget(x, info) );
		}
		else {
			GameObject target = Utils.FindNearestGameObject(gos, info.Point + Utils.ToVector2(gameObject.transform.position));
			AttackTarget(target, info);
		}
	}
	
	public virtual void AttackProjectile(Object args) {
		if (!isServer)
			return;
		/*@switch = !@switch;
		if (@switch)
			return;*/
		
		ProjectileAttackInfo info = (ProjectileAttackInfo) args;
		GameObject projectile = Instantiate(info.Projectile);
		projectile.transform.position = spawnProjectileObject.position;
		if (projectile.GetComponent<Rigidbody2D>() != null)
			projectile.GetComponent<Rigidbody2D>().AddForce(Utils.GetPlayerLook() * info.TrustForce);
		NetworkServer.Spawn(projectile);
	}

	private void AttackTarget(GameObject target, MeleeAttackInfo info) {
		if (!isServer)
			return;
		
		if (target.GetComponent<Health>() != null) {
			target.GetComponent<Health>().Damage(new DamageBase(gameObject, info.Damage));
			ObjectMaterial objectMaterial = target.GetComponent<ObjectMaterial>();
			RpcDamage(target, info, objectMaterial.HitParticleColorMin, objectMaterial.HitParticleColorMax);
		}

		if (target.GetComponent<Kickable>() != null) {
			target.GetComponent<Kickable>()
				.Kick(new Vector2((int) gameObject.transform.localScale.x * info.Kick.x, info.Kick.y));
		}
	}
	
	[ClientRpc]
	private void RpcDamage(GameObject target, MeleeAttackInfo info, Color min, Color max) {
		Instantiate(GameManager.singleton.HitObjectParticle).GetComponent<HitObjectParticle>().Initialize(new Vector2(info.Point.x * (int) transform.localScale.x, info.Point.y) + Utils.ToVector2(transform.position), info.Size, target, min, max);
	}

	public virtual void EndAttack() {
		@switch = !@switch;
		if (@switch)
			return;
		
		// tODO кончание таска
	}
	
	public class AttackEvent : EventBase {
		public AttackEvent(GameObject sender) : base(sender, true) { }
	}
	
	public override int GetNetworkChannel() {
		return Channels.DefaultReliable;
	}
}
