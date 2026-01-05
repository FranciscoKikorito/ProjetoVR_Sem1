using UnityEngine;

public class HandPunch : MonoBehaviour
{
    public int baseDamage = 50;
    public LayerMask enemyLayer;

    public AudioClip[] hitSounds;

    [Range(0, 1)] public float volume = 1.0f;

    [Header("Audio")]
    public AudioSource hitAudioSource; // Assign in Inspector

    int CalculateDamage()
    {
        if (Player.instance != null)
        {
            return Player.instance.CalculatePlayerDamage(baseDamage);
        }

        bool isCrit = Random.value <= 0.1f;
        float dmg = baseDamage;

        if (isCrit)
        {
            dmg *= 1.75f;
        }

        return (int)dmg;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & enemyLayer) == 0)
            return;

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable == null)
            return;

        int damageToApply = CalculateDamage();
        damageable.TakeDamage(damageToApply);

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
}
