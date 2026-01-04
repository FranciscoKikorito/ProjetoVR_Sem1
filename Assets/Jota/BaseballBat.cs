using UnityEngine;

public class BaseballBat : MonoBehaviour, IWeapons
{

    public WeaponStats weaponStats;
    public HandPunch player;

    public void ApplyBonusStat()
    {
        player.baseDamage += weaponStats.bonusAttackDamage;
        player.baseDamage *= weaponStats.bonusAttackDamage;
    }
}
