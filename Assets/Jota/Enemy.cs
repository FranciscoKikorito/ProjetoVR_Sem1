using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float currentHP;
    [SerializeField] private float speed;

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        StartCoroutine(HitFlash());

        // Verifica se o inimigo morreu
        if (currentHP <= 0)
        {
            Die();
        }

        Debug.Log("Enemy Hit, remaining HP: " + currentHP);
    }

    IEnumerator HitFlash()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Color originalColor = meshRenderer.material.color;
            meshRenderer.material.color = Color.red;

            yield return new WaitForSeconds(0.1f);

            meshRenderer.material.color = originalColor;
        }
    }

    void Die()
    {
        Destroy(gameObject);

        Debug.Log("Enemy defeated");
    }
}
