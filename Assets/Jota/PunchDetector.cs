using System.Collections;
using UnityEngine;

public class PunchDetector : MonoBehaviour
{
    [Header("Punch Settings")]
    public float minPunchVelocity = 2.5f;
    public float punchDamage = 10f;
    public float punchCooldown = 0.5f;
    public LayerMask enemyLayer;

    [Header("References")]
    public Transform handTransform;
    public OVRHand hand;

    private Vector3 previousPosition;
    private float currentVelocity;
    private float lastPunchTime;
    private bool isPunching = false;

    void Start()
    {
        previousPosition = transform.position;

        if (hand != null) 
        { 
            hand = GetComponent<OVRHand>();
        }
    }

    void Update()
    {
        CalculateHandVelocity();

        DetectPunch();
    }

    void CalculateHandVelocity()
    {
        Vector3 currentPosition = handTransform.position;

        float distance = Vector3.Distance(currentPosition, previousPosition);
        currentVelocity = distance / Time.deltaTime;

        previousPosition = currentPosition;
    }

    void DetectPunch()
    {
        if (currentVelocity >= minPunchVelocity &&
            Time.time > lastPunchTime + punchCooldown &&
            !isPunching)
        {
            // Verifica se a mão está fechada (soco)
            if (IsFistClosed())
            {
                StartCoroutine(PerformPunch());
            }
        }
    }

    bool IsFistClosed()
    {
        if (hand == null) return true;

        float confidence = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);

        return confidence > 0.7f;
    }

    IEnumerator PerformPunch()
    {
        isPunching = true;
        lastPunchTime = Time.time;

        Debug.Log("Punch Detected! Velocity: " + currentVelocity);

        DetectCollisions();

        yield return new WaitForSeconds(0.1f);

        isPunching = false;
    }

    void DetectCollisions()
    {
        Collider[] hitColliders = Physics.OverlapSphere(
            handTransform.position,
            0.1f,
            enemyLayer
        );

        foreach (Collider collider in hitColliders)
        {
            Enemy enemyHealth = collider.GetComponent<Enemy>();
            if (enemyHealth != null)
            {
                float damageMultiplier = Mathf.Clamp(currentVelocity / minPunchVelocity, 1f, 3f);
                float totalDamage = punchDamage * damageMultiplier;

                enemyHealth.TakeDamage(totalDamage);

                Debug.Log("Enemy Hit! Damage: " + totalDamage);
            }
        }
    }
}
