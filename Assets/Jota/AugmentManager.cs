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
    private List<GameObject> keptWeapons = new List<GameObject>(); // Armas que NÃO devem ser destruídas
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

        // Verificar se jogador já tem 2 armas
        bool canSpawnWeapons = Player.instance != null && Player.instance.activeAugments.Count < 2;
        float actualWeaponChance = canSpawnWeapons ? weaponChance : 0f;

        for (int i = 0; i < choicesPerWave; i++)
        {
            Augment newAugment;

            bool chooseWeapon = Random.value < actualWeaponChance;

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
            // Spawnar arma usando o prefab do augment
            spawnedObject = Instantiate(augment.weaponPrefab, spawnPoint.position, spawnPoint.rotation);

            // Adicionar script de pickup para arma
            WeaponPickup weaponPickup = spawnedObject.AddComponent<WeaponPickup>();
            weaponPickup.augment = augment;
            weaponPickup.augmentManager = this;

            // Configurar física para ser pego
            SetupWeaponForPickup(spawnedObject);

            Debug.Log($"Arma spawnada: {augment.augmentName}");
        }
        else
        {
            // Spawnar bebida normal
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

    // =============================

    private void SetupWeaponForPickup(GameObject weapon)
    {
        // Garantir que tem Rigidbody
        Rigidbody rb = weapon.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = weapon.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Garantir que tem collider
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

            // Verificar se é bebida (tem SimpleDrink) e está perto da cabeça
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
            // Armas são pegas pelo WeaponPickup script, não por distância
        }
    }

    public void OnWeaponPickedUp(GameObject weapon, Augment augment)
    {
        Debug.Log("Arma pega: " + augment.augmentName);

        if (audioSource != null && drinkSound != null)
        {
            audioSource.PlayOneShot(drinkSound);
        }

        ShowAugmentOnScreen(augment);

        if (Player.instance != null)
        {
            Player.instance.ApplyAugment(augment);

            // Aplicar efeito específico da arma
            ApplyWeaponEffect(weapon, augment);
        }

        // Remover da lista de drinks (para não ser destruída)
        currentDrinks.Remove(weapon);

        // Adicionar à lista de armas mantidas
        keptWeapons.Add(weapon);

        // Remover texto da arma
        BillboardText text = weapon.GetComponentInChildren<BillboardText>();
        if (text != null)
        {
            Destroy(text.gameObject);
        }

        // Fechar seleção
        FinishSelection();
    }

    private void ApplyWeaponEffect(GameObject weapon, Augment augment)
    {
        // Adicionar script HandPunch para detectar dano
        HandPunch weaponHandPunch = weapon.AddComponent<HandPunch>();
        weaponHandPunch.player = Player.instance;
        weaponHandPunch.enemyLayer = LayerMask.GetMask("Enemy");

        // Copiar configurações de áudio e VFX das mãos para a arma
        CopyHandPunchSettings(weaponHandPunch);

        // Adicionar script específico da arma baseado no nome
        string weaponName = augment.augmentName.ToLower();

        if (weaponName.Contains("baseball") || weaponName.Contains("bat"))
        {
            BaseballBatWeapon batScript = weapon.AddComponent<BaseballBatWeapon>();
            batScript.damageMultiplier = 2f;
        }
        else if (weaponName.Contains("banana"))
        {
            BananaWeapon bananaScript = weapon.AddComponent<BananaWeapon>();
            bananaScript.isSuperBanana = augment.rarity == Rarity.Legendary;
            bananaScript.critDamageBoost = bananaScript.isSuperBanana ? 5f : 1f;
        }
        else if (weaponName.Contains("tankard") || weaponName.Contains("caneca"))
        {
            TankardWeapon tankardScript = weapon.AddComponent<TankardWeapon>();
        }
        else if (weaponName.Contains("brass") || weaponName.Contains("knuckles"))
        {
            BrassKnucklesWeapon knucklesScript = weapon.AddComponent<BrassKnucklesWeapon>();
            knucklesScript.armorBonus = 20;
        }

        Debug.Log($"Efeito da arma {augment.augmentName} aplicado!");
    }

    private void CopyHandPunchSettings(HandPunch weaponHandPunch)
    {
        // Buscar automaticamente os HandPunch das mãos
        HandPunch[] allHandPunches = FindObjectsOfType<HandPunch>();
        HandPunch referenceHandPunch = null;

        foreach (HandPunch hp in allHandPunches)
        {
            // Encontrar um HandPunch que não seja de uma arma (assume que está nas mãos do jogador)
            if (hp.gameObject != weaponHandPunch.gameObject && hp.player != null)
            {
                referenceHandPunch = hp;
                break;
            }
        }

        if (referenceHandPunch == null)
        {
            Debug.LogWarning("Não foi encontrado HandPunch das mãos para copiar configurações!");
            return;
        }

        // Copiar configurações de áudio
        weaponHandPunch.hitSounds = referenceHandPunch.hitSounds;
        weaponHandPunch.volume = referenceHandPunch.volume;

        // Garantir AudioSource
        AudioSource weaponAudioSource = weaponHandPunch.gameObject.GetComponent<AudioSource>();
        if (weaponAudioSource == null)
        {
            weaponAudioSource = weaponHandPunch.gameObject.AddComponent<AudioSource>();
        }

        // Configurar AudioSource
        weaponAudioSource.spatialBlend = 1f;
        weaponAudioSource.maxDistance = 10f;
        weaponAudioSource.minDistance = 0.5f;

        weaponHandPunch.hitAudioSource = weaponAudioSource;

        // Copiar VFX
        weaponHandPunch.hitVFXPrefabs = referenceHandPunch.hitVFXPrefabs;
        weaponHandPunch.destroyVFXAfter = referenceHandPunch.destroyVFXAfter;

        // Copiar outras configurações
        weaponHandPunch.punchVelocityThreshold = referenceHandPunch.punchVelocityThreshold;

        Debug.Log($"Configurações de HandPunch copiadas de {referenceHandPunch.gameObject.name}");
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

    private void FinishSelection()
    {
        // Destruir apenas bebidas, manter armas
        foreach (GameObject drink in currentDrinks)
        {
            if (drink != null)
            {
                // Verificar se é bebida (tem SimpleDrink)
                if (drink.GetComponent<SimpleDrink>() != null)
                {
                    Destroy(drink);
                }
                // Armas sem SimpleDrink ficam no jogo
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
            // Não destruir armas mantidas
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
