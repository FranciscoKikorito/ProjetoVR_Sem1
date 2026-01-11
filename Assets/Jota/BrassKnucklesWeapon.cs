using UnityEngine;

public class BrassKnucklesWeapon : WeaponBase
{
    [Header("Brass Knuckles Settings")]
    public int armorBonus = 20;
    public float attackSpeedBonus = 0.2f; // Ataques mais rápidos

    private int originalArmor;
    private float originalAttackSpeed; // Você pode precisar adicionar attackSpeed ao Player
    private bool effectApplied = false;

    public override void ApplyWeaponEffect(Player targetPlayer)
    {
        if (targetPlayer == null || effectApplied) return;

        // Guardar valores originais
        originalArmor = targetPlayer.currentStats.armor;

        // Aplicar bônus de armor
        targetPlayer.currentStats.armor += armorBonus;

        // Aplicar bônus de velocidade de ataque (se seu sistema tiver)
        // targetPlayer.currentStats.attackSpeed += attackSpeedBonus;

        effectApplied = true;
        Debug.Log($"Brass Knuckles equipados! Armor aumentado para {targetPlayer.currentStats.armor}");
    }

    public override void RemoveWeaponEffect(Player targetPlayer)
    {
        if (targetPlayer == null || !effectApplied) return;

        // Restaurar valores originais
        targetPlayer.currentStats.armor = originalArmor;

        // Restaurar velocidade de ataque
        // targetPlayer.currentStats.attackSpeed = originalAttackSpeed;

        effectApplied = false;
        Debug.Log($"Brass Knuckles removidos! Armor restaurado para {originalArmor}");
    }

    protected override void InitializeWeapon()
    {
        base.InitializeWeapon();

        // Brass knuckles são leves, então threshold de velocidade pode ser menor
        velocityThreshold = 1.0f;
        hitCooldown = 0.2f; // Ataques mais rápidos
    }

    protected override int CalculateDamage()
    {
        int baseDamageResult = base.CalculateDamage();

        // Brass knuckles tem dano extra baseado na armor do jogador
        if (player != null)
        {
            float armorBonusDamage = player.currentStats.armor * 0.1f;
            baseDamageResult += Mathf.RoundToInt(armorBonusDamage);
        }

        return baseDamageResult;
    }

    protected override void OnHitEffect(GameObject enemy)
    {
        // Efeito de impacto forte (soco metálico)
        if (audioSource != null)
        {
            audioSource.pitch = Random.Range(1.0f, 1.3f);
        }

        // Pequeno efeito de impacto
        GameObject impactEffect = new GameObject("KnucklesImpact");
        impactEffect.transform.position = transform.position;
        ParticleSystem ps = impactEffect.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startSpeed = 2f;
        main.startLifetime = 0.5f;
        main.startSize = 0.1f;
        main.startColor = Color.gray;

        Destroy(impactEffect, 0.5f);
    }
}
