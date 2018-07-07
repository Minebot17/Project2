using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class HitObjectParticle : MonoBehaviour {

	public float LiveTime;

	/// <param name="position">Место удара</param>
	/// <param name="size">Размер удара</param>
	/// <param name="hitObject">Объект, по которому ударили</param>
	public void Initialize(Vector2 position, Vector2 size, GameObject hitObject) {
		ObjectMaterial objectMaterial = hitObject.GetComponent<ObjectMaterial>();
		if (objectMaterial == null) {
			Destroy(gameObject);
			return;
		}

		Timer.StartNewTimer("DeleteHitParticle", LiveTime, 1, gameObject, x => Destroy(gameObject));
		ParticleSystem particle = GetComponent<ParticleSystem>();
		transform.position = new Vector3(position.x, position.y, transform.position.z);
		ParticleSystem.MainModule main = particle.main;
		main.startColor = new ParticleSystem.MinMaxGradient(objectMaterial.HitParticleColorMin, objectMaterial.HitParticleColorMax);
		ParticleSystem.ShapeModule shape = particle.shape;
		shape.scale = size;
		particle.Play();
	}
}
