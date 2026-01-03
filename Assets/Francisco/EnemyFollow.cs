using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 2f;
    public float rotationSpeed = 10f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (player == null)
        {
            GameObject rig = GameObject.Find("[BuildingBlock] Camera Rig");
            if (rig != null)
                player = rig.transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // ----- MOVEMENT -----
        Vector3 direction = player.position - transform.position;
        direction.y = 0f; // ignore vertical difference
        float distanceSqr = direction.sqrMagnitude;

        if (distanceSqr > 0.01f)
        {
            direction.Normalize();

            Vector3 targetVelocity = direction * moveSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(
                Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime)
            );
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }
}
