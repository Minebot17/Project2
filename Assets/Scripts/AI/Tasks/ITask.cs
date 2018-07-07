using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITask {
	bool Handle();
	void Tick();
	bool IsRemoved();
}
