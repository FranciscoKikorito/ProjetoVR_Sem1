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

    public int enemyDamagePerHit;

    public List<Augment> activeAugments = new List<Augment>();

    public static Player instance;

    private bool isInvincible = false;
    [SerializeField] private float invincibilityDuration = 3f;


    [Header("Audio")]
    public AudioClip[] hitSounds;
    public AudioSource hitAudioSource;

    [Header("Weapon System")]
    public WeaponManager weaponManager;
    public Transform leftHand;  // Transform da mão esquerda
    public Transform rightHand; // Transform da mão direita


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
        currentHP = currentStats.health;
        enemyDamagePerHit = 25;

        // Inicializar WeaponManager
        InitializeWeaponManager();
    }

    private void InitializeWeaponManager()
    {
        if (weaponManager == null)
            weaponManager = GetComponent<WeaponManager>();

        if (weaponManager == null)
            weaponManager = gameObject.AddComponent<WeaponManager>();

        // Configurar mãos
        weaponManager.leftHand = leftHand;
        weaponManager.rightHand = rightHand;
    }

    private void ApplyWeaponAugment(Augment augment)
    {
        if (augment.weaponPrefab != null)
        {
            // Equipar arma usando o WeaponManager
            if (weaponManager != null)
            {
                weaponManager.EquipWeapon(augment.weaponPrefab, augment.weaponSlot);

                // Atualizar dano base do jogador com o dano da arma
                UpdatePlayerAttackDamage();
            }
        }
    }

    private void UpdatePlayerAttackDamage()
    {
        if (weaponManager != null)
        {
            // Adicionar dano das armas ao dano base do jogador
            int weaponDamage = weaponManager.GetTotalWeaponDamage();
            currentStats.attackDamage = weaponDamage;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            TakeDamage(enemyDamagePerHit);
        }
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
                //currentHP += augment.statValue; // Curar o valor adicional
                break;
            case StatType.AttackDamage:
                currentStats.attackDamage += (int)augment.statValue;
                break;
            case StatType.AlcoholAmplification:
                // Alcohol amplification � o multiplicador de dano
                currentStats.alcoholAmplification += augment.statValue;
                break;
            case StatType.Armor:
                currentStats.armor += (int)augment.statValue;
                break;
            case StatType.CriticalChance:
                currentStats.critChance += augment.statValue;
                break;
            case StatType.CriticalDamage:
                currentStats.critDamage += augment.statValue;
                break;
        }
    }

    /*
    private float CalculateDamageReduction(float armor)
    {
        return playerBaseStats.armor / (armor + 100f);
    }*/

    private int CalculateDamageAfterArmor(int incomingDamage, float armor)
    {
        //float damageReduction = CalculateDamageReduction(armor);
        float damage = incomingDamage - armor;

        return Mathf.RoundToInt(damage);
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        int finalDamage = CalculateDamageAfterArmor(damage, playerBaseStats.armor);


        currentStats.health -= finalDamage;

        PlayHitSound();

        if (currentHP <= 0)
        {
            OnPlayerDeath();
        }

        StartCoroutine(InvincibilityCoroutine());
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
        playerBaseStats.armor = Mathf.Max(0, newArmor);
    }

    public void ResetArmor()
    {
        playerBaseStats.armor = 0;
    }

    public void OnPlayerDeath()
    {
        Debug.Log("Player died");
    }

    public int CalculatePlayerDamage(int baseDamage)
    {
        float damage = baseDamage;

        // Incluir dano das armas
        if (weaponManager != null)
        {
            damage += weaponManager.GetTotalWeaponDamage();
        }

        // Aplicar alcohol amplification
        damage *= currentStats.alcoholAmplification;

        // Verificar crítico
        bool isCrit = Random.value <= currentStats.critChance;

        if (isCrit)
        {
            damage *= currentStats.critDamage;
        }

        return Mathf.RoundToInt(damage);
    }

    void PlayHitSound()
    {
        if (hitAudioSource == null || hitSounds == null || hitSounds.Length == 0)
            return;

        AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];

        hitAudioSource.PlayOneShot(clip);
    }

    private System.Collections.IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

}
