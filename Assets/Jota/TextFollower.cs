using UnityEngine;

public class TextFollower : MonoBehaviour
{
    public Transform target;
    public float verticalOffset = 0.4f;
    public float followSpeed = 10f;

    private Vector3 targetPosition;

    void Update()
    {
        if (target != null && target.gameObject.activeSelf)
        {
            targetPosition = target.position + Vector3.up * verticalOffset;

            transform.position = Vector3.Lerp(transform.position, targetPosition,
                followSpeed * Time.deltaTime);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
