using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Damage config")]
    public int baseDamage = 30;
    public float damageMultiplier = 1f;
    public LayerMask enemyLayer;
    public float hitCooldown = 0.5f;

    [Header("Audio/VFX config")]
    public AudioClip[] hitSounds;
    public AudioSource audioSource;
    public GameObject[] hitEffects;
    public float effectDestroyTime = 2f;

    [Header("Physics config")]
    public float velocityThreshold = 1.5f;
    public Collider weaponCollider;

    // Dano atual (considerando multipliers)
    [HideInInspector] public int damage;

    private Rigidbody rb;
    private Vector3 previousPosition;
    private float currentVelocity;
    private float lastHitTime;

    void Start()
    {
        InitializeComponents();
        damage = Mathf.RoundToInt(baseDamage * damageMultiplier);
    }

    void InitializeComponents()
    {
        // Adicionar/obter Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; // Não afetado por física externa
            rb.useGravity = false;
        }

        // Adicionar collider se não existir
        if (weaponCollider == null)
        {
            weaponCollider = GetComponent<Collider>();
            if (weaponCollider == null)
            {
                // Adicionar box collider como padrão
                weaponCollider = gameObject.AddComponent<BoxCollider>();
            }
        }

        // Configurar collider como trigger
        weaponCollider.isTrigger = false; // Usamos collision, não trigger

        // Configurar AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1f; // Som 3D
                audioSource.maxDistance = 10f;
            }
        }

        previousPosition = transform.position;
    }

    void Update()
    {
        // Calcular velocidade da arma
        currentVelocity = (transform.position - previousPosition).magnitude / Time.deltaTime;
        previousPosition = transform.position;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Verificar cooldown
        if (Time.time - lastHitTime < hitCooldown) return;

        // Verificar velocidade mínima
        if (currentVelocity < velocityThreshold) return;

        // Verificar se é inimigo
        if (((1 << collision.gameObject.layer) & enemyLayer) == 0) return;

        // Aplicar dano
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            int finalDamage = CalculateFinalDamage();
            damageable.TakeDamage(finalDamage);

            // Efeitos
            PlayHitEffects(collision);

            lastHitTime = Time.time;
        }
    }

    private int CalculateFinalDamage()
    {
        // Usar stats do jogador para calcular dano final
        float finalDamage = damage;

        if (Player.instance != null)
        {
            // Multiplicador de álcool
            finalDamage *= Player.instance.currentStats.alcoholAmplification;

            // Chance crítica
            bool isCrit = Random.value <= Player.instance.currentStats.critChance;
            if (isCrit)
            {
                finalDamage *= Player.instance.currentStats.critDamage;
            }
        }

        return Mathf.RoundToInt(finalDamage);
    }

    private void PlayHitEffects(Collision collision)
    {
        // Som
        if (hitSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
            audioSource.PlayOneShot(clip);
        }

        // Partículas/VFX
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

    public void Initialize(LayerMask enemyLayerMask)
    {
        enemyLayer = enemyLayerMask;
        InitializeComponents();
    }

    public void UpdateDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
        damage = Mathf.RoundToInt(baseDamage * damageMultiplier);
    }
}
