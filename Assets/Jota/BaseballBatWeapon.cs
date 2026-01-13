using UnityEngine;

public class BaseballBatWeapon : MonoBehaviour
{
    public float damageMultiplier = 2f;

    void Start()
    {
        if (Player.instance != null)
        {
            Player.instance.currentStats.attackDamage =
                Mathf.RoundToInt(Player.instance.currentStats.attackDamage * damageMultiplier);
            Debug.Log($"Baseball Bat: Dano x{damageMultiplier}");
        }
    }

    void OnDestroy()
    {
        if (Player.instance != null)
        {
            Player.instance.currentStats.attackDamage =
                Mathf.RoundToInt(Player.instance.currentStats.attackDamage / damageMultiplier);
        }
    }
}
