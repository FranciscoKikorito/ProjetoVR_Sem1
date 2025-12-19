using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public Transform player; // Drag your Player GameObject here in the Inspector
    public float moveSpeed = 0.1f; // Speed of the enemy

    void Start()
    {
        // If player isn't set in Inspector, try finding it by tag
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        // 1. Calculate Direction
        Vector3 direction = player.position - transform.position;
        // Optional: Normalize if you want consistent speed regardless of distance
        // direction.Normalize();

        // 2. Move Towards Player (Option 1: Using Vector3.MoveTowards)
        // This is great for fixed-speed movement towards a point.
        transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

        // OR (Option 2: Using transform.Translate for more custom control)
        // transform.Translate(direction.normalized * moveSpeed * Time.deltaTime, Space.World);
        // Use .normalized if you didn't normalize earlier for consistent speed.
        // Space.World ensures movement in world directions, not local (like the enemy's forward).
    }
}
