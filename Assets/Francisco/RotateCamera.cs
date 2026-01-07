using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public Camera targetCamera;

    void Start()
    {
        if (targetCamera == null)
        {
            Debug.LogWarning("No camera assigned.");
            return;
        }

        targetCamera.transform.Rotate(0f, 180f, 0f);
    }
}
