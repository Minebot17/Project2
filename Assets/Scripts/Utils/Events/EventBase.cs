using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBase {

	public GameObject Sender;
	public bool IsCancable;
	private bool isCancel;

	public EventBase(GameObject sender, bool isCancable) {
		if (InitScane.instance.LogEvents)
			Debug.Log("[Event] Sender: " + sender + " Type: " + this);
		Sender = sender;
		IsCancable = isCancable;
	}

	public bool IsCancel {
		get { return isCancel; }
		set { isCancel = IsCancable && value; }
	}
}
