using UnityEngine;

public class TankardWeapon : WeaponBase
{
    [Header("Tankard Settings")]
    public float alcoholBoostMultiplier = 4f;
    public float alcoholBoostDivisor = 3f;

    private float originalAlcoholAmplification;
    private bool effectApplied = false;

    public override void ApplyWeaponEffect(Player targetPlayer)
    {
        if (targetPlayer == null || effectApplied) return;

        // Guardar valor original
        originalAlcoholAmplification = targetPlayer.currentStats.alcoholAmplification;

        // Fórmula: (valor_atual / 3) * 4
        float currentAlcohol = targetPlayer.currentStats.alcoholAmplification;
        float dividedValue = currentAlcohol / alcoholBoostDivisor;
        float newAlcoholValue = dividedValue * alcoholBoostMultiplier;

        // Aplicar novo valor (garantir mínimo de 1.0)
        targetPlayer.currentStats.alcoholAmplification = Mathf.Max(1.0f, newAlcoholValue);

        effectApplied = true;
        Debug.Log($"Tankard equipado! Alcohol Amplification aumentado para {targetPlayer.currentStats.alcoholAmplification}");
    }

    public override void RemoveWeaponEffect(Player targetPlayer)
    {
        if (targetPlayer == null || !effectApplied) return;

        // Restaurar valor original
        targetPlayer.currentStats.alcoholAmplification = originalAlcoholAmplification;

        effectApplied = false;
        Debug.Log($"Tankard removido! Alcohol Amplification restaurado para {originalAlcoholAmplification}");
    }

    protected override void OnHitEffect(GameObject enemy)
    {
        // Efeito especial: partículas de cerveja/espuma
        if (hitEffects.Length > 0)
        {
            GameObject beerEffect = Instantiate(hitEffects[0], transform.position, Quaternion.identity);
            Destroy(beerEffect, 1f);
        }
    }

    protected override int CalculateDamage()
    {
        int baseDamageResult = base.CalculateDamage();

        // Tankard tem dano extra baseado no alcohol amplification
        if (player != null)
        {
            float alcoholBonus = (player.currentStats.alcoholAmplification - 1.0f) * 10f;
            baseDamageResult += Mathf.RoundToInt(alcoholBonus);
        }

        return baseDamageResult;
    }
}
