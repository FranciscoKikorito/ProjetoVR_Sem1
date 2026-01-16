using UnityEngine;

public class SimpleWeapon : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0, 30 * Time.deltaTime, 0);
        transform.position += Vector3.up * Mathf.Sin(Time.time) * 0.001f;
    }
}
