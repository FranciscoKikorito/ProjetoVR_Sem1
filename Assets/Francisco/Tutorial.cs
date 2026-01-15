using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform spawnPointNormal;
    public Transform spawnPoint360;

    [Header("Drink Prefabs")]
    public GameObject drinkPrefab; // only 1 prefab needed

    [Header("Wave Managers")]
    public GameObject WaveManager;      // for normal drink
    public GameObject WaveManager360;   // for 360 mode drink

    [Header("Player Head")]
    public Transform playerHead;

    [Header("Drink Settings")]
    public float drinkDistance = 0.4f;

    [Header("Tutorial Object")]
    public GameObject TutorialStart; // deactivate after drinking

    // Store spawned drinks
    private List<GameObject> currentDrinks = new List<GameObject>();

    // Store created text objects for billboard update
    private List<Transform> billboardTexts = new List<Transform>();

    private void Start()
    {
        if (playerHead == null)
            playerHead = Camera.main?.transform;

        SpawnTutorialDrinks();
    }

    private void Update()
    {
        UpdateBillboards();
        CheckForDrinkConsumption();
    }

    private void SpawnTutorialDrinks()
    {
        if (drinkPrefab == null) return;

        // Spawn Normal drink
        if (spawnPointNormal != null)
        {
            GameObject drinkNormal = Instantiate(drinkPrefab, spawnPointNormal.position, spawnPointNormal.rotation);
            var td = drinkNormal.AddComponent<TutorialDrink>();
            td.drinkType = TutorialDrink.DrinkType.Normal;
            currentDrinks.Add(drinkNormal);
            CreateDrinkText(drinkNormal, "Normal");
        }

        // Spawn 360 Mode drink
        if (spawnPoint360 != null)
        {
            GameObject drink360 = Instantiate(drinkPrefab, spawnPoint360.position, spawnPoint360.rotation);
            var td = drink360.AddComponent<TutorialDrink>();
            td.drinkType = TutorialDrink.DrinkType.Mode360;
            currentDrinks.Add(drink360);
            CreateDrinkText(drink360, "360 Mode");
        }
    }

    private void CreateDrinkText(GameObject drink, string text)
    {
        Renderer rend = drink.GetComponentInChildren<Renderer>();
        Vector3 topOffset = Vector3.up * 0.4f;
        if (rend != null)
            topOffset = Vector3.up * (rend.bounds.size.y + 0.1f);

        GameObject textObj = new GameObject("DrinkText");
        textObj.transform.SetParent(drink.transform);
        textObj.transform.position = drink.transform.position + topOffset;

        TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
        textMesh.text = text;
        textMesh.fontSize = 2.5f;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.white;
        textMesh.enableAutoSizing = false;
        textMesh.rectTransform.localScale = Vector3.one;
        textMesh.rectTransform.sizeDelta = new Vector2(2f, 0.5f);
        textMesh.outlineWidth = 0.25f;
        textMesh.outlineColor = Color.black;

        billboardTexts.Add(textObj.transform);
    }

    private void UpdateBillboards()
    {
        if (playerHead == null) return;

        foreach (Transform t in billboardTexts)
        {
            if (t == null) continue;

            Vector3 lookDirection = t.position - playerHead.position;
            lookDirection.y = 0;

            if (lookDirection.sqrMagnitude > 0.001f)
                t.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    private void CheckForDrinkConsumption()
    {
        if (playerHead == null) return;

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
        if (drink == null) return;

        TutorialDrink td = drink.GetComponent<TutorialDrink>();
        if (td == null) return;

        // Activate correct wave manager
        if (td.drinkType == TutorialDrink.DrinkType.Normal)
        {
            if (WaveManager != null) WaveManager.SetActive(true);
            Debug.Log("Normal drink consumed → WaveManager activated!");
        }
        else if (td.drinkType == TutorialDrink.DrinkType.Mode360)
        {
            if (WaveManager360 != null) WaveManager360.SetActive(true);
            Debug.Log("360 Mode drink consumed → WaveManager360 activated!");
        }

        // Deactivate tutorial start
        if (TutorialStart != null)
        {
            TutorialStart.SetActive(false);
            Debug.Log("TutorialStart deactivated!");
        }

        // Destroy all remaining drinks, including the one consumed
        foreach (GameObject d in currentDrinks)
        {
            if (d != null)
                Destroy(d);
        }

        // Clear the list
        currentDrinks.Clear();
    }
}

// Tiny component to track which type of drink it is
public class TutorialDrink : MonoBehaviour
{
    public enum DrinkType
    {
        Normal,
        Mode360
    }

    public DrinkType drinkType;
}
