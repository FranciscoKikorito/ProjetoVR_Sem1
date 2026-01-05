using UnityEngine;

public class HandPunch : MonoBehaviour
{
    public int baseDamage = 50;
    public LayerMask enemyLayer;
    public AudioClip[] hitSounds; // This creates a list in the Inspector
    [Range(0, 1)] public float volume = 1.0f;

    int CalculateDamage()
    {
        if (Player.instance != null)
        {
            return Player.instance.CalculatePlayerDamage(baseDamage);
        }

        // Fallback se Player.instance não estiver disponível
        bool isCrit = Random.value <= 0.1f; // 10% crítico padrão
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

        Debug.Log("Apply damage via collision");

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

        int damageToApply = CalculateDamage();

        damageable?.TakeDamage(damageToApply);

        if (hitSounds != null && hitSounds.Length > 0)
        {
            // Pick a random index from 0 to the end of the list
            int randomIndex = Random.Range(0, hitSounds.Length);
            AudioClip clipToPlay = hitSounds[randomIndex];

            // Play the sound at the point of impact
            AudioSource.PlayClipAtPoint(clipToPlay, collision.contacts[0].point, volume);
        }

       
    }

}
