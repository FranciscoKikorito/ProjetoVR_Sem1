using UnityEngine;

[CreateAssetMenu(fileName = "Stats", menuName = "Scriptable Objects/Stats")]
public class Stats : ScriptableObject
{
    public float health;
    public int attackDamage;
    public int critChance;
    public float crintDamage = 0.75f;
    public int armor;
    public float alcoholAmplification;
}
