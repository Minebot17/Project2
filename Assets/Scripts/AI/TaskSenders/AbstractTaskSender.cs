using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// В Start подписывается на эвенты нужного Observer, и отсылает их методом ITaskHandler#ReceiveEvent обработчику
/// В FixedUpdate на основе текущего IEntityState в ITaskHandler посылает нужный ITask вашему ITaskHandler'у
/// Этот компонент отвечает за логику конкретной части поведения вашего моба
/// </summary>
/// <typeparam name="T">ITaskHandler вашего моба</typeparam>
/// <typeparam name="M">EntityInfo вашего моба</typeparam>
public class AbstractTaskSender<T, M> : MonoBehaviour, ITaskSender where T : ITaskHandler where M : EntityInfo {
	protected T handler;
	protected M info;
	
	protected virtual void Start() {
		handler = (T) GetComponent<ITaskHandler>();
		info = (M) GetComponent<EntityInfo>();
	}
}
