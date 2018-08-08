using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerManager {
	public static GameObject CurrentContainer;

	public static void OpenContainer(GameObject container, IStorage storage) {
		if (CurrentContainer == null) {
			CurrentContainer = MonoBehaviour.Instantiate(container);
			CurrentContainer.GetComponent<ItemContainer>().OnOpen(storage);
		}
	}

	public static void CloseContainer() {
		if (CurrentContainer != null) {
			CurrentContainer.GetComponent<ItemContainer>().OnClose();
			CurrentContainer = null;
		}
	}

	public static bool IsOpen() {
		return CurrentContainer != null;
	}

	public static bool IsOpen(string containerName) {
		return CurrentContainer != null &&
		       CurrentContainer.GetComponent<ItemContainer>().ContainerName.Equals(containerName);
	}
}
