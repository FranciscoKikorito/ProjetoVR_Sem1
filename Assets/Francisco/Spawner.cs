using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Enemies & Spawn Points")]
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private List<Transform> spawnPoints;

    [Header("Enemy List")]
    [SerializeField] private GameObject enemyList; // Parent object for spawned enemies

    [Header("Wave Settings")]
    [SerializeField] private float spawnFrequency;
    [SerializeField] private float timeBetweenWaves;
    [SerializeField] private float enemyHealthIncrease;
    [SerializeField] private int enemyIncreaseNumber;
    [SerializeField] private float initialEnemyHealth;
    [SerializeField] private int initialEnemyNumber;

    [Header("Augment System")]
    [SerializeField] private AugmentManager augmentManager;
    [SerializeField] private Player player;

    private Wave wave;

    private float spawnTimer;
    private int currentEnemies;
    private int enemiesSpawnedThisWave;
    private bool isWaitingForNextWave = false;

    // Prevent same spawn point twice in a row
    private int lastSpawnPointIndex = -1;

    void Start()
    {
        wave = new Wave(1, initialEnemyHealth, initialEnemyNumber);

        spawnTimer = 0f;
        currentEnemies = 0;
        enemiesSpawnedThisWave = 0;
        isWaitingForNextWave = false;

        // Safety check
        if (enemyList == null)
        {
            Debug.LogWarning("Enemy List is not assigned in Spawner. Enemies will spawn without parent.");
        }
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

        // Instantiate enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        // ✅ Set parent to enemyList if assigned
        if (enemyList != null)
        {
            enemy.transform.SetParent(enemyList.transform);
        }

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

    private HashSet<GameObject> deadEnemies = new HashSet<GameObject>();

    public void NotifyDeath(GameObject enemy)
    {
        // Only count this enemy once
        if (deadEnemies.Contains(enemy))
            return;

        deadEnemies.Add(enemy); // mark as counted
        currentEnemies--;

        if (!isWaitingForNextWave && enemiesSpawnedThisWave >= wave.maxEnemyNum && currentEnemies <= 0)
        {
            isWaitingForNextWave = true; // prevent multiple triggers
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

        // Difficulty scaling
        if (wave.waveNum == 5 || wave.waveNum == 10 || wave.waveNum == 15 || wave.waveNum == 20)
        {
            spawnFrequency = Mathf.Max(0f, spawnFrequency - 1f);
            player.enemyDamagePerHit += 25;
            Debug.Log($"Wave {wave.waveNum}: spawn frequency reduced! New spawnFrequency = {spawnFrequency}");
        }

        enemiesSpawnedThisWave = 0;
        currentEnemies = 0;
        spawnTimer = 0f;
        isWaitingForNextWave = false;

        Debug.Log($"Wave {wave.waveNum} started! Enemy count: {wave.maxEnemyNum}, Health: {wave.enemyHealth}");
    }

    public int GetCurrentWave()
    {
        return wave.waveNum;
    }

}
