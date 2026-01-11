using UnityEngine;

public class BananaWeapon : WeaponBase
{
    [Header("Banana Settings")]
    public bool isSuperBanana = false;
    public float critDamageBoost = 1.0f; // 100% para banana normal
    public float superCritDamageBoost = 5.0f; // 500% para super banana

    private float originalCritDamage;
    private bool effectApplied = false;

    void Start()
    {
        base.Start();

        // Definir boost baseado no tipo
        if (isSuperBanana)
        {
            critDamageBoost = superCritDamageBoost;
            Debug.Log("SUPER BANANA equipada! +500% Crit Damage");
        }
        else
        {
            Debug.Log("Banana equipada! +100% Crit Damage");
        }
    }

    public override void ApplyWeaponEffect(Player targetPlayer)
    {
        if (targetPlayer == null || effectApplied) return;

        // Guardar valor original
        originalCritDamage = targetPlayer.currentStats.critDamage;

        // Aplicar boost de crit damage
        targetPlayer.currentStats.critDamage += critDamageBoost;

        effectApplied = true;
        Debug.Log($"Crit Damage aumentado para {targetPlayer.currentStats.critDamage}x");
    }

    public override void RemoveWeaponEffect(Player targetPlayer)
    {
        if (targetPlayer == null || !effectApplied) return;

        // Restaurar valor original
        targetPlayer.currentStats.critDamage = originalCritDamage;

        effectApplied = false;
        Debug.Log($"Crit Damage restaurado para {originalCritDamage}x");
    }

    protected override void OnCriticalHit()
    {
        base.OnCriticalHit();

        // Efeito visual especial para crítico com banana
        if (isSuperBanana)
        {
            // Efeito de super crítico
            GameObject superCritEffect = new GameObject("SuperCritEffect");
            Light light = superCritEffect.AddComponent<Light>();
            light.color = Color.yellow;
            light.intensity = 5f;
            light.range = 10f;
            Destroy(superCritEffect, 0.5f);

            // Tocar som especial
            if (audioSource != null && hitSounds.Length > 0)
            {
                audioSource.pitch = 1.5f;
                audioSource.PlayOneShot(hitSounds[0]);
            }
        }
        else
        {
            // Efeito normal de crítico
            GameObject critEffect = new GameObject("CritEffect");
            Light light = critEffect.AddComponent<Light>();
            light.color = Color.yellow;
            light.intensity = 2f;
            light.range = 5f;
            Destroy(critEffect, 0.3f);
        }
    }

    protected override void OnHitEffect(GameObject enemy)
    {
        // Chance de deixar o inimigo escorregando (efeito da banana)
        if (Random.value <= 0.3f && enemy.GetComponent<Rigidbody>() != null)
        {
            StartCoroutine(SlipEffect(enemy));
        }
    }

    private System.Collections.IEnumerator SlipEffect(GameObject enemy)
    {
        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
        if (enemyRb != null)
        {
            // Adicionar força aleatória para simular escorregão
            Vector3 slipForce = new Vector3(
                Random.Range(-2f, 2f),
                0,
                Random.Range(-2f, 2f)
            );
            enemyRb.AddForce(slipForce, ForceMode.Impulse);

            // Mudar cor temporariamente
            Renderer renderer = enemy.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color originalColor = renderer.material.color;
                renderer.material.color = Color.yellow;

                yield return new WaitForSeconds(1f);

                renderer.material.color = originalColor;
            }
        }
    }
}
