using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Enemies & Spawn Points")]
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private List<Transform> spawnPoints;

    [Header("Enemy List")]
    [SerializeField] private GameObject enemyList;

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
    private int lastSpawnPointIndex = -1;
    private HashSet<GameObject> deadEnemies = new HashSet<GameObject>();

    void Start()
    {
        wave = new Wave(1, initialEnemyHealth, initialEnemyNumber);

        spawnTimer = 0f;
        currentEnemies = 0;
        enemiesSpawnedThisWave = 0;
        isWaitingForNextWave = false;

        if (enemyList == null)
            Debug.LogWarning("Enemy List is not assigned in Spawner. Enemies will spawn without parent.");
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
            do { spawnIndex = Random.Range(0, spawnPoints.Count); }
            while (spawnIndex == lastSpawnPointIndex);
        }

        lastSpawnPointIndex = spawnIndex;
        Transform spawnPoint = spawnPoints[spawnIndex];

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        if (enemyList != null)
            enemy.transform.SetParent(enemyList.transform);

        Health health = enemy.GetComponentInChildren<Health>();
        if (health != null)
            health.InitializeEnemy(wave.enemyHealth, this);

        enemiesSpawnedThisWave++;
        currentEnemies++;

        Debug.Log($"Spawned enemy {enemiesSpawnedThisWave}/{wave.maxEnemyNum} at {spawnPoint.position}");
    }

    public void NotifyDeath(GameObject enemy)
    {
        if (deadEnemies.Contains(enemy)) return;

        deadEnemies.Add(enemy);
        currentEnemies--;

        if (!isWaitingForNextWave && enemiesSpawnedThisWave >= wave.maxEnemyNum && currentEnemies <= 0)
        {
            isWaitingForNextWave = true;
            StartCoroutine(AdvanceWaveAfterDelay());
        }
    }

    private IEnumerator AdvanceWaveAfterDelay()
    {
        Debug.Log($"Wave {wave.waveNum} finished. Next wave in {timeBetweenWaves} seconds.");
        yield return new WaitForSeconds(timeBetweenWaves);

        if (augmentManager != null)
            augmentManager.ShowAugmentSelection();
        else
            ContinueToNextWave();
    }

    public void ContinueToNextWave()
    {
        wave.waveNum++;

        if (wave.waveNum % 2 == 1)
            wave.maxEnemyNum += enemyIncreaseNumber;

        wave.enemyHealth += enemyHealthIncrease;

        if (wave.waveNum == 5 || wave.waveNum == 10 || wave.waveNum == 15 || wave.waveNum == 20)
            spawnFrequency = Mathf.Max(0f, spawnFrequency - 1f);

        enemiesSpawnedThisWave = 0;
        currentEnemies = 0;
        spawnTimer = 0f;
        isWaitingForNextWave = false;
        deadEnemies.Clear();

        Debug.Log($"Wave {wave.waveNum} started! Enemy count: {wave.maxEnemyNum}, Health: {wave.enemyHealth}");
    }

    public int GetCurrentWave() => wave.waveNum;
}
