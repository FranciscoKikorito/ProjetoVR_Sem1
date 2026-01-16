using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public Augment augment;
    public AugmentManager augmentManager;
    public float pickupDelay = 0.5f;

    private float spawnTime;
    private bool canBePickedUp = false;

    void Start()
    {
        spawnTime = Time.time;
        StartCoroutine(EnablePickupAfterDelay());
    }

    private System.Collections.IEnumerator EnablePickupAfterDelay()
    {
        yield return new WaitForSeconds(pickupDelay);
        canBePickedUp = true;
        Debug.Log("Arma agora pode ser pega: " + augment.augmentName);
    }

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
        if (!canBePickedUp) return;

        HandPunch handPunch = other.GetComponent<HandPunch>();
        if (handPunch != null || other.CompareTag("PlayerHand"))
        {
            if (augmentManager != null && augment != null)
            {
                augmentManager.OnWeaponPickedUp(gameObject, augment);
                Destroy(this);
            }
        }
    }
}
