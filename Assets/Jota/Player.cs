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

    private void ApplyWeaponAugment(Augment augment)
    {
        if (augment.weaponPrefab != null)
        {
            // Encontrar slot apropriado para a arma
            Transform weaponSlot = null;

            // Voc� pode adaptar isso para seu sistema de armas
            if (weapons != null && weapons.Length > 0)
            {
                // Aqui voc� pode implementar l�gica para equipar a arma
                // Por exemplo, substituir uma arma existente
            }

            // Instantiate a nova arma
            GameObject newWeapon = Instantiate(augment.weaponPrefab, transform.position, Quaternion.identity);

            // Configurar a arma (voc� pode precisar adaptar)
            IWeapons weaponComponent = newWeapon.GetComponent<IWeapons>();
            if (weaponComponent != null)
            {
                weaponComponent.ApplyBonusStat();
            }
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

        // Aplicar alcohol amplification
        damage *= currentStats.alcoholAmplification;

        // Verificar cr�tico
        bool isCrit = Random.value <= (currentStats.critChance * 100f);

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
