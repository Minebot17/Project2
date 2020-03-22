using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTask : AbstractTask<EntityInfo> {

	public RotateTask(GameObject gameObject) : base(gameObject) { }
	
	public override bool Handle() {
		gameObject.transform.localScale = new Vector3(-gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
		End();
		return true;
	}

	public override void Tick() {
		
	}
}
