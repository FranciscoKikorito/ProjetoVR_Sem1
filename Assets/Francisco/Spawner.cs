using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Enemies & Spawn Points")]
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private List<Transform> spawnPoints;

    [Header("Wave Settings")]
    [SerializeField] private float spawnFrequency;
    [SerializeField] private float timeBetweenWaves;
    [SerializeField] private float enemyHealthIncrease;
    [SerializeField] private int enemyIncreaseNumber;
    [SerializeField] private float initialEnemyHealth;
    [SerializeField] private int initialEnemyNumber;

    [Header("Augment System")]
    [SerializeField] private AugmentManager augmentManager;

    [Header("Drinking System")]
    public DrinkAugmentSystem drinkSystem;

    private Wave wave;

    private float spawnTimer;
    private int currentEnemies;            // Enemies currently alive
    private int enemiesSpawnedThisWave;    // Number of enemies spawned in this wave
    private bool isWaitingForNextWave = false;

    void Start()
    {
        wave = new Wave(1, initialEnemyHealth, initialEnemyNumber);

        // Spawn first enemy immediately
        spawnTimer = 0f;
        currentEnemies = 0;
        enemiesSpawnedThisWave = 0;
        isWaitingForNextWave = false;
    }

    void Update()
    {
        // Dont spawn while waiting for next wave
        if (isWaitingForNextWave) return;

        spawnTimer -= Time.deltaTime;
        if (enemiesSpawnedThisWave < wave.maxEnemyNum && spawnTimer <= 0f)
        {
            SpawnEnemy();
            spawnTimer = spawnFrequency;
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs.Count == 0 || spawnPoints.Count == 0)
        {
            Debug.LogWarning("Spawner missing enemy prefabs or spawn points");
            return;
        }

        // Pick a random enemy prefab and spawn point
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        // Initialize the enemy's health
        Health health = enemy.GetComponentInChildren<Health>();
        if (health != null)
        {
            health.InitializeEnemy(wave.enemyHealth, this);
        }
        else
        {
            Debug.LogError("Spawned enemy has no Health component!", enemy);
        }

        enemiesSpawnedThisWave++;
        currentEnemies++;

        Debug.Log($"Spawned enemy {enemiesSpawnedThisWave}/{wave.maxEnemyNum} at {spawnPoint.position}");
    }

    // Called by enemy on death
    public void NotifyDeath()
    {
        currentEnemies--;

        if (enemiesSpawnedThisWave > 0 && currentEnemies <= 0 && enemiesSpawnedThisWave >= wave.maxEnemyNum)
        {
            StartCoroutine(AdvanceWaveAfterDelay());
        }
    }

    private IEnumerator AdvanceWaveAfterDelay()
    {
        isWaitingForNextWave = true;

        Debug.Log($"Wave {wave.waveNum} finished. Next wave in {timeBetweenWaves} seconds.");

        yield return new WaitForSeconds(timeBetweenWaves);

        // MOSTRAR AUGMENTS AQUI
        if (augmentManager != null)
        {
            augmentManager.ShowAugmentSelection();
            // Esperar até que o augment seja selecionado
            while (Time.timeScale == 0f)
            {
                yield return null;
            }
        }

        // Mostrar bebidas se tiver sistema
        if (drinkSystem != null)
        {
            drinkSystem.ShowDrinks();
        }
        else
        {
            // Se não tiver sistema, continuar direto
            ContinueToNextWave();
        }
    }

    public void ContinueToNextWave()
    {
        // Avançar para próxima wave
        wave.waveNum++;
        wave.maxEnemyNum += enemyIncreaseNumber;
        wave.enemyHealth += enemyHealthIncrease;

        enemiesSpawnedThisWave = 0;
        currentEnemies = 0;
        spawnTimer = 0f;
        isWaitingForNextWave = false;

        Debug.Log($"Wave {wave.waveNum} started!");
    }
}
