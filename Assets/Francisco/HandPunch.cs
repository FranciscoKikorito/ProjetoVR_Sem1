using UnityEngine;

public class HandPunch : MonoBehaviour
{
    public int baseDamage = 50;
    public LayerMask enemyLayer;

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

    }
}
