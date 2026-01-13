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
            // Calcular posição alvo (acima do objeto)
            targetPosition = target.position + Vector3.up * verticalOffset;

            // Suavizar movimento
            transform.position = Vector3.Lerp(transform.position, targetPosition,
                followSpeed * Time.deltaTime);
        }
        else
        {
            // Destruir texto se o alvo foi destruído
            Destroy(gameObject);
        }
    }
}
