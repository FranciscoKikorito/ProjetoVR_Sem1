using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour, IDamageable
{
    private int currentHealth;
    private Spawner spawner;
    private float maxHealth;

    private Animator animator;
    private Rigidbody rb;

    [SerializeField] private float deathDelay = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    public void InitializeEnemy(float healthValue, Spawner spawnerRef)
    {
        maxHealth = healthValue;
        currentHealth = Mathf.RoundToInt(maxHealth);
        spawner = spawnerRef;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        // Notify spawner
        if (spawner != null)
            spawner.NotifyDeath(this.gameObject);

        // Destroy
        Destroy(gameObject);
    }


    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);
    }
}
