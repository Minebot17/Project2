using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibleObserver : PlayerObserver {
	public Vector3 Offset;
	private bool beVisible;

	protected override void Observe() {
		bool visible = Utils.IsFreeBetweenPlayer(transform.position + Offset) && Utils.GetDistanceBetweenPlayer(transform.position + Offset) <= ObserveRadius;
		if (visible && !beVisible)
			CallEvent(new PlayerEnter(gameObject));
		else if (!visible && beVisible)
			CallEvent(new PlayerOut(gameObject));
		beVisible = visible;
	}
}
