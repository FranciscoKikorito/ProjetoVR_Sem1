using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public Augment augment;
    public AugmentManager augmentManager;

    void OnCollisionEnter(Collision collision)
    {
        TryPickup(collision.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        TryPickup(other.gameObject);
    }

    private void TryPickup(GameObject other)
    {
        // Verificar se foi a mão do jogador (procura HandPunch nas mãos)
        HandPunch handPunch = other.GetComponent<HandPunch>();
        if (handPunch != null || other.CompareTag("PlayerHand"))
        {
            if (augmentManager != null && augment != null)
            {
                augmentManager.OnWeaponPickedUp(gameObject, augment);
                Destroy(this); // Remove o script de pickup, a arma já foi pega
            }
        }
    }
}
