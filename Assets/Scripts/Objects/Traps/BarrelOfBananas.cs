using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(DeathStandart))]
public class BarrelOfBananas : MonoBehaviour {
	public GameObject Banana;
	
	private void Start() {
		GetComponent<DeathStandart>().GetEventSystem<DeathStandart.DeathEvent>().SubcribeEvent(e => {
			int count = InitScane.rnd.Next(3) + 3;

			for (int i = 0; i < count; i++) {
				GameObject banana = ObjectsManager.SpawnGameObject(Banana, transform.position + new Vector3(0, 0, -0.001f), new Vector3(), null, true);
				banana.GetComponent<Rigidbody2D>().AddForce(Utils.RandomPoint(3000) + new Vector2(1500, 3000));
				banana.GetComponent<SpawnedData>().spawnedData = GetComponent<SpawnedData>().spawnedData;
			}
		});
	}
}
