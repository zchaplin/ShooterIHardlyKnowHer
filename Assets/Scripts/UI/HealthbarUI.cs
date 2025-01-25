using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealthbarUI : MonoBehaviour
{
    private TMP_Text healthText;
    private HealthManager health;
    // Start is called before the first frame update
    void Start()
    {

        healthText = GetComponent<TMP_Text>();

        GameObject healthManagerObject = GameObject.Find("healthManager");
        health = healthManagerObject.GetComponent<HealthManager>();
    }

    // Update is called once per frame
    void Update()
    {
        healthText.text = "HEALTH: " + health.getRemainingPlayerHealth().ToString();
    }
}
