using UnityEngine;

public class StatsDisplay : MonoBehaviour
{
    [Header("Player & Text Component")]
    public Player player;
    public TextMesh textComponent;

    void Update()
    {
        if (player == null || Player.instance == null || Player.instance.currentStats == null || textComponent == null)
            return;

        var stats = Player.instance.currentStats;
        int critChance = (int)(stats.critChance * 100);

        textComponent.text =
            $"<color=green>Health: {stats.health:0}</color>\n" +
            $"<color=orange>Attack: {stats.attackDamage:0}</color>\n" +
            $"<color=purple>Alcohol Amplification: {stats.alcoholAmplification:0.00}</color>\n" +
            $"<color=red>Crit Chance: {critChance:0}%</color>\n" +
            $"<color=yellow>Crit Damage: {stats.critDamage:0.00}x</color>\n" +
            $"<color=blue>Armor: {stats.armor:0}</color>\n";
    }
}
