using UnityEngine;

public class TextReference : MonoBehaviour
{
    public GameObject assignedText;

    void OnDestroy()
    {
        if (assignedText != null)
        {
            Destroy(assignedText);
        }
    }
}
