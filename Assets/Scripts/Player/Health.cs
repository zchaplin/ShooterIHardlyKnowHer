using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;  // Maximum health the object can have.
    private int currentHealth;   // Current health of the object.

    void Start()
    {
        currentHealth = maxHealth;  // Initialize health to maximum at the start.
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;  // Reduce health by the damage amount.
        Debug.Log("Health now: " + currentHealth);  // Output the current health to the console.

        if (currentHealth <= 0)
        {
            Die();  // Call the Die method if health is 0 or less.
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died.");  // Log the death event.
        // Here you can add what happens when the object dies, like playing an animation or destroying the object.
        Destroy(gameObject);  // Destroys the object on death.
    }
}