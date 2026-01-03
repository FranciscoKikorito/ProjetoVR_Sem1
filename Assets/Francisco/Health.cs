using UnityEngine;

public class Health : MonoBehaviour
{
    private int currentHealth;
    private Spawner spawner;
    private float maxHealth;

    // Called by spawner when spawning
    public void InitializeEnemy(float healthValue, Spawner spawnerRef)
    {
        maxHealth = healthValue;
        currentHealth = Mathf.RoundToInt(maxHealth);
        spawner = spawnerRef;
        Debug.Log($"{gameObject.name} initialized with {currentHealth} health");
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. Remaining: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        // Notify the spawner that an enemy died
        if (spawner != null)
        {
            spawner.NotifyDeath();
        }
        Destroy(gameObject);
    }
}
