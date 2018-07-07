using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Пользовательский таймер для Unity by Minebot
/// </summary>
public class Timer {
	private static GameObject storage;

	/// <summary>
	/// Инициализация алгоритма. Должна вызываться 1 раз при запуске сцены. Создает таймер
	/// </summary>
	public static void InitializeCreate() {
		storage = new GameObject("Timers");
	}

	/// <summary>
	/// Инициализация алгоритма. Должна вызываться 1 раз при запуске сцены. Присваивает уже имеющийся таймер
	/// </summary>
	public static void InitializeFind() {
		storage = GameObject.Find("Timers");
	}

	/// <summary>
	/// Создает новый таймер, который отсчитывает заданное время, затем вызывает метод
	/// </summary>
	/// <param name="name">Имя таймера</param>
	/// <param name="duration">Задержка таймера до вызова метода (-1 - бесконечность)</param>
	/// <param name="countOfRepeat">Число повторений (-1 - бесконечность)</param>
	/// <param name="parent">Родитель таймера. Если удалят родителя, то удалится и сам таймер. null для отключения</param>
	/// <param name="method">Метод, который будет вызываться</param>
	/// <returns>Объект таймера</returns>
	public static ITimer StartNewTimer(string name, float duration, int countOfRepeat, GameObject parent, Action<ITimer> method) {
		TimerSkin skin = storage.AddComponent<TimerSkin>();
		skin.Initialize(new StandartTimer(), name, duration, countOfRepeat, method, parent);
		return (ITimer) skin.GetTimer();
	}

	/// <summary>
	/// Создает новый таймер, который отсчитывает заданное время, затем вызывает метод.
	/// Второй метод вызывается каждый кадр, пока таймер работает
	/// </summary>
	/// <param name="name">Имя таймера</param>
	/// <param name="duration">Задержка таймера до вызова метода (-1 - бесконечность)</param>
	/// <param name="countOfRepeat">Число повторений (-1 - бесконечность)</param>
	/// <param name="parent">Родитель таймера. Если удалят родителя, то удалится и сам таймер. null для отключения</param>
	/// <param name="method">Метод, который будет вызываться</param>
	/// <param name="intervalMethod">Метод, вызывающийся каждый кадр (второй параметр - время рендра кадра, третий - сколько осталось времени, четвертый - общее время таймера)</param>
	/// <returns>Объект таймера</returns>
	public static ITimer StartNewTimer(string name, float duration, int countOfRepeat, GameObject parent, Action<ITimer> method, Action<ITimer, float, float, float> intervalMethod) {
		TimerSkin skin = storage.AddComponent<TimerSkin>();
		skin.Initialize(new IntervalTimer(), name, duration, countOfRepeat, method, intervalMethod, parent);
		return (ITimer) skin.GetTimer();
	}

	/// <summary>
	/// Создание кастомного таймера
	/// </summary>
	/// <param name="customTimer">Объект таймера</param>
	/// <param name="args">Аргументы для его инициализции</param>
	/// <returns>Объект таймера</returns>
	public static ITimer StartNewTimer(ITimer customTimer, params System.Object[] args) {
		TimerSkin skin = storage.AddComponent<TimerSkin>();
		skin.Initialize(customTimer, args);
		return skin.GetTimer();
	}

	/// <summary>
	/// Находит таймер по его имени
	/// </summary>
	/// <param name="name">Имя таймера</param>
	/// <returns>Объект таймера. Если таймер не нашелся, то возвращает null</returns>
	public static ITimer FindTimer(String name) {
		TimerSkin[] timers = storage.GetComponents<TimerSkin>();
		foreach (TimerSkin timer in timers)
			if (timer.GetTimer().GetName().Equals(name))
				return timer.GetTimer();
		return null;
	}

	/// <summary>
	/// Оболочка для интерфейса ITimer
	/// </summary>
	public class TimerSkin : MonoBehaviour {
		private ITimer timer;

		public void Initialize(ITimer timer, params System.Object[] args) {
			this.timer = timer;
			this.timer.Initialize(this, args);
		}

		private void Update() {
			timer.Update();
		}

