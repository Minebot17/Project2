using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISerializableObject {
	void Initialize(); // вызывается до первого вызова Serialize вообще в сохранении
	List<string> Serialize();
	int Deserialize(List<string> data); // return type - количество десериализованных линий
}
