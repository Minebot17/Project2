using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerManager {
	public static GameObject CurrentContainer;

	public static void OpenContainer(GameObject container, List<string> data) {
		if (CurrentContainer == null) {
			CurrentContainer = container;
			MonoBehaviour.Instantiate(container).GetComponent<ItemContainer>().OnOpen(data);
		}
	}

	public static List<string> CloseContainer() {
		if (CurrentContainer != null) {
			List<string> result = CurrentContainer.GetComponent<ItemContainer>().OnClose();
			CurrentContainer = null;
			return result;
		}

		return null;
	}
}
