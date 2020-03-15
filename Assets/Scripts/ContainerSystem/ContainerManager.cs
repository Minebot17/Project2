using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerManager {
	public static ItemContainer CurrentContainer;

	public static void OpenContainer(GameObject container, IStorage storage) {
		if (!CurrentContainer) {
			CurrentContainer = MonoBehaviour.Instantiate(container).GetComponent<ItemContainer>();
			CurrentContainer.OnOpen(storage);
		}
	}

	public static void CloseContainer() {
		if (CurrentContainer) {
			CurrentContainer.OnClose();
			CurrentContainer = null;
		}
	}

	public static bool IsOpen() {
		return CurrentContainer;
	}

	public static bool IsOpen(string containerName) {
		return CurrentContainer &&
		       CurrentContainer.ContainerName.Equals(containerName);
	}

	public static bool IsOpen(IStorage storage) {
		return CurrentContainer && CurrentContainer.Storage == storage;
	}

	public static void UpdateSlots() {
		CurrentContainer.UpdateSlots();
	}
}
