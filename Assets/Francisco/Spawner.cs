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

    private Wave wave;

    private float spawnTimer;
    private int currentEnemies;            // Enemies currently alive
    private int enemiesSpawnedThisWave;    // Number of enemies spawned in this wave
    private bool isWaitingForNextWave = false;

    // NEW: prevent same spawn point twice in a row
    private int lastSpawnPointIndex = -1;

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

        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

        int spawnIndex;
        if (spawnPoints.Count == 1)
        {
            spawnIndex = 0;
        }
        else
        {
            do
            {
                spawnIndex = Random.Range(0, spawnPoints.Count);
            } while (spawnIndex == lastSpawnPointIndex);
        }

        lastSpawnPointIndex = spawnIndex;
        Transform spawnPoint = spawnPoints[spawnIndex];

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

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

        if (augmentManager != null)
        {
            augmentManager.ShowAugmentSelection();
        }
        else
        {
            ContinueToNextWave();
        }
    }

    public void ContinueToNextWave()
    {
        wave.waveNum++;

        // Increase enemy number only every 2 waves
        if (wave.waveNum % 2 == 1)
        {
            wave.maxEnemyNum += enemyIncreaseNumber;
        }

        // Always increase health
        wave.enemyHealth += enemyHealthIncrease;

        // Reduce spawn timer at waves 5 and 10...
        if (wave.waveNum == 5 || wave.waveNum == 10 || wave.waveNum == 15 || wave.waveNum == 20)
        {
            spawnFrequency = Mathf.Max(0f, spawnFrequency - 1f);
            Debug.Log($"Wave {wave.waveNum}: spawn frequency reduced! New spawnFrequency = {spawnFrequency}");
        }

        enemiesSpawnedThisWave = 0;
        currentEnemies = 0;
        spawnTimer = 0f;
        isWaitingForNextWave = false;

        Debug.Log($"Wave {wave.waveNum} started! Enemy count: {wave.maxEnemyNum}, Health: {wave.enemyHealth}");
    }
}
