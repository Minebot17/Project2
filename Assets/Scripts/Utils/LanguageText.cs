using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Lang Text")]
public class LanguageText : Text {

	public string LangKey;

	private void Start () {
		text = LanguageManager.GetValue(LangKey);
		LanguageManager.ChangeLanguageEvent.SubcribeEvent(x => text = LanguageManager.GetValue(LangKey));
	}
}
