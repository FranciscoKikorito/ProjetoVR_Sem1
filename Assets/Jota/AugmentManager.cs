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
    public Transform playerHead;

    [Header("Drink Settings")]
    public int drinksPerWave = 3;
    public float drinkDistance = 0.4f;
    public Transform[] drinkSpawnPoints;

    [Header("Drink prefabs")]
    public GameObject[] drinkPrefabs;

    [Header("HUD Display")]
    public TextMeshProUGUI augmentDisplayText;
    public float displayDuration = 3f;

    [Header("Augment Pools")]
    public AugmentPool statAugments;
    public AugmentPool weaponAugments;

    [Header("Selection Settings")]
    public int choicesPerWave = 3;
    [Range(0f, 1f)]
    public float weaponChance = 0.5f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip drinkSound;
    public AudioClip drinkSound2;

    private List<Augment> generatedChoices = new List<Augment>();
    private List<GameObject> currentDrinks = new List<GameObject>();
    private List<GameObject> keptWeapons = new List<GameObject>();
    private bool isSelectionActive = false;

    private int pickedWeapons = 0;
    private int maxPlayerWeapons = 2;


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

        int weaponCountThisWave = 0;
        int maxWeaponsThisWave = 1; // 1 weapon per wave

        for (int i = 0; i < choicesPerWave; i++)
        {
            Augment newAugment;

            bool canSpawnWeapon = (pickedWeapons < maxPlayerWeapons) && (weaponCountThisWave < maxWeaponsThisWave);
            bool chooseWeapon = canSpawnWeapon && Random.value < weaponChance;

            if (chooseWeapon)
            {
                newAugment = GetRandomAugmentFromPool(weaponAugments);
                weaponCountThisWave++;
            }
            else
            {
                newAugment = GetRandomAugmentFromPool(statAugments);
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
            string instruction = "<color=yellow>Escolha sua bebida ou arma!</color>\n";
            instruction += "Para bebidas: aproxime da sua cabeça\n";
            instruction += "Para armas: pegue com a mão";

            augmentDisplayText.text = instruction;
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


    private void SpawnDrinks()
    {
        ClearCurrentDrinks();

        if (generatedChoices.Count < drinksPerWave)
        {
            Debug.LogWarning("Não há augments suficientes!");
            return;
        }

        List<Transform> shuffledPoints = new List<Transform>(drinkSpawnPoints);
        for (int i = 0; i < shuffledPoints.Count; i++)
        {
            Transform temp = shuffledPoints[i];
            int rnd = Random.Range(i, shuffledPoints.Count);
            shuffledPoints[i] = shuffledPoints[rnd];
            shuffledPoints[rnd] = temp;
        }

        List<GameObject> availablePrefabs = new List<GameObject>(drinkPrefabs);

        int spawnCount = Mathf.Min(drinksPerWave, shuffledPoints.Count, generatedChoices.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            if (availablePrefabs.Count == 0) break;

            int prefabIndex = Random.Range(0, availablePrefabs.Count);
            GameObject chosenPrefab = availablePrefabs[prefabIndex];
            availablePrefabs.RemoveAt(prefabIndex);

            SpawnSingleDrink(shuffledPoints[i], generatedChoices[i], chosenPrefab);
        }
    }

    private void SpawnSingleDrink(Transform spawnPoint, Augment augment, GameObject drinkPrefab)
    {
        if (augment == null) return;

        GameObject spawnedObject = null;

        if (augment.augmentType == AugmentType.Weapon && augment.weaponPrefab != null)
        {
            spawnedObject = Instantiate(augment.weaponPrefab, spawnPoint.position, spawnPoint.rotation);

            WeaponPickup weaponPickup = spawnedObject.AddComponent<WeaponPickup>();
            weaponPickup.augment = augment;
            weaponPickup.augmentManager = this;
            weaponPickup.pickupDelay = 1.0f;

            SetupWeaponForPickup(spawnedObject);
            StartCoroutine(DisableColliderTemporarily(spawnedObject, 0.5f));
        }
        else
        {
            if (drinkPrefab != null)
            {
                spawnedObject = Instantiate(drinkPrefab, spawnPoint.position, spawnPoint.rotation);

                SimpleDrink drinkScript = spawnedObject.GetComponent<SimpleDrink>();
                if (drinkScript == null)
                    drinkScript = spawnedObject.AddComponent<SimpleDrink>();

                drinkScript.augment = augment;
            }
        }

        if (spawnedObject != null)
        {
            currentDrinks.Add(spawnedObject);
            CreateDrinkText(spawnedObject, augment);
        }
    }


    private IEnumerator DisableColliderTemporarily(GameObject weapon, float delay)
    {
        Collider collider = weapon.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
            yield return new WaitForSeconds(delay);
            collider.enabled = true;
        }
    }


    private void SetupWeaponForPickup(GameObject weapon)
    {
        Rigidbody rb = weapon.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = weapon.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        Collider collider = weapon.GetComponent<Collider>();
        if (collider == null)
        {
            weapon.AddComponent<BoxCollider>();
        }
    }

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

            SimpleDrink drinkScript = drink.GetComponent<SimpleDrink>();
            if (drinkScript != null)
            {
                float distance = Vector3.Distance(drink.transform.position, playerHead.position);

                if (distance < drinkDistance)
                {
                    DrinkSelected(drink);
                    break;
                }
            }
        }
    }

    public void OnWeaponPickedUp(GameObject weapon, Augment augment)
    {
        pickedWeapons++;
        ShowAugmentOnScreen(augment);

        if (Player.instance != null)
        {
            Player.instance.ApplyAugment(augment);
            ApplyWeaponEffect(weapon, augment);
        }

        currentDrinks.Remove(weapon);
        keptWeapons.Add(weapon);

        TextReference textRef = weapon.GetComponent<TextReference>();
        if (textRef != null)
        {
            if (textRef.assignedText != null)
                Destroy(textRef.assignedText);
            Destroy(textRef);
        }

        FinishSelection();
    }



    private void ApplyWeaponEffect(GameObject weapon, Augment augment)
    {
        string weaponName = augment.augmentName.ToLower();

        if (weaponName.Contains("baseball") || weaponName.Contains("bat"))
        {
            BaseballBatWeapon batScript = weapon.AddComponent<BaseballBatWeapon>();
            batScript.damageMultiplier = 1.5f;
            if (Player.instance != null)
            {
                Player.instance.currentStats.attackDamage =
                    Mathf.RoundToInt(Player.instance.currentStats.attackDamage * batScript.damageMultiplier);
            }
        }
        else if (weaponName.Contains("banana"))
        {
            BananaWeapon bananaScript = weapon.AddComponent<BananaWeapon>();
            bananaScript.isSuperBanana = augment.rarity == Rarity.Legendary;
            bananaScript.critDamageBoost = bananaScript.isSuperBanana ? 2f : 1f;
            if (Player.instance != null)
            {
                Player.instance.currentStats.critDamage += bananaScript.critDamageBoost;
            }
        }
        else if (weaponName.Contains("brass") || weaponName.Contains("knuckles"))
        {
            BrassKnucklesWeapon knucklesScript = weapon.AddComponent<BrassKnucklesWeapon>();
            knucklesScript.armorBonus = 100;
            if (Player.instance != null)
            {
                Player.instance.currentStats.armor += knucklesScript.armorBonus;
            }
        }

        Debug.Log($"Efeito da arma {augment.augmentName} aplicado!");
    }

    private void DrinkSelected(GameObject drink)
    {
        SimpleDrink drinkScript = drink.GetComponent<SimpleDrink>();
        if (drinkScript == null || drinkScript.augment == null) return;

        Augment selectedAugment = drinkScript.augment;
        Debug.Log("Augment selecionado: " + selectedAugment.augmentName);

        if (audioSource != null)
        {
            AudioClip clipToPlay = Random.value < 0.5f ? drinkSound : drinkSound2;
            audioSource.PlayOneShot(clipToPlay);
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

    private void FinishSelection()
    {
        foreach (GameObject drink in currentDrinks)
        {
            if (drink != null)
            {
                if (drink.GetComponent<SimpleDrink>() != null)
                {
                    TextReference textRef = drink.GetComponent<TextReference>();
                    if (textRef != null && textRef.assignedText != null)
                    {
                        Destroy(textRef.assignedText);
                    }
                    Destroy(drink);
                }
            }
        }

        currentDrinks.Clear();
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
            if (drink != null && !keptWeapons.Contains(drink))
            {
                Destroy(drink);
            }
        }
        currentDrinks.Clear();
    }

    private void CreateDrinkText(GameObject drink, Augment augment)
    {
        GameObject textObj = new GameObject("DrinkText");

        textObj.transform.position = drink.transform.position + Vector3.up * 0.4f;

        TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
        textMesh.text = augment.augmentName + "\n" + augment.description;
        textMesh.fontSize = 0.5f;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = GetColorByRarity(augment.rarity);
        textMesh.textWrappingMode = TextWrappingModes.Normal;
        textMesh.enableAutoSizing = false;
        textMesh.rectTransform.localScale = Vector3.one; // Garantir scale 1

        // Tamanho fixo para o texto
        textMesh.rectTransform.sizeDelta = new Vector2(3, 1);

        TextFollower textFollower = textObj.AddComponent<TextFollower>();
        textFollower.target = drink.transform;
        textFollower.verticalOffset = 0.4f;

        BillboardText billboard = textObj.AddComponent<BillboardText>();
        billboard.target = playerHead;

        if (drink.GetComponent<TextReference>() == null)
        {
            TextReference textRef = drink.AddComponent<TextReference>();
            textRef.assignedText = textObj;
        }
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
