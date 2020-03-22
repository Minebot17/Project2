using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TemporarilyAnimationTask<T> : AbstractAnimationTask<T> where T : EntityInfo {
	private int iterations;

	public TemporarilyAnimationTask(GameObject gameObject, int iterations) : base(gameObject) {
		this.iterations = iterations;
	}

	public abstract override void Handle();

	public override void Tick() {
		if (iterations <= 0)
			End();
		else
			iterations--;
	}

	public abstract override string GetName();
}
