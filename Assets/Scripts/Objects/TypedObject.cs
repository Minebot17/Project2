using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypedObject : MonoBehaviour {
	
	public int TypeIndex;
	public Type[] Types;

	[System.Serializable]
	public class Type {
		public Vector2 Size;
		public Vector2 MinUV;
		public Vector2 MaxUV;
		public Type(Vector2 size, Vector2 minUv, Vector2 maxUv) {
			Size = size;
			MinUV = minUv;
			MaxUV = maxUv;
		}
	}
}
