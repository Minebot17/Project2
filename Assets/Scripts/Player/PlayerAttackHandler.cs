using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(EntityJumpedInfo))]
public class PlayerAttackHandler : AttackHandler {
	public readonly EventHandler<AttackEvent> attackEvent = new EventHandler<AttackEvent>();

	[SerializeField] 
	private float comboTimerError;

	[SerializeField] 
	private int numberOfCombo;
	
	[SerializeField] 
	private int weaponCount;

	private Timer.ITimer postAttackTimer;
	private int currentCombo = -1;
	private bool isAttack;
	private bool clickPostAttack;
	private EntityJumpedInfo info;
	private int oldCombo;
	private Timer.ITimer cdTimer;

	private void Start() {
		info = GetComponent<EntityJumpedInfo>();
	}

	private void FixedUpdate() {
		if (!isLocalPlayer)
			return;
		if (!info.EnableAI)
			return;
		if (ContainerManager.IsOpen())
			return;
		if (InputManager.IsFire1Down(true) && info.EnableAI) {
			AnimatorStateInfo anim = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
			if (currentCombo == -1 && postAttackTimer == null)
				Attack();
			else if ((postAttackTimer != null || (isAttack && anim.length - (anim.normalizedTime * anim.length) < comboTimerError)) && !clickPostAttack)
				clickPostAttack = true;
		}
		
		GetComponent<Animator>().SetInteger("Attack", isAttack ? currentCombo : -1);
	}

	private void Attack() {
		AttackEvent result = attackEvent.CallListners(new AttackEvent(gameObject));
		if (result.IsCancel)
			return;

		isAttack = true;
		currentCombo = (currentCombo + 1) >= numberOfCombo ? 0 : currentCombo + 1;
		if (postAttackTimer != null) {
			postAttackTimer.Remove();
			postAttackTimer = null;
		}
	}

	public override void EndAttack() {
		if (cdTimer != null)
			return;
		cdTimer = Timer.StartNewTimer("CD", 0.1f, 1, gameObject, x => { cdTimer = null; });
		
		isAttack = false;
		if (!clickPostAttack) {
			oldCombo = currentCombo;
			currentCombo = -1;
			postAttackTimer = Timer.StartNewTimer("PlayerPostAttack", comboTimerError, 1, gameObject, timer1 => {
				postAttackTimer = null;
				currentCombo = -1;
			}, (timer1, f, arg3, arg4) => {
				if (clickPostAttack) {
					clickPostAttack = false;
					currentCombo = oldCombo;
					Attack();
				}
			});
		}
		else {
			clickPostAttack = false;
			Attack();
		}
	}
}
