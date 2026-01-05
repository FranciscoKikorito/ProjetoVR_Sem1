using System.Collections.Generic;
using UnityEngine;

public class AugmentManager : MonoBehaviour
{
    [System.Serializable]
    public class AugmentPool
    {
        public List<Augment> commonAugments;
        public List<Augment> uncommonAugments;
        public List<Augment> rareAugments;
        public List<Augment> epicAugments;
        public List<Augment> legendaryAugments;
    }

    [Header("References")]
    public Spawner waveSpawner;

    [Header("Augment Pools")]
    public AugmentPool statAugments;
    public AugmentPool weaponAugments;

    [Header("Selection Settings")]
    public int choicesPerWave = 3;
    [Range(0f, 1f)]
    public float weaponChance = 0.5f;

    private List<Augment> generatedChoices = new List<Augment>();

    public void ShowAugmentSelection()
    {
        Debug.Log("Mostrando seleção de augments");

        Time.timeScale = 0f;

        GenerateAugmentChoices();
    }

    private void GenerateAugmentChoices()
    {
        generatedChoices.Clear();

        int weaponCount = 0;
        int statCount = 0;

        for (int i = 0; i < choicesPerWave; i++)
        {
            Augment newAugment;

            bool chooseWeapon = Random.value < weaponChance;

            if (chooseWeapon && weaponCount < choicesPerWave - 1)
            {
                newAugment = GetRandomAugmentFromPool(weaponAugments);
                weaponCount++;
            }
            else
            {
                newAugment = GetRandomAugmentFromPool(statAugments);
                statCount++;
            }

            if (Player.instance != null && !newAugment.canStack)
            {
                bool alreadyHas = false;
                foreach (Augment active in Player.instance.activeAugments)
                {
                    if (active.augmentName == newAugment.augmentName)
                    {
                        alreadyHas = true;
                        break;
                    }
                }

                if (alreadyHas)
                {
                    i--;
                    continue;
                }
            }

            generatedChoices.Add(newAugment);
        }
    }

    private Augment GetRandomAugmentFromPool(AugmentPool pool)
    {
        // Sistema de raridade simples
        float rarityRoll = Random.value;
        List<Augment> selectedPool;

        if (rarityRoll < 0.01f) // 1% Legendary
            selectedPool = pool.legendaryAugments;
        else if (rarityRoll < 0.05f) // 4% Epic
            selectedPool = pool.epicAugments;
        else if (rarityRoll < 0.15f) // 10% Rare
            selectedPool = pool.rareAugments;
        else if (rarityRoll < 0.40f) // 25% Uncommon
            selectedPool = pool.uncommonAugments;
        else // 60% Common
            selectedPool = pool.commonAugments;

        if (selectedPool == null || selectedPool.Count == 0)
        {
            // Fallback para common
            selectedPool = pool.commonAugments;
        }

        if (selectedPool.Count == 0)
        {
            Debug.LogError($"Augment pool vazio para raridade: {rarityRoll}");
            return null;
        }

        return selectedPool[Random.Range(0, selectedPool.Count)];
    }

    public void SelectAugment(Augment selectedAugment)
    {
        Debug.Log($"Augment selecionado: {selectedAugment.augmentName}");

        if (Player.instance != null)
        {
            Player.instance.ApplyAugment(selectedAugment);
        }

        Time.timeScale = 1f; //unpause

        if (waveSpawner != null)
        {
            
        }
    }
}
