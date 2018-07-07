using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthPanelGUI : MonoBehaviour {
	[SerializeField]
	private Health health;

	[SerializeField] 
	private Stamina stamina;

	[SerializeField] 
	private Level level;
	
	[SerializeField] 
	private Nickname nick;
	
	[SerializeField]
	private RectTransform hpRect;
	
	[SerializeField]
	private RectTransform staminaRect;

	[SerializeField] 
	private Text hpText;
	
	[SerializeField] 
	private Text levelText;
	
	[SerializeField] 
	private Text nickText;

	private void FixedUpdate() {
		hpRect.sizeDelta = new Vector2((int)((health.Healths/health.MaxHealth.GetCalculated())*100f) , 7);
		hpText.text = health.Healths + "/" + health.MaxHealth.GetCalculated() + " HP";
		staminaRect.sizeDelta = new Vector2((int) (stamina.Procent / 100), 2);
		levelText.text = level.Levels + " LVL";
		nickText.text = nick.Nick;
	}
}
