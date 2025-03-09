using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    public int totalHealth = 5;
    private int remainingHealth;

    // Event system for player damage
    public delegate void OnPlayerDamaged();
    public event OnPlayerDamaged PlayerDamaged;
    // Start is called before the first frame update
    void Start()
    {
        remainingHealth = totalHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (remainingHealth <= 0) {
            //lose game
            SceneManager.LoadScene(0);
        }
    }

    public void playerTakeDamage(int dmg) {
        if (remainingHealth > 0) {
            remainingHealth -= dmg;
            PlayerDamaged?.Invoke();
        }
        if (remainingHealth < 0) {
            remainingHealth = 0;
        }
    }

    public int getRemainingPlayerHealth() {
        return remainingHealth;
    }

}