using UnityEngine;

public abstract class WeaponBase : Weapon
{
    [Header("Configurações Base")]
    public string weaponName;
    public int baseDamage;
    public float attackSpeed = 1f;
    public LayerMask enemyLayer;

    [Header("Efeitos Visuais/Sonoros")]
    public AudioClip[] hitSounds;
    public AudioSource audioSource;
    public GameObject[] hitEffects;
    public float effectDestroyTime = 2f;

    [Header("Física")]
    public float velocityThreshold = 1.5f;
    public float hitCooldown = 0.3f;

    // Componentes
    protected Collider weaponCollider;
    protected Rigidbody rb;

    // Estado
    protected Vector3 previousPosition;
    protected float currentVelocity;
    protected float lastHitTime;

    // Referência ao jogador
    protected Player player;

    // Damage atual (pode ser modificado por efeitos)
    public int CurrentDamage { get; protected set; }

    public void Start()
    {
        InitializeWeapon();
        CurrentDamage = baseDamage;
    }

    void Update()
    {
        UpdateVelocity();
        UpdateWeapon();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (CanHit())
        {
            if (IsEnemy(collision.gameObject))
            {
                HandleHit(collision);
            }
        }
    }

    protected virtual void InitializeWeapon()
    {
        player = Player.instance;

        // Configurar Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Configurar Collider
        weaponCollider = GetComponent<Collider>();
        if (weaponCollider == null)
        {
            weaponCollider = gameObject.AddComponent<BoxCollider>();
        }
        weaponCollider.isTrigger = false;

        // Configurar AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1f;
            }
        }

        previousPosition = transform.position;
    }

    protected virtual void UpdateVelocity()
    {
        currentVelocity = (transform.position - previousPosition).magnitude / Time.deltaTime;
        previousPosition = transform.position;
    }

    protected virtual void UpdateWeapon()
    {
        // Sobrescrever em classes filhas para lógica específica
    }

    protected virtual bool CanHit()
    {
        return currentVelocity >= velocityThreshold &&
               Time.time - lastHitTime >= hitCooldown;
    }

    protected virtual bool IsEnemy(GameObject obj)
    {
        return ((1 << obj.layer) & enemyLayer) != 0;
    }

    protected virtual void HandleHit(Collision collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            int damage = CalculateDamage();
            damageable.TakeDamage(damage);

            PlayHitEffects(collision);
            OnHitEffect(collision.gameObject);

            lastHitTime = Time.time;
        }
    }

    protected virtual int CalculateDamage()
    {
        float damage = CurrentDamage;

        if (player != null)
        {
            // Aplicar alcohol amplification
            damage *= player.currentStats.alcoholAmplification;

            // Aplicar chance crítica
            bool isCrit = Random.value <= player.currentStats.critChance;
            if (isCrit)
            {
                damage *= player.currentStats.critDamage;
                OnCriticalHit();
            }
        }

        return Mathf.RoundToInt(damage);
    }

    protected virtual void PlayHitEffects(Collision collision)
    {
        // Som
        if (hitSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
            audioSource.PlayOneShot(clip);
        }

        // Efeitos visuais
        if (hitEffects.Length > 0 && collision.contacts.Length > 0)
        {
            ContactPoint contact = collision.contacts[0];
            GameObject effect = hitEffects[Random.Range(0, hitEffects.Length)];

            if (effect != null)
            {
                GameObject vfx = Instantiate(effect, contact.point,
                    Quaternion.LookRotation(contact.normal));
                Destroy(vfx, effectDestroyTime);
            }
        }
    }

    protected virtual void OnHitEffect(GameObject enemy)
    {
        // Sobrescrever em classes filhas para efeitos especiais
    }

    protected virtual void OnCriticalHit()
    {
        // Sobrescrever em classes filhas para efeitos de crítico
    }

    public virtual void ApplyWeaponEffect(Player targetPlayer)
    {
        // Aplicar efeito especial da arma ao jogador
        // Sobrescrever em cada arma específica
    }

    public virtual void RemoveWeaponEffect(Player targetPlayer)
    {
        // Remover efeito especial da arma do jogador
        // Sobrescrever em cada arma específica
    }

    public void Initialize(LayerMask enemyLayerMask)
    {
        enemyLayer = enemyLayerMask;
    }
}
