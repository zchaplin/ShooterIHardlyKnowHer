using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class HealthbarUI : MonoBehaviour
{
    public LocalizedString healthLocalizedString;
    private TMP_Text healthText;
    private HealthManager health;

    void Start()
    {
        healthText = GetComponent<TMP_Text>();

        GameObject healthManagerObject = GameObject.Find("healthManager");
        health = healthManagerObject.GetComponent<HealthManager>();

        healthLocalizedString.StringChanged += UpdateHealthText;
    }

    void Update()
    {
        UpdateHealthText(healthLocalizedString.GetLocalizedString());
    }

    void UpdateHealthText(string localizedHealth)
    {
        healthText.text = localizedHealth + " " + health.getRemainingPlayerHealth().ToString();
    }

    private void OnDestroy()
    {
        healthLocalizedString.StringChanged -= UpdateHealthText;
    }
}
