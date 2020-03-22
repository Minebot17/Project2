using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Хранилище параметров вашего Entity. Их можно задать в редакторе
/// Так же от сюда вы можете брать/записывать информацию из любого места архитектуры. Информация может быть модифицированная эффектами
/// Отсюда происходит регулирование анимаций, путем поимки эвентов. Например так GetEventSystem(RunEvent)().SubcribeEvent(x => animator.SetBool("Run", true));
/// </summary>
public class EntityInfo : MonoBehaviour {

	/// <summary>
	/// Если AI выключенно, то TaskHandler не будет обрабатывать задачи
	/// </summary>
	public bool EnableAI = true;
	
	public virtual void Start () {
		GetComponent<SimpleObject>().Initialize();
	}
}
