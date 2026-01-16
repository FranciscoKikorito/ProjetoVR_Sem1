using UnityEngine;


public class HandPunch : MonoBehaviour
{
    public Player player;
    public LayerMask enemyLayer;

    public float punchVelocityThreshold = 1.5f;
    private Vector3 previousPosition;
    private float currentVelocity;

    [Header("Audio")]
    public AudioClip[] hitSounds;
    public AudioSource hitAudioSource;
    [Range(0, 1)] public float volume = 1.0f;

    [Header("VFX")]
    public GameObject[] hitVFXPrefabs; // This is now an array (list)
    public float destroyVFXAfter = 2.0f;
    private void Start()
    {
        previousPosition = transform.position;
    }

    private void Update()
    {
        // Calculate hand velocity manually
        currentVelocity = (transform.position - previousPosition).magnitude / Time.deltaTime;
        previousPosition = transform.position;
    }

    int CalculateDamage()
    {
        bool isCrit = Random.value <= Player.instance.currentStats.critChance;
        float dmg = Player.instance.currentStats.attackDamage;

        if (isCrit)
        {
            dmg *= Player.instance.currentStats.critDamage;
        }

        // Optionally scale damage by velocity
        dmg *= Mathf.Clamp(currentVelocity / punchVelocityThreshold, 0.5f, 2f);

        return Mathf.RoundToInt(dmg);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only apply damage if hand is moving fast enough
        if (currentVelocity < punchVelocityThreshold)
            return;

        if (((1 << collision.gameObject.layer) & enemyLayer) == 0)
            return;

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable == null)
            return;

        int damageToApply = CalculateDamage();
        damageable.TakeDamage(damageToApply);
        SpawnHitVFX(collision);

        PlayHitSound();
    }

    void PlayHitSound()
    {
        if (hitAudioSource == null || hitSounds == null || hitSounds.Length == 0)
            return;

        AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
        hitAudioSource.volume = volume;
        hitAudioSource.PlayOneShot(clip);
    }

    void SpawnHitVFX(Collision collision)
    {
        // Check if the array is empty or null
        if (hitVFXPrefabs != null && hitVFXPrefabs.Length > 0)
        {
            // Pick a random index from 0 to the end of your list
            int randomIndex = Random.Range(0, hitVFXPrefabs.Length);
            GameObject selectedVFX = hitVFXPrefabs[randomIndex];

            if (selectedVFX != null)
            {
                ContactPoint contact = collision.contacts[0];
                GameObject vfx = Instantiate(selectedVFX, contact.point, Quaternion.LookRotation(contact.normal));
                Destroy(vfx, destroyVFXAfter);
            }
        }
    }

}
