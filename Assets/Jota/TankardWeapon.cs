using UnityEngine;

public class TankardWeapon : MonoBehaviour
{
    private float originalAlcoholAmplification;

    void Start()
    {
        if (Player.instance != null)
        {
            originalAlcoholAmplification = Player.instance.currentStats.alcoholAmplification;
            float newValue = (originalAlcoholAmplification / 3f) * 4f;
            Player.instance.currentStats.alcoholAmplification = newValue;
            Debug.Log($"Tankard: Alcohol Amplification aumentado");
        }
    }

    void OnDestroy()
    {
        if (Player.instance != null)
        {
            Player.instance.currentStats.alcoholAmplification = originalAlcoholAmplification;
        }
    }
}
