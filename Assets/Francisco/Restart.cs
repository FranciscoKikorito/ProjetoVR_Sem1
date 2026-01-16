using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    [Header("Spawn Point")]
    public Transform spawnPoint;

    [Header("Drink Prefab")]
    public GameObject drinkPrefab;

    [Header("Player Head")]
    public Transform playerHead;

    [Header("Drink Settings")]
    public float drinkDistance = 0.4f;

    [Header("Wave")]
    public Transform deathStatsPoint;
    public Spawner spawner;
    public Spawner spawner2;

    [Header("Scene Settings")]
    public string sceneToLoad;

    private GameObject currentDrink;
    private Transform billboardText;
    private TextMeshPro textMesh;

    private void Start()
    {
        if (playerHead == null)
            playerHead = Camera.main?.transform;

        if (spawner == null)
            spawner = spawner2;

        SpawnRestartDrink();
    }

    private void Update()
    {
        UpdateBillboard();
        CheckForDrinkConsumption();
    }

    private void SpawnRestartDrink()
    {
        if (drinkPrefab == null || spawnPoint == null) return;

        currentDrink = Instantiate(drinkPrefab, spawnPoint.position, spawnPoint.rotation);

        var td = currentDrink.AddComponent<RestartDrink>();
        td.isRestartDrink = true;

        CreateDrinkText(currentDrink);
    }

    private void CreateDrinkText(GameObject drink)
    {
        Renderer rend = drink.GetComponentInChildren<Renderer>();
        Vector3 topOffset = Vector3.up * 0.4f;
        if (rend != null)
            topOffset = Vector3.up * (rend.bounds.size.y + 0.1f);

        GameObject textObj = new GameObject("DrinkText");
        textObj.transform.SetParent(drink.transform);
        textObj.transform.position = drink.transform.position + topOffset;

        textMesh = textObj.AddComponent<TextMeshPro>();
        textMesh.fontSize = 2.5f;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.white;
        textMesh.enableAutoSizing = false;
        textMesh.rectTransform.localScale = Vector3.one;
        textMesh.rectTransform.sizeDelta = new Vector2(3f, 1f);
        textMesh.outlineWidth = 0.25f;
        textMesh.outlineColor = Color.black;

        UpdateWaveText();

        billboardText = textObj.transform;
    }

    private void UpdateWaveText()
    {
        if (textMesh == null) return;

        int survivedWaves = 0;

        if (spawner != null)
            survivedWaves = Mathf.Max(0, spawner.GetCurrentWave() - 1);

        textMesh.text = "Restart\n\nSurvived waves : " + survivedWaves;
    }

    private void UpdateBillboard()
    {
        if (playerHead == null || billboardText == null) return;

        Vector3 lookDirection = billboardText.position - playerHead.position;
        lookDirection.y = 0;

        if (lookDirection.sqrMagnitude > 0.001f)
            billboardText.rotation = Quaternion.LookRotation(lookDirection);
    }

    private void CheckForDrinkConsumption()
    {
        if (playerHead == null || currentDrink == null) return;

        float distance = Vector3.Distance(currentDrink.transform.position, playerHead.position);

        if (distance < drinkDistance)
            DrinkSelected();
    }

    private void DrinkSelected()
    {
        if (currentDrink != null)
            Destroy(currentDrink);

        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

public class RestartDrink : MonoBehaviour
{
    public bool isRestartDrink = false;
}
