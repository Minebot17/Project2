using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBlock {

	public string name;
	public Action draw;
	
	public ActionBlock(string name, Action draw) {
		this.name = name;
		this.draw = draw;
	}
}
