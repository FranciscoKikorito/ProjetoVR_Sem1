using UnityEngine;

public class HandPunch : MonoBehaviour
{
    public int damage = 50;
    public LayerMask enemyLayer;

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & enemyLayer) == 0)
            return;

        Debug.Log("Apply damage via collision");

        Health health = collision.gameObject.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }
}
