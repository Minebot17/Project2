using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Класс по управлению системы языков в игре
/// </summary>
public static class LanguageManager {
	
	/// <summary>
	/// Список из всех языков
	/// </summary>
	public static readonly List<Language> Languages = new List<Language>();
	
	/// <summary>
	/// Текущий установленный язык
	/// </summary>
	public static Language CurrentLanguage = null;
	
	/// <summary>
	/// Эвент, который вызывается каждый раз, когда пользователь меняет язык
	/// </summary>
	public static EventHandler<EventBase> ChangeLanguageEvent = new EventHandler<EventBase>();

	public static void Initialize() {
		TextAsset[] langAssets = Resources.LoadAll<TextAsset>("Lang");
		foreach (TextAsset asset in langAssets) {
			List<string> lines = Encoding.UTF8.GetString(asset.bytes).Split(new [] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries).ToList();
			lines.RemoveAll(x => x[0].Equals('#') );
			Dictionary<string, string> toLang = new Dictionary<string, string>();
			for (int i = 1; i < lines.Count; i++) {
				string[] splitted = lines[i].Split(new [] {'='}, StringSplitOptions.RemoveEmptyEntries);
				try {
					toLang.Add(splitted[0], splitted[1]);
				}
				catch (IndexOutOfRangeException e) {
					Debug.Log(e);
				}
			}

			string[] languageInfo = lines[0].Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
			Languages.Add(new Language(languageInfo[0], languageInfo[1], (SystemLanguage) int.Parse(languageInfo[2]), toLang));
		}

		SetDefaultLanguage();
	}

	/// <summary>
	/// Установить английский язык
	/// </summary>
	public static void SetDefaultLanguage() {
		SetLanguage(x => x.Code.Equals("en"));
	}

	/// <summary>
	/// Установить существующий язык, который будет выбран заданным условием
	/// </summary>
	public static bool SetLanguage(Predicate<Language> predicate) {
		Language toCurrent = Languages.Find(predicate);
		if (toCurrent != null)
			SetLanguage(toCurrent);
		return toCurrent != null;
	}

	/// <summary>
	/// Установить заданный язык
	/// </summary>
	/// <param name="language">Язык для установки</param>
	public static void SetLanguage(Language language) {
		CurrentLanguage = language;
		ChangeLanguageEvent.CallListners(new EventBase(null, false));
	}

	/// <summary>
	/// Получить предложение из языка по ключю в файле языка
	/// </summary>
	/// <param name="key">Ключ</param>
	/// <returns>Переведенное предложение</returns>
	public static string GetValue(string key) {
		return CurrentLanguage.Dictionary[key];
	}

	public class Language {
		public string Name;
		public string Code;
		public SystemLanguage System;
		public Dictionary<string, string> Dictionary;
		
		public Language(string name, string code, SystemLanguage system, Dictionary<string, string> dictionary) {
			Name = name;
			Code = code;
			System = system;
			Dictionary = dictionary;
		}
	}
}
