using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializeStamina : MonoBehaviour, ISerializableObject {

	public Stamina ToSerialize;
	
	public void Initialize() {
		
	}

	public List<string> Serialize() {
		return new List<string> { ToSerialize.StaminaValue+"" };
	}

	public int Deserialize(List<string> data) {
		ToSerialize.StaminaValue = int.Parse(data[0]);
		return 1;
	}
}
