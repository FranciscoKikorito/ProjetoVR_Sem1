using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class Tutorial : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform spawnPointNormal;
    public Transform spawnPoint360;

    [Header("Drink Prefabs")]
    public GameObject normalDrinkPrefab;
    public GameObject mode360DrinkPrefab;

    [Header("Player Head")]
    public Transform playerHead;

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
    }

    private void SpawnTutorialDrinks()
    {
        // Spawn Normal drink
        if (normalDrinkPrefab != null && spawnPointNormal != null)
        {
            GameObject drinkNormal = Instantiate(normalDrinkPrefab, spawnPointNormal.position, spawnPointNormal.rotation);
            CreateDrinkText(drinkNormal, "Normal");
        }

        // Spawn 360 Mode drink
        if (mode360DrinkPrefab != null && spawnPoint360 != null)
        {
            GameObject drink360 = Instantiate(mode360DrinkPrefab, spawnPoint360.position, spawnPoint360.rotation);
            CreateDrinkText(drink360, "360 Mode");
        }
    }

    private void CreateDrinkText(GameObject drink, string text)
    {
        GameObject textObj = new GameObject("DrinkText");
        textObj.transform.SetParent(drink.transform);
        textObj.transform.localPosition = Vector3.up * 0.4f;

        TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
        textMesh.text = text;
        textMesh.fontSize = 2.5f;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.white;
        textMesh.enableAutoSizing = false;

        textMesh.rectTransform.sizeDelta = new Vector2(2f, 0.5f);
        textMesh.rectTransform.localScale = Vector3.one;
        textMesh.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        textMesh.rectTransform.localPosition = Vector3.zero;

        // Outline for readability
        textMesh.outlineWidth = 0.25f;
        textMesh.outlineColor = Color.black;

        // Store for manual billboard update
        billboardTexts.Add(textObj.transform);
    }

    // Billboard method instead of separate class
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
}
