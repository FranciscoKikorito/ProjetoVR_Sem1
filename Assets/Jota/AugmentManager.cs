using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

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
    public Transform playerHead; // Arraste a câmera principal aqui

    [Header("Drink Settings")]
    public int drinksPerWave = 3;
    public float drinkDistance = 0.4f;
    public Transform[] drinkSpawnPoints; // Pontos no balcão onde bebidas spawnam

    [Header("Drink prefabs")]
    public GameObject[] drinkPrefabs; // Vários modelos de copo/bebida

    [Header("HUD Display")]
    public TextMeshProUGUI augmentDisplayText; // Texto na tela do jogador
    public float displayDuration = 3f; // Tempo que o texto fica visível

    [Header("Augment Pools")]
    public AugmentPool statAugments;
    public AugmentPool weaponAugments;

    [Header("Selection Settings")]
    public int choicesPerWave = 3;
    [Range(0f, 1f)]
    public float weaponChance = 0.5f;

    [Header("Audio Settings")]
    public AudioSource audioSource; // Audio source to play sounds
    public AudioClip drinkSound;   // Sound to play when a drink is consumed

    private List<Augment> generatedChoices = new List<Augment>();
    private List<GameObject> currentDrinks = new List<GameObject>();
    private bool isSelectionActive = false;

    private void Start()
    {
        if (playerHead == null)
            playerHead = Camera.main?.transform;

        if (augmentDisplayText != null)
            augmentDisplayText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isSelectionActive && playerHead != null)
        {
            CheckForDrinkConsumption();
        }
    }

    public void ShowAugmentSelection()
    {
        Debug.Log("Mostrando seleção de augments");

        isSelectionActive = true;

        GenerateAugmentChoices();
        SpawnDrinks();
        ShowInstructionText();
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

    private void ShowInstructionText()
    {
        if (augmentDisplayText != null)
        {
            augmentDisplayText.text = "<color=yellow>Chose your drink!!</color>\nGet your head closer to drink";
            augmentDisplayText.gameObject.SetActive(true);
            StartCoroutine(HideTextAfterDelay(3f));
        }
    }

    private IEnumerator HideTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (augmentDisplayText != null && isSelectionActive)
        {
            augmentDisplayText.gameObject.SetActive(false);
        }
    }

    private Augment GetRandomAugmentFromPool(AugmentPool pool)
    {
        float rarityRoll = Random.value;
        List<Augment> selectedPool;

        if (rarityRoll < 0.03f)
            selectedPool = pool.legendaryAugments;
        else if (rarityRoll < 0.10f)
            selectedPool = pool.epicAugments;
        else if (rarityRoll < 0.20f)
            selectedPool = pool.rareAugments;
        else if (rarityRoll < 0.60f)
            selectedPool = pool.uncommonAugments;
        else
            selectedPool = pool.commonAugments;

        if (selectedPool == null || selectedPool.Count == 0)
        {
            selectedPool = pool.commonAugments;
        }

        if (selectedPool.Count == 0)
        {
            Debug.LogError("Pool de augment vazia!");
            return null;
        }

        return selectedPool[Random.Range(0, selectedPool.Count)];
    }

    // =============================
    // UPDATED SPAWN LOGIC ONLY
    // =============================

    private void SpawnDrinks()
    {
        ClearCurrentDrinks();

        if (generatedChoices.Count < drinksPerWave)
        {
            Debug.LogWarning("Não há augments suficientes!");
            return;
        }

        // Shuffle spawn points each wave
        List<Transform> shuffledPoints = new List<Transform>(drinkSpawnPoints);
        for (int i = 0; i < shuffledPoints.Count; i++)
        {
            Transform temp = shuffledPoints[i];
            int rnd = Random.Range(i, shuffledPoints.Count);
            shuffledPoints[i] = shuffledPoints[rnd];
            shuffledPoints[rnd] = temp;
        }

        // Copy prefabs list so we can remove used ones
        List<GameObject> availablePrefabs = new List<GameObject>(drinkPrefabs);

        int spawnCount = Mathf.Min(drinksPerWave, shuffledPoints.Count, generatedChoices.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            if (availablePrefabs.Count == 0) break;

            int prefabIndex = Random.Range(0, availablePrefabs.Count);
            GameObject chosenPrefab = availablePrefabs[prefabIndex];
            availablePrefabs.RemoveAt(prefabIndex); // Prevent reuse this wave

            SpawnSingleDrink(shuffledPoints[i], generatedChoices[i], chosenPrefab);
        }
    }

    private void SpawnSingleDrink(Transform spawnPoint, Augment augment, GameObject drinkPrefab)
    {
        if (drinkPrefab == null || augment == null) return;

        GameObject drink = Instantiate(drinkPrefab, spawnPoint.position, spawnPoint.rotation);
        currentDrinks.Add(drink);

        SimpleDrink drinkScript = drink.GetComponent<SimpleDrink>();
        if (drinkScript == null)
            drinkScript = drink.AddComponent<SimpleDrink>();

        drinkScript.augment = augment;

        CreateDrinkText(drink, augment);
    }

    // =============================

    private Color GetColorByRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return Color.white;
            case Rarity.Uncommon: return Color.green;
            case Rarity.Rare: return Color.blue;
            case Rarity.Epic: return Color.magenta;
            case Rarity.Legendary: return Color.yellow;
            default: return Color.white;
        }
    }

    private void CheckForDrinkConsumption()
    {
        for (int i = currentDrinks.Count - 1; i >= 0; i--)
        {
            GameObject drink = currentDrinks[i];
            if (drink == null) continue;

            float distance = Vector3.Distance(drink.transform.position, playerHead.position);

            if (distance < drinkDistance)
            {
                DrinkSelected(drink);
                break;
            }
        }
    }

    private void DrinkSelected(GameObject drink)
    {
        SimpleDrink drinkScript = drink.GetComponent<SimpleDrink>();
        if (drinkScript == null || drinkScript.augment == null) return;

        Augment selectedAugment = drinkScript.augment;
        Debug.Log("Augment selecionado: " + selectedAugment.augmentName);

        if (audioSource != null && drinkSound != null)
        {
            audioSource.PlayOneShot(drinkSound);
        }

        ShowAugmentOnScreen(selectedAugment);

        if (Player.instance != null)
        {
            Player.instance.ApplyAugment(selectedAugment);
        }

        ClearCurrentDrinks();
        isSelectionActive = false;

        if (waveSpawner != null)
        {
            waveSpawner.ContinueToNextWave();
        }
    }

    private void ShowAugmentOnScreen(Augment augment)
    {
        if (augmentDisplayText == null) return;

        string rarityColor = GetRarityColorCode(augment.rarity);
        string augmentType = augment.augmentType == AugmentType.Weapon ? "ARMA" : "STAT";

        augmentDisplayText.text =
            $"<size=150%><color={rarityColor}>{augment.augmentName}</color></size>\n" +
            $"<color=white>{augment.description}</color>\n\n" +
            $"<color=#FFA500>Tipo: {augmentType} | Raridade: {augment.rarity}</color>";

        augmentDisplayText.gameObject.SetActive(true);
        StartCoroutine(HideDisplayAfterDelay(displayDuration));
    }

    private string GetRarityColorCode(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return "white";
            case Rarity.Uncommon: return "green";
            case Rarity.Rare: return "blue";
            case Rarity.Epic: return "magenta";
            case Rarity.Legendary: return "yellow";
            default: return "white";
        }
    }

    private IEnumerator HideDisplayAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (augmentDisplayText != null)
        {
            augmentDisplayText.gameObject.SetActive(false);
        }
    }

    private void ClearCurrentDrinks()
    {
        foreach (GameObject drink in currentDrinks)
        {
            if (drink != null)
                Destroy(drink);
        }
        currentDrinks.Clear();
    }

    private void CreateDrinkText(GameObject drink, Augment augment)
    {
        GameObject textObj = new GameObject("DrinkText");
        textObj.transform.position = drink.transform.position + Vector3.up * 0.4f;
        textObj.transform.SetParent(drink.transform);

        TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
        textMesh.text = augment.augmentName + "\n" + augment.description;
        textMesh.fontSize = 2f;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = GetColorByRarity(augment.rarity);
        textMesh.enableWordWrapping = true;
        textMesh.enableAutoSizing = false;
        textMesh.rectTransform.localScale = Vector3.one;
        textMesh.rectTransform.sizeDelta = new Vector2(3, 1);

        BillboardText billboard = textObj.AddComponent<BillboardText>();
        billboard.target = playerHead;
    }

    public void SelectAugment(Augment selectedAugment)
    {
        Debug.Log($"Augment selecionado manualmente: {selectedAugment.augmentName}");

        if (Player.instance != null)
        {
            Player.instance.ApplyAugment(selectedAugment);
        }

        if (isSelectionActive)
        {
            ClearCurrentDrinks();
            isSelectionActive = false;
        }

        if (waveSpawner != null)
        {
            waveSpawner.ContinueToNextWave();
        }
    }
}

// Mantém o texto sempre virado para o jogador
public class BillboardText : MonoBehaviour
{
    public Transform target;

    void Update()
    {
        if (target == null) return;

        Vector3 lookDirection = transform.position - target.position;
        lookDirection.y = 0;
        if (lookDirection.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDirection);
    }
}
