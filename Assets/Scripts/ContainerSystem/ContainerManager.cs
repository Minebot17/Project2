using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerManager {
	public static ItemContainer CurrentContainer;

	public static void OpenContainer(GameObject container, IStorage storage) {
		if (ReferenceEquals(CurrentContainer, null)) {
			CurrentContainer = MonoBehaviour.Instantiate(container).GetComponent<ItemContainer>();
			CurrentContainer.OnOpen(storage);
		}
	}

	public static void CloseContainer() {
		if (!ReferenceEquals(CurrentContainer, null)) {
			CurrentContainer.OnClose();
			CurrentContainer = null;
		}
	}

	public static bool IsOpen() {
		return !ReferenceEquals(CurrentContainer, null);
	}

	public static bool IsOpen(string containerName) {
		return !ReferenceEquals(CurrentContainer, null) &&
				CurrentContainer.ContainerName.Equals(containerName);
	}

	public static bool IsOpen(IStorage storage) {
		return !ReferenceEquals(CurrentContainer, null) && CurrentContainer.Storage == storage;
	}

	public static void UpdateSlots() {
		CurrentContainer.UpdateSlots();
	}
}
