using System.Collections;
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
	///	Пустая ли указанная ячейка?
	/// </summary>
	bool IsEmpty(int slotId);

	/// <summary>
	/// Очищает весь инвентарь от всех стаков
	/// </summary>
	void Clear();
}
