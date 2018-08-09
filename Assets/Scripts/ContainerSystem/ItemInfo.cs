using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInfo : MonoBehaviour {

	public string ItemName;
	public int MaxStackSize;
	public Texture2D Icon;
	public Vector2 ColliderSize;
	public bool SpecialRenderer;

	public override bool Equals(object other) {
		return other != null && other is ItemInfo && ((ItemInfo)other).ItemName.Equals(ItemName);
	}

	public override int GetHashCode() {
		return ItemName.GetHashCode();
	}
}
