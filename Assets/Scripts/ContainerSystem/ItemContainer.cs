using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class ItemContainer : NetworkBehaviour {

	[SerializeField]
	protected string containerName;
	protected IStorage storage;
	private const float TimerToUngrab = 0.05f;

	public IStorage Storage => storage;

	protected GameObject bufferSlot;
	
	public string ContainerName => containerName;

	/// <summary>
	/// Вызывается при открытии контейнера для его инициализации
	/// </summary>
	/// <param name="storage">Хранилище для контейнера</param>
	public virtual void OnOpen(IStorage storage) {
		this.storage = storage;
	}
	
	/// <summary>
	/// Вызывается при закрытии контейнера для очистки от лишних данных
	/// </summary>
	public abstract void OnClose();

	/// <summary>
	/// Берет в мышку предмет из заданного слота
	/// </summary>
	/// <param name="slotId">Индекс слота, из которого нужно взять предмет</param>
	public void GrabFromSlot(int slotId) {
		if (GetSlot(slotId).transform.childCount == 1)
			return;
		bufferSlot = GetSlot(slotId).transform.GetChild(1).gameObject;
	}

	/// <summary>
	/// Отпускает предмет в исходный слот
	/// </summary>
	public void Ungrab() {
		if (bufferSlot == null)
			return;

		bufferSlot.transform.localPosition = new Vector3(0, 0, -171f);
		bufferSlot = null;
	}

	/// <summary>
	/// Взят ли в мышку какой-либо предмет?
	/// </summary>
	public bool IsGrabed() {
		return bufferSlot != null;
	}

	private void FixedUpdate() {
		if (bufferSlot != null) {
			Vector3 pos = Utils.GetMouseWorldPosition();
			bufferSlot.transform.position = new Vector3(pos.x, pos.y, -171f);
			if (!Input.GetMouseButton(0)) {
				GameObject toSlot =
					Utils.GetObjectOverMouse(LayerMask.GetMask("UI0"), x => x.GetComponent<ItemSlot>() != null);
				if (toSlot != null && toSlot != bufferSlot)
					storage.SlotsInteraction(bufferSlot.transform.parent.GetComponent<ItemSlot>().SlotIndex, toSlot.GetComponent<ItemSlot>().SlotIndex);
				GameObject dropArea =
					Utils.GetObjectOverMouse(LayerMask.GetMask("UI0"), x => x.name.Equals("DropArea"));
				if (dropArea == null)
					Timer.StartNewTimer("UngrabItem", TimerToUngrab, 1, gameObject, x => Ungrab());
				else {
					storage.DropItemStack(bufferSlot.transform.parent.GetComponent<ItemSlot>().SlotIndex,
						GameManager.singleton.LocalPlayer.transform.position, Utils.RandomPoint(1000));
					Ungrab();
				}
			}
		}
	}

	/// <summary>
	/// Получить объект указанного слота
	/// </summary>
	public abstract GameObject GetSlot(int id);

	public void UpdateSlots() {
		for (int i = 0; i < storage.GetStorageSize(); i++)
			GetSlot(i).GetComponent<ItemSlot>().SetStack(storage.GetItemStack(i));
	}
}
