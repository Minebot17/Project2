using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializeHealth : MonoBehaviour, ISerializableObject {

	public Health ToSerialize;
	
	public void Initialize() {
		ToSerialize.HealthValue = (int) ToSerialize.MaxHealth.GetCalculated();
	}

	public List<string> Serialize() {
		return new List<string> { ToSerialize.HealthValue+"", ToSerialize.MaxHealth.OriginalValue+"" };
	}

	public int Deserialize(List<string> data) {
		ToSerialize.HealthValue = int.Parse(data[0]);
		ToSerialize.MaxHealth.Calculate(float.Parse(data[1]));
		if (ToSerialize.HealthValue < 0 && ToSerialize.GetComponent<IDeath>() != null)
			ToSerialize.GetComponent<IDeath>().Death(null);
		return 1;
	}
}
