using UnityEngine;

public class BaseballBat : MonoBehaviour, IWeapons
{

    public WeaponStats weaponStats;
    public HandPunch player;

    public void ApplyBonusStat()
    {
        player.damage += weaponStats.bonusAttackDamage;
        player.damage *= weaponStats.bonusAttackDamage;
    }
}
