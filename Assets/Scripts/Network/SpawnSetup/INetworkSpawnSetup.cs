using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INetworkSpawnSetup {
	List<string> SendData(GameObject gameObject);
	void RecieveData(GameObject gameObject, List<string> data);
}
