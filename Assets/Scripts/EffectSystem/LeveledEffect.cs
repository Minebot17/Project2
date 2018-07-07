using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LeveledEffect : TimeEffect {

	public int Level;
	public bool Stackable;

	public LeveledEffect(string name, float timeLeft, bool isDisplayed, int level, bool stackable) : base(name, timeLeft, isDisplayed) {
		Level = level;
		Stackable = stackable;
	}

	public void AddLevel(int level) {
		Level += level;
	}
}
