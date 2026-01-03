using UnityEngine;

public class HandPunch : MonoBehaviour
{
    public int damage = 50;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Apply damage via collision");

        // Only hit objects with Health component
        Health health = collision.gameObject.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }
}