		public ITimer GetTimer() {
			return timer;
		}
	}

	/// <summary>
	/// Интерфейс таймера
	/// </summary>
	public interface ITimer {
		/// <summary>
		/// Инициализация таймера. Тут вы должны определить все требуемые для работы поля. Все аргументы, которые вы передали при создании таймера находятся в args.
		/// Также, если вы хотите удалить компонент таймера, то используйте для этого skin: MonoBehaviour.Destroy(skin)
		/// Другие действия с компонентом совершайте тоже, используя skin
		/// </summary>
		/// <param name="skin">Оболочка для интерфейса</param>
		/// <param name="args">Переданные аргументы при создании таймера</param>
		void Initialize(TimerSkin skin, params System.Object[] args);
		void Update();
		void Remove();
		void Stop();
		void ReStart();
		float GetLastTime();
		string GetName();
	}

	/// <summary>
	/// Реализация стандартного таймера
	/// </summary>
	public class StandartTimer : ITimer {
		protected TimerSkin skin;
		public string timerName;
		public float duration;
		public int countOfRepeat;
		public GameObject parent;
		protected bool haveParent;
		protected Action<ITimer> method;
		protected bool stop;
		public float n;
		protected bool isInited;

		public virtual void Initialize(TimerSkin skin, params System.Object[] args) {
			if (isInited)
				return;

			this.skin = skin;
			isInited = true;
			timerName = (string) args[0];
			duration = (float) args[1];
			countOfRepeat = (int) args[2];
			method = (Action<ITimer>) args[3];
			if (args[4] != null) {
				haveParent = true;
				parent = (GameObject) args[4];
			}
			n = duration;
		}

		public virtual void Update() {
			if (!stop) {
				if (n <= 0) {
					if (haveParent && parent == null) {
						MonoBehaviour.Destroy(skin);
						return;
					}

					method(this);
					countOfRepeat--;
					if (countOfRepeat == 0)
						MonoBehaviour.Destroy(skin);
					else
						n = duration;
				}
				else
					n -= Time.deltaTime;
			}
		}

		/// <summary>
		/// Удалить данный таймер
		/// </summary>
		public void Remove() {
			MonoBehaviour.Destroy(skin);
		}

		/// <summary>
		/// Приостановить таймер
		/// </summary>
		public void Stop() {
			stop = true;
		}

		/// <summary>
		/// Возабновить таймер (начинается с момента его остановки)
		/// </summary>
		public void ReStart() {
			stop = false;
		}

		/// <summary>
		/// Возвращает оставшиеся время
		/// </summary>
		public float GetLastTime() {
			return n;
		}

		/// <summary>
		/// Получить имя данного таймера
		/// </summary>
		/// <returns>Имя таймера</returns>
		public string GetName() {
			return timerName;
		}
	}

	/// <summary>
	/// Реализация таймера, который вызывает метод после каждого прорисованного кадра
	/// </summary>
	public class IntervalTimer : StandartTimer {
		public Action<ITimer, float, float, float> interval;

		public override void Initialize(TimerSkin skin, params System.Object[] args) {
			if (isInited)
				return;

			this.skin = skin;
			isInited = true;
			timerName = (string) args[0];
			duration = (float) args[1];
			countOfRepeat = (int) args[2];
			method = (Action<ITimer>) args[3];
			interval = (Action<ITimer, float, float, float>) args[4];
			if (args[5] != null) {
				haveParent = true;
				parent = (GameObject) args[5];
			}
			n = duration;
		}

		public override void Update() {
			if (stop == false) {
				if (haveParent && parent == null) {
					MonoBehaviour.Destroy(skin);
					return;
				}

				if (n <= 0) {
					if ((object) method != (object) null)
						method(this);

					countOfRepeat--;
					if (countOfRepeat == 0)
						MonoBehaviour.Destroy(skin);
					else
						n = duration;
				}
				else {
					n -= Time.deltaTime; 
					interval(this, Time.deltaTime, n, duration);
				}
			}
		}
		
		public void Remove() {
			MonoBehaviour.Destroy(skin);
		}
	}
}