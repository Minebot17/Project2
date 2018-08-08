using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InputManager {

	public const float DownFireTime = 0.25f;
	public const float DownJumpAndSitAxisTime = 0.15f;
	public static float HorizontalAxis;
	public static float JumpAxis;
	public static float SitAxis;
	public static float LMB;
	public static float RMB;

	private static Timer.ITimer fire1DownTimer;
	private static Timer.ITimer fire2DownTimer;
	private static Timer.ITimer jumpDownTimer;
	private static Timer.ITimer sitDownTimer;

	public static void Handle() {
		if (LMB == 0 && Input.GetAxisRaw("Fire1") >= 1 && fire1DownTimer == null)
			fire1DownTimer = Timer.StartNewTimer("Fire1Down", DownFireTime, 1, null, timer => { fire1DownTimer = null; });
		if (RMB == 0 && Input.GetAxisRaw("Fire2") >= 1 && fire2DownTimer == null)
			fire2DownTimer = Timer.StartNewTimer("Fire2Down", DownFireTime, 1, null, timer => { fire2DownTimer = null; });
		if (JumpAxis == 0 && Input.GetAxisRaw("Jump") >= 1 && jumpDownTimer == null)
			jumpDownTimer = Timer.StartNewTimer("JumpDown", DownJumpAndSitAxisTime, 1, null, timer => { jumpDownTimer = null; });
		if (SitAxis == 0 && Input.GetAxisRaw("Sit") >= 1 && sitDownTimer == null)
			sitDownTimer = Timer.StartNewTimer("SitDown", DownJumpAndSitAxisTime, 1, null, timer => { sitDownTimer = null; });
		
		HorizontalAxis = Input.GetAxisRaw("Horizontal");
		JumpAxis = Input.GetAxisRaw("Jump");
		SitAxis = Input.GetAxisRaw("Sit");
		LMB = Input.GetAxisRaw("Fire1");
		RMB = Input.GetAxisRaw("Fire2");
	}

	public static bool IsFire1Down(bool resetValue) {
		bool result = fire1DownTimer != null;
		if (resetValue)
			fire1DownTimer = null;
		return result;
	}
	
	public static bool IsFire2Down(bool resetValue) {
		bool result = fire2DownTimer != null;
		if (resetValue)
			fire2DownTimer = null;
		return result;
	}

	public static bool IsJumpDown(bool resetValue) {
		bool result = jumpDownTimer != null;
		if (resetValue)
			jumpDownTimer = null;
		return result;
	}
	
	public static bool IsSitDown(bool resetValue) {
		bool result = sitDownTimer != null;
		if (resetValue)
			sitDownTimer = null;
		return result;
	}

	public static void DisponseTimers() {
		fire1DownTimer = null;
		fire2DownTimer = null;
		jumpDownTimer = null;
		sitDownTimer = null;
	}
}
