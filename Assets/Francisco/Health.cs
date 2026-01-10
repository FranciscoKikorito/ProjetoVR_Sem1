using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    private int currentHealth;
    private Spawner spawner;
    private float maxHealth;

    private Animator animator;
    private Rigidbody rb;
    private bool isDead = false;

    [SerializeField] private float deathDelay = 3f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    // Called by spawner when spawning
    public void InitializeEnemy(float healthValue, Spawner spawnerRef)
    {
        maxHealth = healthValue;
        currentHealth = Mathf.RoundToInt(maxHealth);
        spawner = spawnerRef;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
    }

    /*
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        
        if (animator != null)
            animator.SetBool("IsDead", true);

        // 🔒 Freeze Rigidbody position & rotation
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        StartCoroutine(DeathRoutine());
    }

    private System.Collections.IEnumerator DeathRoutine()
    {
        &&yield return new WaitForSeconds(deathDelay);

        if (spawner != null)
            spawner.NotifyDeath();

        Destroy(gameObject);
    }*/


    private void Die()
    {
        if (spawner != null)
            spawner.NotifyDeath();

        Destroy(gameObject);
    }
}
