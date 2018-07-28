using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializeHealth : MonoBehaviour, ISerializableObject {

	public Health ToSerialize;
	
	public void Initialize() {
		ToSerialize.HealthValue = (int) ToSerialize.MaxHealth.GetCalculated();
	}

	public List<string> Serialize() {
		return new List<string> { ToSerialize.HealthValue+"" };
	}

	public int Deserialize(List<string> data) {
		ToSerialize.HealthValue = int.Parse(data[0]);
		if (ToSerialize.HealthValue < 0 && ToSerialize.GetComponent<IDeath>() != null)
			ToSerialize.GetComponent<IDeath>().Death(null);
		return 1;
	}
}
