﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Интерфейс, предназначен для взаимодействия с хранилищем стаков вне контейнера
/// </summary>
public interface IStorage : ISerializableObject {
	
	/// <summary>
	/// Устанавливает указанный стак в указанную ячейку
	/// </summary>
	void SetItemStack(int slotId, ItemStack stack);
	
	/// <summary>
	/// Удаляет стак с указанной ячейки
	/// </summary>
	void RemoveItemStack(int slotId);
	
	/// <summary>
	/// Возвращает стак с указанной ячейки
	/// </summary>
	ItemStack GetItemStack(int slotId);
	
	/// <summary>
	/// Добавляет стак в инвентарь
	/// </summary>
	/// <returns>Было ли добавление было успешно?</returns>
	bool AddItemStack(ItemStack stack);

	/// <summary>
	/// Меняет местами два ItemStack
	/// </summary>
	void SwapItemStacks(int slotIdOne, int slotIdTwo);

	/// <summary>
	/// Задает ItemStack'у под указанной ячейкой
	/// </summary>
	/// <returns></returns>
	bool SetStackCount(int slotId, int newCount);

	/// <summary>
	/// Взаимодействие между двумя слотами. Тип взаимодействия зависит от предметов с слотах. Это может быть слияние или перемещение
	/// </summary>
	void SlotsInteraction(int slotFrom, int slotTo);

	/// <summary>
	/// Выбрасывает предмет в заданной ячейке, с заданной начальной позицией и вектором силы
	/// </summary>
	void DropItemStack(int slotId, int count, Vector3 position, Vector3 force);
	void DropItemStack(int slotId, Vector3 position, Vector3 force);

	/// <summary>
	///	Пустая ли указанная ячейка?
	/// </summary>
	bool IsEmpty(int slotId);

	/// <summary>
	/// Очищает весь инвентарь от всех стаков
	/// </summary>
	void Clear();

	/// <summary>
	/// Получает кол-во слотов в storage
	/// </summary>
	int GetStorageSize();

	/// <summary>
	/// Можно ли установить данный предмет в данный слот?
	/// </summary>
	bool IsValidForSet(int slotId, ItemStack stack);
}
