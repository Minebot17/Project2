using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(EntityJumpedInfo))]
public class JumpedEntityController : NetworkBehaviour {
	private EntityJumpedInfo info;
	private Rigidbody2D rigidbody2D;

	private void Start () {
		info = GetComponent<EntityJumpedInfo>();
		rigidbody2D = GetComponent<Rigidbody2D>();
		
		// Debug
		if (GameSettings.SettingVisualizeTraectorySimple.Value) {
			LineRenderer line = Instantiate(GameManager.singleton.LineDebugObject, transform.position, new Quaternion(), transform).GetComponent<LineRenderer>();
			Timer.StartNewTimer("PlayerTraectoryCenter", GameSettings.SettingTraectoryTracingFrequency.Value, -1, gameObject, timer => {
				line.positionCount = line.positionCount + 1;
				line.SetPosition(line.positionCount-1, transform.position + new Vector3(0, 0, -0.1f));
			});
		}

		if (GameSettings.SettingVisualizeTraectoryAdvanced.Value) {
			LineRenderer line0 = Instantiate(GameManager.singleton.LineDebugObject, transform.position, new Quaternion(), transform).GetComponent<LineRenderer>();
			Timer.StartNewTimer("PlayerTraectoryCenter", GameSettings.SettingTraectoryTracingFrequency.Value, -1, gameObject, timer => {
				line0.positionCount = line0.positionCount + 1;
				line0.SetPosition(line0.positionCount-1, transform.position + new Vector3(-16.5f, 32.5f, -0.1f));
			});
			LineRenderer line1 = Instantiate(GameManager.singleton.LineDebugObject, transform.position, new Quaternion(), transform).GetComponent<LineRenderer>();
			Timer.StartNewTimer("PlayerTraectoryCenter", GameSettings.SettingTraectoryTracingFrequency.Value, -1, gameObject, timer => {
				line1.positionCount = line1.positionCount + 1;
				line1.SetPosition(line1.positionCount-1, transform.position + new Vector3(16.5f, 32.5f, -0.1f));
			});
			LineRenderer line2 = Instantiate(GameManager.singleton.LineDebugObject, transform.position, new Quaternion(), transform).GetComponent<LineRenderer>();
			Timer.StartNewTimer("PlayerTraectoryCenter", GameSettings.SettingTraectoryTracingFrequency.Value, -1, gameObject, timer => {
				line2.positionCount = line2.positionCount + 1;
				line2.SetPosition(line2.positionCount-1, transform.position + new Vector3(16.5f, -20.5f, -0.1f));
			});
			LineRenderer line3 = Instantiate(GameManager.singleton.LineDebugObject, transform.position, new Quaternion(), transform).GetComponent<LineRenderer>();
			Timer.StartNewTimer("PlayerTraectoryCenter", GameSettings.SettingTraectoryTracingFrequency.Value, -1, gameObject, timer => {
				line3.positionCount = line3.positionCount + 1;
				line3.SetPosition(line3.positionCount-1, transform.position + new Vector3(-16.5f, -20.5f, -0.1f));
			});
		}
	}

	private void FixedUpdate() {
		if (!isLocalPlayer)
			return;
		if (!info.EnableAI)
			return;
		if (ContainerManager.IsOpen())
			return;
		
		int scale = (int)transform.localScale.x;
		transform.Find("NameRender").transform.localScale = new Vector3(scale, 1, 1);
		bool toForward = scale == 1 ? InputManager.HorizontalAxis >= 1 : InputManager.HorizontalAxis <= -1;
		bool toBack = scale == 1 ? InputManager.HorizontalAxis <= -1 : InputManager.HorizontalAxis >= 1;

		if ((!toForward && !toBack) || (toForward && toBack) || (UnityEngine.Input.GetKey(KeyCode.A) && UnityEngine.Input.GetKey(KeyCode.D))) {
			if (info.IsRunBack || info.IsRunForward)
				SetStand();
		}
		else {
			SetRun(toForward);
			Move((int)InputManager.HorizontalAxis, info.RunSpeed);
		}
		
		if (InputManager.IsJumpDown(false) && info.OnGround && Timer.FindTimer("JumpUpper") == null) {
			Jump(info.JumpPower/1.25f);
			Timer.StartNewTimer("JumpUpper", 0.02f, 5, gameObject, timer => {
				if (info.OnGround || InputManager.JumpAxis < 1)
					timer.Remove();
				else
					rigidbody2D.AddForce(new Vector2(0, info.JumpPower/7));
			});
		}

		if (InputManager.IsSitDown(false) && info.OnGround) {
			Collider2D[] colliders = new Collider2D[10];
			Physics2D.OverlapCollider(info.GroundTrigger, new ContactFilter2D(), colliders);
			foreach (Collider2D collider in colliders)
				if (!ReferenceEquals(collider, null) && !ReferenceEquals(collider.gameObject.GetComponent<PlatformEffector2D>(), null)) {
					collider.gameObject.SetActive(false);
					Timer.StartNewTimer("RemovePlatform", 0.25f, 1, gameObject,
						x => collider.gameObject.SetActive(true));
				}
		}

		Animator anim = GetComponent<Animator>();
		anim.SetBool("RunForward", info.IsRunForward);
		anim.SetBool("RunBack", info.IsRunBack);
		anim.SetBool("Fall", !info.OnGround);
	}
	
	private void Jump(float power) {
		EntityJumpedInfo.JumpEvent result = info.jumpEvent.CallListners(new EntityJumpedInfo.JumpEvent(gameObject, power));
		if (!result.IsCancel)
			rigidbody2D.AddForce(new Vector2(0, result.JumpPower));
	}
	
	private void SetStand() {
		rigidbody2D.velocity = new Vector2(0, rigidbody2D.velocity.y);
		info.standEvent.CallListners(new EntityMovableInfo.StandEvent(gameObject));
		info.IsRunBack = false;
		info.IsRunForward = false;
	}

	private void SetRun(bool forward) {
		info.IsRunForward = forward;
		info.IsRunBack = !forward;
	}
	
	private void Move(int direction, float speed) {
		int scale = (int)transform.localScale.x;
		if (scale != direction)
			transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
			
		EntityMovableInfo.RunEvent result = info.runEvent.CallListners(new EntityMovableInfo.RunEvent(gameObject, direction, speed));
		if (!result.IsCancel)
			rigidbody2D.velocity = new Vector2(result.Direction * result.Speed, rigidbody2D.velocity.y);
	}
}
