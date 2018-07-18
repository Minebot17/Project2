using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISerializableObject {
	void Initialize(); // вызывается до первого вызова Serialize вообще в сохранении
	List<string> Serialize();
	void Deserialize(List<string> data);
}
