using UnityEngine;

public class TextReference : MonoBehaviour
{
    public GameObject assignedText;

    void OnDestroy()
    {
        // Destruir texto associado quando este objeto for destruído
        if (assignedText != null)
        {
            Destroy(assignedText);
        }
    }
}
