using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializeHiddenBorder : MonoBehaviour, ISerializableObject {

	public void Initialize() {
		
	}

	public List<string> Serialize() {
		return new List<string>();
	}

	public int Deserialize(List<string> data) {
		GetComponent<HiddenEntryObject>().Invoke("Start", 0);
		return 0;
	}
}
