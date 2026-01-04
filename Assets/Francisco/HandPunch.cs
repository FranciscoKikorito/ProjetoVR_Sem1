using UnityEngine;

public class HandPunch : MonoBehaviour
{
    public int damage = 50;
    public LayerMask enemyLayer;

    int calculateDamage()
    {
        return damage;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & enemyLayer) == 0)
            return;

        Debug.Log("Apply damage via collision");

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

        damageable?.TakeDamage(damage);

    }
}
