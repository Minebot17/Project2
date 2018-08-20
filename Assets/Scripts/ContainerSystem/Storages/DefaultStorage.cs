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

	protected virtual void FixedUpdate() {
		if (markDirty) {
			RpcSendDataToClients(Serialize().ToArray());
			markDirty = false;
		}
	}

	public virtual void Initialize() {
		storage = new ItemStack[StorageSize];
	}

	public virtual List<string> Serialize() {
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

	public virtual int Deserialize(List<string> data) {
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

	public virtual void SetItemStack(int slotId, ItemStack stack) {
		if (slotId >= storage.Length)
			throw new Exception("Вы попытались задать ItemStack, указав несуществующий индекс ячейки");
		if (!IsValidForSet(slotId, stack)) {
			Debug.LogWarning("Вы попытались задать ItemStack, указав неподходящую ячейку");
			return;
		}

		storage[slotId] = stack;
		MarkDirty();
		if (!isServer)
			CmdSetItemStack(slotId, stack.Serialize().ToArray());
	}

	public virtual void RemoveItemStack(int slotId) {
		if (slotId >= storage.Length)
			throw new Exception("Вы попытались удалить ItemStack, указав несуществующий индекс ячейки");
		storage[slotId] = null;
		MarkDirty();
		if (!isServer)
			CmdRemoveItemStack(slotId);
	}

	public virtual ItemStack GetItemStack(int slotId) {
		if (slotId >= storage.Length)
			throw new Exception("Вы попытались получить ItemStack, указав несуществующий индекс ячейки");
		return storage[slotId];
	}

	public virtual bool AddItemStack(ItemStack stack) {
		for (int i = 0; i < StorageSize; i++) {
			if (!IsValidForSet(i, stack))
				continue;
			
			if (IsEmpty(i)) {
				SetItemStack(i, stack);
				return true;
			}
			else if (GetItemStack(i).EqualsWithoutSize(stack)) {
				int maxStack = ItemManager.FindItemInfo(GetItemStack(i).ItemName).MaxStackSize;
				if (maxStack >= GetItemStack(i).StackSize + stack.StackSize) {
					SetStackCount(i, GetItemStack(i).StackSize + stack.StackSize);
					return true;
				}
			}
		}

		return false;
	}

	public virtual void SwapItemStacks(int slotIdOne, int slotIdTwo) {
		if (slotIdOne >= storage.Length || slotIdTwo >= storage.Length)
			throw new Exception("Вы попытались получить ItemStack, указав несуществующий индекс ячейки");
		if (!IsValidForSet(slotIdOne, GetItemStack(slotIdTwo)) || !IsValidForSet(slotIdTwo, GetItemStack(slotIdOne))) {
			Debug.LogWarning("Вы попытались задать ItemStack, указав неподходящую ячейку");
			return;
		}

		ItemStack buffer = GetItemStack(slotIdOne);
		SetItemStack(slotIdOne, GetItemStack(slotIdTwo));
		SetItemStack(slotIdTwo, buffer);
	}

	public virtual bool SetStackCount(int slotId, int newCount) {
		if (newCount > ItemManager.FindItemInfo(GetItemStack(slotId).ItemName).MaxStackSize)
			return false;
		if (newCount <= 0)
			RemoveItemStack(slotId);
		else {
			storage[slotId].StackSize = newCount;
			MarkDirty();
		}
		if (!isServer)
			CmdSetStackCount(slotId, newCount);

		return true;
	}
	
	public virtual void SlotsInteraction(int slotFrom, int slotTo) {
		if (slotFrom == slotTo)
			return;
		
		ItemStack stackFrom = GetItemStack(slotFrom);
		ItemStack stackTo = GetItemStack(slotTo);
		if (!IsValidForSet(slotTo, stackFrom))
			return;
		
		if (IsEmpty(slotTo)) {
			SetItemStack(slotTo, stackFrom);
			RemoveItemStack(slotFrom);
		}
		else if (CanSwap(stackFrom, stackTo))
			SwapItemStacks(slotFrom, slotTo);
		else if (stackFrom.EqualsWithoutSize(stackTo)) {
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

	protected virtual bool CanSwap(ItemStack one, ItemStack two) {
		int maxSize = ItemManager.FindItemInfo(one.ItemName).MaxStackSize;
		return (!(one.ItemName.Equals(two.ItemName)) || one.StackSize == maxSize || two.StackSize == maxSize);
	}

	public virtual void DropItemStack(int slotId, Vector3 position, Vector3 force) {
		DropItemStack(slotId, GetItemStack(slotId).StackSize, position, force);
	}

	public virtual void DropItemStack(int slotId, int count, Vector3 position, Vector3 force) {
		if (!IsEmpty(slotId)) {
			if (!isServer) 
				CmdDropItemStack(slotId, position, force);
			else {
				ItemStack stack = GetItemStack(slotId).Copy();
				stack.StackSize = count;
				ItemManager.DropItemStack(stack, position, force);
			}

			SetStackCount(slotId, GetItemStack(slotId).StackSize - count);
		}
	}

	public virtual bool IsEmpty(int slotId) {
		if (slotId >= storage.Length)
			throw new Exception("Вы попытались проверить ItemStack, указав несуществующий индекс ячейки");
		return storage[slotId] == null;
	}

	public virtual void Clear() {
		for (int i = 0; i < StorageSize; i++)
			storage[i] = null;
	}

	public virtual int GetStorageSize() {
		return StorageSize;
	}

	public virtual bool IsValidForSet(int slotId, ItemStack stack) {
		return true;
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
