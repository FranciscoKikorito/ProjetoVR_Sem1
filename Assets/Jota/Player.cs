using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [SerializeField]
    private GameObject[] weapons;

    public int augmentCount;
    public int killCount;

    [SerializeField]
    public Stats playerBaseStats;
    public Stats currentStats;

    public float currentHP;

    public List<Augment> activeAugments = new List<Augment>();

    public static Player instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentStats = Instantiate(playerBaseStats);
        currentHP = playerBaseStats.health;
    }

    public void ApplyAugment(Augment augment)
    {
        activeAugments.Add(augment);
        augmentCount++;

        if (augment.augmentType == AugmentType.StatUpgrade)
        {
            ApplyStatAugment(augment);
        }
        else if (augment.augmentType == AugmentType.Weapon)
        {
            ApplyWeaponAugment(augment);
        }
    }

    private void ApplyStatAugment(Augment augment)
    {
        switch (augment.statType)
        {
            case StatType.Health:
                currentStats.health += augment.statValue;
                currentHP += augment.statValue; // Curar o valor adicional
                break;
            case StatType.DamageMultiplier:
                // Alcohol amplification é o multiplicador de dano
                currentStats.alcoholAmplification += augment.statValue;
                break;
            case StatType.Defense:
                currentStats.armor += (int)augment.statValue;
                break;
            case StatType.CriticalChance:
                currentStats.critChance += (int)augment.statValue;
                currentStats.critChance = Mathf.Min(currentStats.critChance, 100); // Max 100%
                break;
            case StatType.LifeSteal:
                // Você pode implementar lifesteal depois
                break;
        }
    }

    private void ApplyWeaponAugment(Augment augment)
    {
        if (augment.weaponPrefab != null)
        {
            // Encontrar slot apropriado para a arma
            Transform weaponSlot = null;

            // Você pode adaptar isso para seu sistema de armas
            if (weapons != null && weapons.Length > 0)
            {
                // Aqui você pode implementar lógica para equipar a arma
                // Por exemplo, substituir uma arma existente
            }

            // Instantiate a nova arma
            GameObject newWeapon = Instantiate(augment.weaponPrefab, transform.position, Quaternion.identity);

            // Configurar a arma (você pode precisar adaptar)
            IWeapons weaponComponent = newWeapon.GetComponent<IWeapons>();
            if (weaponComponent != null)
            {
                weaponComponent.ApplyBonusStat();
            }
        }
    }

    private float CalculateDamageReduction (float armor)
    {
        return playerBaseStats.armor / (armor + 100f);
    }

    private int CalculateDamageAfterArmor( int incomingDamage, float armor)
    {
        float damageReduction = CalculateDamageReduction (armor);

        float damageAfterReduction = incomingDamage * (1f - damageReduction);

        return Mathf.RoundToInt (damageAfterReduction);
    }

    public void TakeDamage(int damage)
    {

        int finalDamage = CalculateDamageAfterArmor(damage,playerBaseStats.armor);
        currentHP -= finalDamage;
        if (currentHP <= 0) 
        { 
            OnPlayerDeath();
        }
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Min(currentHP + amount, currentStats.health);
    }


    public void AddArmor(int amount)
    {
        playerBaseStats.armor += amount;
        playerBaseStats.armor = Mathf.Max(0, playerBaseStats.armor);
    }

    public void SetArmor(int newArmor)
    {
        playerBaseStats.armor = Mathf.Max (0, newArmor);
    }

    public void ResetArmor()
    {
        playerBaseStats.armor = 0;
    }

    public void OnPlayerDeath()
    {

    }

    public int CalculatePlayerDamage(int baseDamage)
    {
        float damage = baseDamage;

        // Aplicar alcohol amplification
        damage *= currentStats.alcoholAmplification;

        // Verificar crítico
        bool isCrit = Random.value <= (currentStats.critChance / 100f);

        if (isCrit)
        {
            damage *= currentStats.critDamage;
        }

        return Mathf.RoundToInt(damage);
    }
}
