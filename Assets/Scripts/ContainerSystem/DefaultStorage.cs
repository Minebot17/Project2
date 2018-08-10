using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Дефолтная реализация storage. Содержит указанное кол-во ячеек
/// </summary>
public class DefaultStorage : NetworkBehaviour, IStorage {
	[SerializeField]
	protected int StorageSize;
	protected ItemStack[] storage;
	protected bool markDirty;

	private void FixedUpdate() {
		if (markDirty) {
			if (ContainerManager.IsOpen(this))
				ContainerManager.UpdateSlots();
			RpcSendDataToClients(Serialize().ToArray());
			markDirty = false;
		}
	}

	public void Initialize() {
		storage = new ItemStack[StorageSize];
	}

	public List<string> Serialize() {
		List<string> result = new List<string>();
		result.Add(StorageSize+"");
		for (int i = 0; i < StorageSize; i++) {
			if (IsEmpty(i))
				result.Add("null");
			else {
				result.Add("not null");
				result.AddRange(GetItemStack(i).Serialize());
			}
		}

		return result;
	}

	public int Deserialize(List<string> data) {
		int result = 1;
		int slot = 0;
		StorageSize = int.Parse(data[0]);
		storage = new ItemStack[StorageSize];
		for (int i = 1; i < data.Count; i++) {
			if (data[i].Equals("null")){
				result++;
				slot++;
			}
			else if (data[i].Equals("not null")) {
				result += 3;
				storage[slot] = new ItemStack();
				List<string> subList = new List<string>(data);
				for (int j = 0; j < i + 1; j++)
					subList.RemoveAt(0);
				i += storage[slot].Deserialize(subList);
				slot++;
			}
		}

		return result;
	}

	public void SetItemStack(int slotId, ItemStack stack) {
		if (slotId >= storage.Length)
			throw new Exception("Вы попытались задать ItemStack, указав несуществующий индекс ячейки");
		storage[slotId] = stack;
		MarkDirty();
		if (!isServer)
			CmdSetItemStack(slotId, stack.Serialize().ToArray());
	}

	public void RemoveItemStack(int slotId) {
		if (slotId >= storage.Length)
			throw new Exception("Вы попытались удалить ItemStack, указав несуществующий индекс ячейки");
		storage[slotId] = null;
		MarkDirty();
		if (!isServer)
			CmdRemoveItemStack(slotId);
	}

	public ItemStack GetItemStack(int slotId) {
		if (slotId >= storage.Length)
			throw new Exception("Вы попытались получить ItemStack, указав несуществующий индекс ячейки");
		return storage[slotId];
	}

	public bool AddItemStack(ItemStack stack) {
		for (int i = 0; i < StorageSize; i++)
			if (IsEmpty(i)) {
				SetItemStack(i, stack);
				return true;
			}

		return false;
	}

	public void SwapItemStacks(int slotIdOne, int slotIdTwo) {
		if (slotIdOne >= storage.Length || slotIdTwo >= storage.Length)
			throw new Exception("Вы попытались получить ItemStack, указав несуществующий индекс ячейки");
		ItemStack buffer = GetItemStack(slotIdOne);
		SetItemStack(slotIdOne, GetItemStack(slotIdTwo));
		SetItemStack(slotIdTwo, buffer);
	}

	public bool SetStackCount(int slotId, int newCount) {
		if (newCount > ItemManager.FindItemInfo(GetItemStack(slotId).ItemName).MaxStackSize)
			return false;
		storage[slotId].StackSize = newCount;
		MarkDirty();
		if (!isServer)
			CmdSetStackCount(slotId, newCount);

		return true;
	}
	
	public void SlotsInteraction(int slotFrom, int slotTo) {
		ItemStack stackFrom = GetItemStack(slotFrom);
		ItemStack stackTo = GetItemStack(slotTo);
		if (IsEmpty(slotTo)) {
			SetItemStack(slotTo, stackFrom);
			RemoveItemStack(slotFrom);
		}
		else if (!stackFrom.ItemName.Equals(stackTo.ItemName))
			SwapItemStacks(slotFrom, slotTo);
		else {
			int maxSize = ItemManager.FindItemInfo(stackFrom.ItemName).MaxStackSize;
			int residue = stackFrom.StackSize + stackTo.StackSize - maxSize;
			if (residue > 0) {
				SetStackCount(slotFrom, residue);
				SetStackCount(slotTo, maxSize);
			}
			else {
				RemoveItemStack(slotFrom);
				SetItemStack(slotTo, new ItemStack(stackFrom.ItemName, stackFrom.StackSize + stackTo.StackSize));
			}
		}
		if (!isServer)
			CmdSlotsInteraction(slotFrom, slotTo);
	}

	public void DropItemStack(int slotId, Vector3 position, Vector3 force) {
		if (!IsEmpty(slotId)) {
			if (!isServer)
				CmdDropItemStack(slotId, position, force);
			else 
				ItemManager.DropItemStack(GetItemStack(slotId), position, force);
			RemoveItemStack(slotId);
		}
	}

	public bool IsEmpty(int slotId) {
		if (slotId >= storage.Length)
			throw new Exception("Вы попытались проверить ItemStack, указав несуществующий индекс ячейки");
		return storage[slotId] == null;
	}

	public void Clear() {
		for (int i = 0; i < StorageSize; i++)
			storage[i] = null;
	}

	public int GetStorageSize() {
		return StorageSize;
	}

	public void MarkDirty() {
		markDirty = true;
	}

	[Command]
	public void CmdDropItemStack(int slotId, Vector3 position, Vector3 force) {
		DropItemStack(slotId, position, force);
	}

	[Command]
	public void CmdSlotsInteraction(int slotFrom, int slotTo) {
		SlotsInteraction(slotFrom, slotTo);
	}

	[Command]
	public void CmdSetItemStack(int slotId, string[] stackData) {
		ItemStack stack = new ItemStack();
		stack.Deserialize(stackData.ToList());
		SetItemStack(slotId, stack);
	}
	
	[Command]
	public void CmdRemoveItemStack(int slotId) {
		RemoveItemStack(slotId);
	}
	
	[Command]
	public void CmdSetStackCount(int slotId, int newCount) {
		SetStackCount(slotId, newCount);
	}

	[ClientRpc]
	public void RpcSendDataToClients(string[] data) {
		Deserialize(data.ToList());
		if (ContainerManager.IsOpen(this))
			ContainerManager.UpdateSlots();
	}

	public override int GetNetworkChannel() {
		return Channels.DefaultReliable;
	}
}
