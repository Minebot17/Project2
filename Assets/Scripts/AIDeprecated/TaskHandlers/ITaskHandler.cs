using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITaskHandler {
	void AddTask(ITask task);
	void AddAnimationTask(IAnimationTask animationTask);
	void ReceiveEvent(EventBase e);
	EntityState GetDefaultState();
	EntityState GetCurrentState();
}
