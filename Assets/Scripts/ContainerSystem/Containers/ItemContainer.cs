using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class ItemContainer : MonoBehaviour {

	[SerializeField]
	protected string containerName;
	protected IStorage storage;

	public IStorage Storage => storage;

	public GameObject bufferSlot;
	
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
		if (GetSlot(slotId).transform.childCount == 0)
			return;
		bufferSlot = GetSlot(slotId).transform.GetChild(0).gameObject;
	}

	/// <summary>
	/// Отпускает предмет в исходный слот
	/// </summary>
	public void Ungrab() {
		if (ReferenceEquals(bufferSlot, null))
			return;

		bufferSlot.transform.localPosition = new Vector3(0, 0, -171f);
		bufferSlot = null;
	}

	/// <summary>
	/// Взят ли в мышку какой-либо предмет?
	/// </summary>
	public bool IsGrabed() {
		return !ReferenceEquals(bufferSlot, null);
	}

	private int indexToGrab = -1;
	private void FixedUpdate() {
		if (indexToGrab != -1) {
			if (!storage.IsEmpty(indexToGrab))
				GrabFromSlot(indexToGrab);
			indexToGrab = -1;
		}

		if (!ReferenceEquals(bufferSlot, null)) {
			Vector3 pos = Utils.GetMouseWorldPosition();
			bufferSlot.transform.position = new Vector3(pos.x, pos.y, 0);
			bufferSlot.transform.localPosition = new Vector3(bufferSlot.transform.localPosition.x, bufferSlot.transform.localPosition.y, -174f);
			
			if (!Input.GetMouseButton(0)) {
				GameObject toSlot =
					Utils.GetObjectOverMouse(LayerMask.GetMask("UI0"), x => !ReferenceEquals(x.GetComponent<ItemSlot>(), null));
				if (!ReferenceEquals(toSlot, null) && toSlot != bufferSlot.transform.parent.gameObject)
					storage.SlotsInteraction(bufferSlot.transform.parent.GetComponent<ItemSlot>().SlotIndex, toSlot.GetComponent<ItemSlot>().SlotIndex);
				GameObject dropArea =
					Utils.GetObjectOverMouse(LayerMask.GetMask("UI0"), x => x.name.Equals("DropArea"));
				if (!ReferenceEquals(dropArea, null))
					storage.DropItemStack(bufferSlot.transform.parent.GetComponent<ItemSlot>().SlotIndex,
						GameManager.singleton.LocalPlayer.transform.position, Utils.RandomPoint(1000));
				else if (!storage.IsEmpty(GetBufferIndex()))
					Ungrab();
			}

			if (GameSettings.PutStackUnitKey.IsDown()) {
				GameObject toSlot =
					Utils.GetObjectOverMouse(LayerMask.GetMask("UI0"), x => !ReferenceEquals(x.GetComponent<ItemSlot>(), null));
				if (!ReferenceEquals(toSlot, null) && toSlot != bufferSlot.transform.parent.gameObject && (ReferenceEquals(toSlot.GetComponent<ItemSlot>().Stack, null) || bufferSlot.transform.parent.GetComponent<ItemSlot>().Stack.EqualsWithoutSize(toSlot.GetComponent<ItemSlot>().Stack))) {
					int slotOne = bufferSlot.transform.parent.GetComponent<ItemSlot>().SlotIndex;
					int slotTwo = toSlot.GetComponent<ItemSlot>().SlotIndex;
					if (storage.IsEmpty(slotTwo))
						storage.SetItemStack(slotTwo, new ItemStack(storage.GetItemStack(slotOne).ItemName, 1));
					else
						storage.SetStackCount(slotTwo, storage.GetItemStack(slotTwo).StackSize + 1);
					storage.SetStackCount(slotOne, storage.GetItemStack(slotOne).StackSize - 1);
				}
				
				GameObject dropArea =
					Utils.GetObjectOverMouse(LayerMask.GetMask("UI0"), x => x.name.Equals("DropArea"));
				if (!ReferenceEquals(dropArea, null)) {
					storage.DropItemStack(bufferSlot.transform.parent.GetComponent<ItemSlot>().SlotIndex,
						1, GameManager.singleton.LocalPlayer.transform.position, Utils.RandomPoint(1000));
				}
			}
			
			if (GameSettings.TakeStackUnitKey.IsDown()) {
				GameObject toSlot =
					Utils.GetObjectOverMouse(LayerMask.GetMask("UI0"), x => !ReferenceEquals(x.GetComponent<ItemSlot>(), null));
				if (!ReferenceEquals(toSlot, null) && toSlot != bufferSlot.transform.parent.gameObject && (toSlot.GetComponent<ItemSlot>().Stack != null && bufferSlot.transform.parent.GetComponent<ItemSlot>().Stack.EqualsWithoutSize(toSlot.GetComponent<ItemSlot>().Stack))) {
					int slotOne = bufferSlot.transform.parent.GetComponent<ItemSlot>().SlotIndex;
					int slotTwo = toSlot.GetComponent<ItemSlot>().SlotIndex;
					int count = storage.GetItemStack(slotOne).StackSize;
					int maxCount = ItemManager.FindItemInfo(storage.GetItemStack(slotOne).ItemName).MaxStackSize;
					if (count < maxCount) {
						storage.SetStackCount(slotOne, count + 1);
						storage.SetStackCount(slotTwo, toSlot.GetComponent<ItemSlot>().Stack.StackSize - 1);
					}
				}
			}
		}
	}

	/// <summary>
	/// Получить объект указанного слота
	/// </summary>
	public abstract GameObject GetSlot(int id);

	protected int GetBufferIndex() {
		return bufferSlot.transform.parent.GetComponent<ItemSlot>().SlotIndex;
	}

	public void UpdateSlots() {
		for (int i = 0; i < storage.GetStorageSize(); i++) {
			bool isBufferSlot = false;
			if (IsGrabed()) {
				int index = bufferSlot.transform.parent.GetComponent<ItemSlot>().SlotIndex;
				if (i == index)
					isBufferSlot = true;
			}

			GetSlot(i).GetComponent<ItemSlot>().SetStack(storage.GetItemStack(i));
			if (isBufferSlot) {
				indexToGrab = i;
			}
		}
	}
}
