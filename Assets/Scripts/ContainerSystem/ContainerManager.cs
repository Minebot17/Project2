using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerManager {
	public static ItemContainer CurrentContainer;

	public static void OpenContainer(GameObject container, IStorage storage) {
		if (CurrentContainer == null) {
			CurrentContainer = MonoBehaviour.Instantiate(container).GetComponent<ItemContainer>();
			CurrentContainer.OnOpen(storage);
		}
	}

	public static void CloseContainer() {
		if (CurrentContainer != null) {
			CurrentContainer.OnClose();
			CurrentContainer = null;
		}
	}

	public static bool IsOpen() {
		return CurrentContainer != null;
	}

	public static bool IsOpen(string containerName) {
		return CurrentContainer != null &&
		       CurrentContainer.ContainerName.Equals(containerName);
	}
}
