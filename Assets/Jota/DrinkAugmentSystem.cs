using System.Collections.Generic;
using UnityEngine;

public class DrinkAugmentSystem : MonoBehaviour
{
    [Header("Configuração Básica")]
    public int drinksPerWave = 3;
    public float drinkDistance = 0.4f; // Distância para beber

    [Header("Pontos de Spawn")]
    public Transform[] spawnPoints; // Onde as bebidas aparecem

    [Header("Prefab da Bebida")]
    public GameObject drinkPrefab; // UM só prefab de bebida

    [Header("Lista de Augments")]
    public Augment[] allAugments; // Todos augments disponíveis

    private List<GameObject> currentDrinks = new List<GameObject>();
    private Transform playerHead;
    private bool isActive = false;

    void Start()
    {
        playerHead = Camera.main?.transform;
    }

    void Update()
    {
        if (!isActive || playerHead == null) return;

        // Verificar cada bebida
        for (int i = currentDrinks.Count - 1; i >= 0; i--)
        {
            GameObject drink = currentDrinks[i];
            if (drink == null) continue;

            // Calcular distância até a cabeça do jogador
            float distance = Vector3.Distance(drink.transform.position, playerHead.position);

            // Se estiver perto o suficiente, beber
            if (distance < drinkDistance)
            {
                DrinkSelected(drink);
                break; // Só bebe uma por vez
            }
        }
    }

    // Chamado pelo Spawner quando uma wave acaba
    public void ShowDrinks()
    {
        Debug.Log("Mostrando bebidas para escolha");
        isActive = true;

        // Limpar bebidas antigas
        ClearDrinks();

        // Pegar augments aleatórios
        Augment[] randomAugments = GetRandomAugments(drinksPerWave);

        // Spawnar bebidas
        for (int i = 0; i < Mathf.Min(randomAugments.Length, spawnPoints.Length); i++)
        {
            SpawnDrink(spawnPoints[i], randomAugments[i]);
        }
    }

    private Augment[] GetRandomAugments(int count)
    {
        // Embaralhar lista de augments
        List<Augment> shuffled = new List<Augment>(allAugments);

        for (int i = 0; i < shuffled.Count; i++)
        {
            Augment temp = shuffled[i];
            int randomIndex = Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        // Pegar os primeiros 'count' augments
        return shuffled.GetRange(0, Mathf.Min(count, shuffled.Count)).ToArray();
    }

    private void SpawnDrink(Transform spawnPoint, Augment augment)
    {
        if (drinkPrefab == null || augment == null) return;

        // Criar bebida
        GameObject drink = Instantiate(drinkPrefab, spawnPoint.position, spawnPoint.rotation);
        currentDrinks.Add(drink);

        // Adicionar componente simples na bebida
        SimpleDrink drinkScript = drink.AddComponent<SimpleDrink>();
        drinkScript.augment = augment;

        // Criar texto flutuante (opcional, mas útil)
        CreateFloatingText(drink, augment);
    }

    private void CreateFloatingText(GameObject drink, Augment augment)
    {
        // Criar objeto de texto simples
        GameObject textObj = new GameObject("DrinkText");
        textObj.transform.position = drink.transform.position + Vector3.up * 0.3f;
        textObj.transform.SetParent(drink.transform);

        // Adicionar TextMesh (mais simples que TextMeshPro)
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = augment.augmentName + "\n" + augment.description;
        textMesh.characterSize = 0.05f;
        textMesh.fontSize = 40;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.yellow;
    }

    private void DrinkSelected(GameObject drink)
    {
        SimpleDrink drinkScript = drink.GetComponent<SimpleDrink>();
        if (drinkScript == null || drinkScript.augment == null) return;

        Augment selectedAugment = drinkScript.augment;
        Debug.Log("Bebendo: " + selectedAugment.augmentName);

        // Aplicar augment ao jogador
        if (Player.instance != null)
        {
            Player.instance.ApplyAugment(selectedAugment);
        }

        // Destruir todas as bebidas
        ClearDrinks();

        // Desativar sistema
        isActive = false;

        // Continuar para próxima wave
        FindAnyObjectByType<Spawner>()?.ContinueToNextWave();
    }

    private void ClearDrinks()
    {
        foreach (GameObject drink in currentDrinks)
        {
            if (drink != null)
                Destroy(drink);
        }
        currentDrinks.Clear();
    }
}