using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnEffect : TimeEffect {
	private Health health;
	private List<GameObject> emitters = new List<GameObject>();
	private int timer = 20;
	private const float MinBurnDistance = 5;
	private bool firstTranslate = true;

	public BurnEffect() : base("Burn", 5, true) { }
	
	public override bool OnAdded() {
		health = Handler.gameObject.GetComponent<Health>();
		if (health == null)
			return false;

		NearestObjects nearestObjects = Handler.gameObject.GetComponent<NearestObjects>();
		if (nearestObjects == null)
			return false;
		
		nearestObjects.onNearestEnter.SubcribeEvent(e => {
			EffectHandler handler = e.Collider.gameObject.GetComponent<EffectHandler>();
			if (handler != null)
				handler.AddEffect(new BurnEffect());
		});
		
		List<Collider2D> colliders = Utils.GetComponentsRecursive<Collider2D>(Handler.gameObject);
		if (Handler.gameObject.GetComponent<MeshFilter>() != null && Handler.gameObject.GetComponent<MeshFilter>().mesh == GameManager.singleton.OnePlane) {
			for (int x = 2; x < Handler.gameObject.transform.localScale.x; x += 4)
				for (int y = 2; y < Handler.gameObject.transform.localScale.y; y += 4) {
					Collider2D[] touching = Physics2D.OverlapPointAll(
						Utils.ToVector2(Handler.gameObject.transform.position) + new Vector2(x, y),
						GameManager.singleton.TrapLayerMask);
	
					foreach (Collider2D collider in touching)
						if (colliders.Exists(i => i.Equals(collider))) {
							emitters.Add(MonoBehaviour.Instantiate(GameManager.singleton.FireObjectParticle,
								Handler.gameObject.transform.position + new Vector3(x, y, -1), new Quaternion(),
								Handler.gameObject.transform));
						}

			}
		}
		else {
			int mask = LayerMask.GetMask(LayerMask.LayerToName(Handler.gameObject.layer));
			for(int x = -200; x < 200; x+=10)
				for (int y = -200; y < 200; y += 10) {
					Collider2D[] touching = Physics2D.OverlapPointAll(
						Utils.ToVector2(Handler.gameObject.transform.position) + new Vector2(x, y),
						mask);
	
					foreach (Collider2D collider in touching)
						if (colliders.Exists(i => i.Equals(collider))) {
							emitters.Add(MonoBehaviour.Instantiate(GameManager.singleton.FireObjectParticle,
								Handler.gameObject.transform.position + new Vector3(x, y, -1), new Quaternion(),
								Handler.gameObject.transform));
						}
				}
		}

		return true;
	}

	public override void OnUpdate() {
		if (timer > 0)
			timer--;
		else {
			timer = 20;
			health.Damage(new DamageBase(Handler.gameObject, 2));

			if (firstTranslate) {
				foreach (Collider2D collider in Handler.gameObject.GetComponent<NearestObjects>().NearestColliders) {
					EffectHandler handler = collider.gameObject.GetComponent<EffectHandler>();
					if (handler != null)
						handler.AddEffect(new BurnEffect());
				}
				firstTranslate = false;
			}
		}
	}

	public override bool OnDeleted() {
		foreach (GameObject emitter in emitters)
			MonoBehaviour.Destroy(emitter);
		return true;
	}
}
