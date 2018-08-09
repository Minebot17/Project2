using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemContainer : MonoBehaviour {

	[SerializeField]
	protected string containerName;
	protected IStorage storage;
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
				Ungrab();
				if (toSlot != null && toSlot != bufferSlot)
					SlotsInteraction(bufferSlot.transform.parent.GetComponent<ItemSlot>().SlotIndex, toSlot.GetComponent<ItemSlot>().SlotIndex);
			}
		}
	}

	/// <summary>
	/// Взаимодействие между двумя слотами. Тип взаимодействия зависит от предметов с слотах. Это может быть слияние или перемещение
	/// </summary>
	public void SlotsInteraction(int slotFrom, int slotTo) {
		ItemStack stackFrom = storage.GetItemStack(slotFrom);
		ItemStack stackTo = storage.GetItemStack(slotTo);
		if (storage.IsEmpty(slotTo)) {
			storage.SetItemStack(slotTo, stackFrom);
			storage.RemoveItemStack(slotFrom);
		}
		else if (!stackFrom.ItemName.Equals(stackTo.ItemName))
			storage.SwapItemStacks(slotFrom, slotTo);
		else {
			int maxSize = ItemManager.FindItemInfo(stackFrom.ItemName).MaxStackSize;
			int residue = stackFrom.StackSize + stackTo.StackSize - maxSize;
			if (residue > 0) {
				storage.SetStackCount(slotFrom, residue);
				storage.SetStackCount(slotTo, maxSize);
			}
			else {
				storage.RemoveItemStack(slotFrom);
				storage.SetItemStack(slotTo, new ItemStack(stackFrom.ItemName, stackFrom.StackSize + stackTo.StackSize));
			}
		}
	}

	/// <summary>
	/// Получить объект указанного слота
	/// </summary>
	public abstract GameObject GetSlot(int id);
}
