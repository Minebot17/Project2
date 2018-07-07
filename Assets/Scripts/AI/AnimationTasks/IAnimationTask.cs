using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimationTask {
	void Handle();
	void Tick();
	String GetName();
	bool IsRemoved();
}
