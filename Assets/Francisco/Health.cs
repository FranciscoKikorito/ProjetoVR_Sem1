using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    private int currentHealth;
    private Spawner spawner;
    private float maxHealth;

    // ADD ↓↓↓
    private Animator animator;
    private bool isDead = false;

    void Awake()
    {
    }

    // Called by spawner when spawning
    public void InitializeEnemy(float healthValue, Spawner spawnerRef)
    {
        maxHealth = healthValue;
        currentHealth = Mathf.RoundToInt(maxHealth);
        spawner = spawnerRef;
        Debug.Log($"{gameObject.name} initialized with {currentHealth} health");
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return; // ADD

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Remaining: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return; // ADD
        isDead = true;

        Debug.Log($"{gameObject.name} died!");

        // ADD ↓↓↓ trigger animation
        if (animator != null)
            animator.SetBool("IsDead", true);

        // Notify the spawner
        if (spawner != null)
        {
            spawner.NotifyDeath();
        }

        // CHANGE ↓↓↓ delay destroy
        Destroy(gameObject, 3f); // match death animation length
    }
}
