using UnityEngine;

public class BaseballBatWeapon : WeaponBase
{
    [Header("Baseball Bat Settings")]
    public float damageMultiplier = 2f;
    public float swingForce = 2f;

    private float originalAttackDamage;
    private bool effectApplied = false;

    public override void ApplyWeaponEffect(Player targetPlayer)
    {
        if (targetPlayer == null || effectApplied) return;

        // Guardar valor original
        originalAttackDamage = targetPlayer.currentStats.attackDamage;

        // Aplicar multiplicador
        targetPlayer.currentStats.attackDamage = Mathf.RoundToInt(originalAttackDamage * damageMultiplier);

        effectApplied = true;
        Debug.Log($"Baseball Bat equipado! AD multiplicado por {damageMultiplier}x");
    }

    public override void RemoveWeaponEffect(Player targetPlayer)
    {
        if (targetPlayer == null || !effectApplied) return;

        // Restaurar valor original
        targetPlayer.currentStats.attackDamage = Mathf.RoundToInt(originalAttackDamage);

        effectApplied = false;
        Debug.Log("Baseball Bat removido! AD restaurado");
    }

    protected override void HandleHit(Collision collision)
    {
        base.HandleHit(collision);

        // Efeito extra do baseball bat: pode empurrar inimigos
        Rigidbody enemyRb = collision.gameObject.GetComponent<Rigidbody>();
        if (enemyRb != null)
        {
            Vector3 hitDirection = (collision.transform.position - transform.position).normalized;
            enemyRb.AddForce(hitDirection * swingForce, ForceMode.Impulse);
        }
    }

    protected override void OnHitEffect(GameObject enemy)
    {
        // Efeito especial: som de batida forte
        if (audioSource != null)
        {
            audioSource.pitch = Random.Range(0.8f, 1.2f);
        }
    }
}
