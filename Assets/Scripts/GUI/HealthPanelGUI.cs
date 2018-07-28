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
	private GameProfile profile;
	
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
		hpRect.sizeDelta = new Vector2((int)((health.HealthValue/health.MaxHealth.GetCalculated())*100f) , 7);
		hpText.text = health.HealthValue + "/" + health.MaxHealth.GetCalculated() + " HP";
		staminaRect.sizeDelta = new Vector2((int)((stamina.StaminaValue / stamina.MaxStamina.GetCalculated()) * 101f), 2);
		levelText.text = profile.Level + " LVL";
		nickText.text = profile.ProfileName;
	}
	
	public void SetTarget(GameObject gameObject) {
		health = gameObject.GetComponent<Health>();
		stamina = gameObject.GetComponent<Stamina>();
		profile = gameObject.GetComponent<GameProfile>();
	}
}
