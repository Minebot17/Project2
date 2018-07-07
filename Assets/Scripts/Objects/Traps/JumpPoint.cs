using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPoint : MonoBehaviour {

	public Material JumpPointDynamicMaterial;
	private int maxHeight;

	private void Start() {
		maxHeight = int.Parse(GetComponent<SpawnedData>().spawnedData[0]);
	}

	private void OnTriggerStay2D(Collider2D other) {
		if (other.gameObject.transform.parent != null && other.gameObject.transform.parent.name.Equals("Player") && InputManager.IsJumpDown(true)) {
			GetComponent<Animator>().SetTrigger("Jump");
			Rigidbody2D rigidbody = other.gameObject.transform.parent.GetComponent<Rigidbody2D>();
			rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0);
			rigidbody.AddForce(new Vector2(0, Mathf.Sqrt((2 * maxHeight * 980f)/(Time.fixedDeltaTime * Time.fixedDeltaTime))));
			float xVelocity = rigidbody.velocity.x;
			if (xVelocity > 100 || xVelocity < -100) {
				bool left = xVelocity > 100;
				transform.GetChild(5).localEulerAngles = new Vector3(0, 0, 25f * (left ? -1 : 1));
				Timer.StartNewTimer("JumpPointAngle", 0.7f, 1, gameObject, x => {
					transform.GetChild(5).localEulerAngles = new Vector3(0, 0, 0);
				});
			}

			Timer.StartNewTimer("JumPointAlpha0", 0.1333f, 1, gameObject, timer0 => {
				JumpPointDynamicMaterial.color = new Color(0, 0, 0, 0);
				Timer.StartNewTimer("JumpPoint1", 0.2333f, 1, gameObject, timer1 => {
					JumpPointDynamicMaterial.color = new Color(0, 0, 0, 0.56f);
					Timer.StartNewTimer("JumpPoint2", 0.2666f, 1, gameObject, timer2 => {
						JumpPointDynamicMaterial.color = new Color(0, 0, 0, 1f);
					}, (timer, f, arg3, arg4) => {
						JumpPointDynamicMaterial.color += new Color(0, 0, 0, 1.65f * Time.deltaTime);
					});
				}, (timer, f, arg3, arg4) => {
					JumpPointDynamicMaterial.color += new Color(0, 0, 0, 2.4f * Time.deltaTime);
				});
			}, (timer, f, arg3, arg4) => {
				JumpPointDynamicMaterial.color -= new Color(0, 0, 0, 7.5f * Time.deltaTime);
			});
		}
	}
}
