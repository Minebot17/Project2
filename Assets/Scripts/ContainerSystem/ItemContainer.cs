using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemContainer : MonoBehaviour {

	public string ContainerName;
	public GameObject BufferSlot;

	public abstract void OnOpen(List<string> data);

	public abstract List<string> OnClose();
}
