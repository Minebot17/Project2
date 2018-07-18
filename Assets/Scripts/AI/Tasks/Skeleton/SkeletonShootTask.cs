using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonShootTask : AbstractTask<SkeletonInfo> {
	private Vector2 vector;

	public SkeletonShootTask(GameObject gameObject, Vector2 vector) : base(gameObject) {
		this.vector = vector;
	}
	
	public override bool Handle() {
		if (gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Attack0"))
			return false;

		GameObject target = info.observer.NearestTarget;
		float angle = Mathf.Rad2Deg *
		              (Mathf.Atan2(target.transform.position.y - gameObject.transform.position.y,
			              (target.transform.position.x - gameObject.transform.position.x) *
			              gameObject.transform.localScale.x));
		
		if (!Utils.IsRotatedToPlayer(target, gameObject.transform))
			gameObject.transform.localScale = new Vector3(-gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);

		if (angle > 45) {
			AddAnimation(new HeadAnimationTask(gameObject, 50));
			return false;
		}

		SkeletonInfo.ShootEvent result = info.GetEventSystem<SkeletonInfo.ShootEvent>()
			.CallListners(new SkeletonInfo.ShootEvent(gameObject, true));

		if (!result.IsCancel) {
			AddAnimation(new HeadAnimationTask(gameObject, 70));
			AddAnimation(new SpineAnimationTask(gameObject, 20));
			
			Timer.StartNewTimer("ShootBoneRender", 0.4f, 1, gameObject, timer => {
				info.BoneRender.GetComponent<MeshRenderer>().enabled = false;
				GameObject bone = (GameObject) MonoBehaviour.Instantiate(info.Projectile, info.BoneRender.transform.position, new Quaternion());
				bone.GetComponent<Rigidbody2D>().AddForce(vector*(float)(GameManager.rnd.NextDouble()/10f + 0.9f)*30000 + new Vector2(0, 50));
				bone.GetComponent<Rigidbody2D>().AddTorque(30000f * (float)GameManager.rnd.NextDouble());
			
				Timer.StartNewTimer("ShootBoneRenderPost", 0.68f, 1, gameObject,
					timer1 => {
						info.BoneRender.GetComponent<MeshRenderer>().enabled = true;
						info.GetEventSystem<SkeletonInfo.ShootEvent>().CallListners(new SkeletonInfo.ShootEvent(gameObject, false));
						Timer.StartNewTimer("ShootEnd", 0.48f, 1, gameObject, timer2 => {
							End();
						});
					});
			});
		}

		return !result.IsCancel;
	}

	public override void Tick() {
		
	}
}
