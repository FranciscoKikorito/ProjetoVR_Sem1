using UnityEngine;

public class BananaWeapon : MonoBehaviour
{
    public bool isSuperBanana = false;
    public float critDamageBoost = 1f;

    private float originalCritDamage;

    void Start()
    {
        if (Player.instance != null)
        {
            originalCritDamage = Player.instance.currentStats.critDamage;
            Player.instance.currentStats.critDamage += critDamageBoost;
            Debug.Log($"Banana: Crit Damage +{critDamageBoost * 100}%");
        }
    }

    void OnDestroy()
    {
        if (Player.instance != null)
        {
            Player.instance.currentStats.critDamage = originalCritDamage;
        }
    }
}
