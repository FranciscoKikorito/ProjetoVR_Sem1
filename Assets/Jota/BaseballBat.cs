using UnityEngine;

public class BaseballBat : MonoBehaviour, IWeapons
{

    public WeaponStats weaponStats;
    public Player player;

    public void ApplyBonusStat()
    {
        player.currentStats.attackDamage += weaponStats.bonusAttackDamage;
        player.currentStats.attackDamage *= weaponStats.bonusAttackDamage;
    }
}
