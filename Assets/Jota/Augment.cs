using UnityEngine;

[CreateAssetMenu(fileName = "Augment", menuName = "Scriptable Objects/Augment")]
public class Augment : ScriptableObject
{
    [Header("Basic Info")]
    public string augmentName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    public AugmentType augmentType;
    public Rarity rarity = Rarity.Common;

    [Header("Weapon Augment")]
    public GameObject weaponPrefab;
    public string weaponSlot = "RightHand";

    [Header("Stat Augment")]
    public StatType statType;
    public float statValue;

    [Header("Visual/Sound Effects")]
    public GameObject applyEffect;
    public AudioClip applySound;

    [Header("Stacking")]
    public bool canStack = false;
    public int maxStacks = 1;
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
