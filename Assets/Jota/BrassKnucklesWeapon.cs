using UnityEngine;

public class BrassKnucklesWeapon : MonoBehaviour
{
    public int armorBonus = 20;

    private int originalArmor;

    void Start()
    {
        if (Player.instance != null)
        {
            originalArmor = Player.instance.currentStats.armor;
            Player.instance.currentStats.armor += armorBonus;
            Debug.Log($"Brass Knuckles: Armor +{armorBonus}");
        }
    }

    void OnDestroy()
    {
        if (Player.instance != null)
        {
            Player.instance.currentStats.armor = originalArmor;
        }
    }
}
