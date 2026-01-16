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

    [Header("Death Settings")]
    public GameObject restartObject;
    public GameObject stop1;
    public GameObject stop2;
    public GameObject stop3;
    public GameObject destroyThis;

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
                break;
            case StatType.AttackDamage:
                currentStats.attackDamage += (int)augment.statValue;
                break;
            case StatType.AlcoholAmplification:
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
            GameObject newWeapon = Instantiate(augment.weaponPrefab, transform.position, Quaternion.identity);
        }
    }


    private int CalculateDamageAfterArmor(int incomingDamage, float armor)
    {
        float reduction = Mathf.Clamp01(armor / (armor + 100f));
        float damageAfterReduction = incomingDamage * (1f - reduction);
        return Mathf.RoundToInt(damageAfterReduction);
    }



    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        int finalDamage = CalculateDamageAfterArmor(damage, currentStats.armor); // use currentStats, not base
        currentStats.health -= finalDamage;


        currentStats.health -= finalDamage;
        PlayHitSound();

        if (currentStats.health <= 0)
        {
            currentStats.health = 0;
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

        if (restartObject != null)
            restartObject.SetActive(true);

        if (stop1 != null)
            stop1.SetActive(false);

        if (stop2 != null)
            stop2.SetActive(false);

        if (stop3 != null)
            stop3.SetActive(false);

        Destroy(destroyThis);

        this.enabled = false;
    }

    public int CalculatePlayerDamage(int baseDamage)
    {
        float damage = baseDamage;

        damage *= currentStats.alcoholAmplification;
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
