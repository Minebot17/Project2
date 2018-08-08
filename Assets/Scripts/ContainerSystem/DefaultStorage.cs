using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Дефолтная реализация storage. Содержит указанное кол-во ячеек
/// </summary>
public class DefaultStorage : MonoBehaviour, IStorage {
	public int StorageSize;
	protected ItemStack[] storage;
	
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
				ItemStack stack = GetItemStack(i);
				result.Add(stack.ItemName);
				result.Add(stack.StackSize+"");
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
				storage[slot] = new ItemStack(data[i + 1], int.Parse(data[i + 2]));
				i += 2;
				slot++;
			}
		}

		return result;
	}

	public void SetItemStack(int slotId, ItemStack stack) {
		if (slotId >= storage.Length)
			throw new Exception("Вы попытались задать ItemStack, указав несуществующий индекс ячейки");
		storage[slotId] = stack;
	}

	public void RemoveItemStack(int slotId) {
		if (slotId >= storage.Length)
			throw new Exception("Вы попытались удалить ItemStack, указав несуществующий индекс ячейки");
		storage[slotId] = null;
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

	public bool IsEmpty(int slotId) {
		if (slotId >= storage.Length)
			throw new Exception("Вы попытались проверить ItemStack, указав несуществующий индекс ячейки");
		return storage[slotId] == null;
	}

	public void Clear() {
		for (int i = 0; i < StorageSize; i++)
			storage[i] = null;
	}
}
