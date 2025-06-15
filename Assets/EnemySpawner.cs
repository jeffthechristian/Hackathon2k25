using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // The Enemy prefab to spawn
    public MoneyManager moneyManager; // Reference to MoneyManager
    public Transform[] spawnPointTransforms; // Assign spawn point GameObjects in the Inspector
    private Vector3[] spawnPoints; // Internal array for spawn positions
    private Transform target; // The target object (Ring)
    private int currentWave = 0; // Current wave number (0 means no wave active)
    private int currentEnemyCount = 0; // Track active enemies
    private bool isWaveActive = false; // Is a wave currently running
    private int[] enemiesPerWave = { 3, 6, 12, 18, 24, 32, 40 }; // Enemies per wave
    private float[] spawnRates = { 5f, 4f, 3f, 2.5f, 2f, 1.5f, 1f }; // Spawn rate per wave
    private float[] enemyHealthMultipliers = { 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f }; // Health multiplier per wave
    public Animator shopUIAnimator;
    void Start()
    {
        // Find the target object with the "Ring" tag
        target = GameObject.FindGameObjectWithTag("Ring").transform;
        if (target == null)
        {
            Debug.LogError("Ring not found! Please ensure an object with tag 'Ring' exists.");
            return;
        }

        // Initialize spawn points from assigned transforms
        InitializeSpawnPoints();
    }

    void InitializeSpawnPoints()
    {
        if (spawnPointTransforms == null || spawnPointTransforms.Length == 0)
        {
            Debug.LogError("No spawn points assigned! Please assign spawn points in the Inspector.");
            spawnPoints = new Vector3[0];
            return;
        }

        spawnPoints = new Vector3[spawnPointTransforms.Length];
        for (int i = 0; i < spawnPointTransforms.Length; i++)
        {
            if (spawnPointTransforms[i] != null)
            {
                spawnPoints[i] = spawnPointTransforms[i].position;
            }
            else
            {
                Debug.LogWarning($"Spawn point {i} is not assigned!");
                spawnPoints[i] = target.position; // Fallback to target position
            }
        }
    }

    // Public method to start the next wave
    public void StartNextWave()
    {
        if (isWaveActive)
        {
            Debug.Log("Cannot start new wave: Current wave still active!");
            return;
        }

        if (spawnPoints.Length == 0)
        {
            Debug.LogError("Cannot start wave: No valid spawn points available!");
            return;
        }

        currentWave++;
        isWaveActive = true;
        currentEnemyCount = 0;
        Debug.Log($"Starting Wave {currentWave}");
        if (shopUIAnimator != null)
            shopUIAnimator.SetTrigger("goUp");
        // Calculate enemies for this wave
        int maxEnemiesThisWave = currentWave <= enemiesPerWave.Length
            ? enemiesPerWave[currentWave - 1]
            : enemiesPerWave[enemiesPerWave.Length - 1] + (currentWave - enemiesPerWave.Length) * 5;

        // Use last defined spawn rate and health multiplier for waves beyond 7
        float spawnRateThisWave = currentWave <= spawnRates.Length
            ? spawnRates[currentWave - 1]
            : spawnRates[spawnRates.Length - 1];
        float healthMultiplier = currentWave <= enemyHealthMultipliers.Length
            ? enemyHealthMultipliers[currentWave - 1]
            : enemyHealthMultipliers[enemyHealthMultipliers.Length - 1];

        StartCoroutine(SpawnWave(maxEnemiesThisWave, spawnRateThisWave, healthMultiplier));
    }

    IEnumerator SpawnWave(int maxEnemies, float spawnRate, float healthMultiplier)
    {
        while (currentEnemyCount < maxEnemies)
        {
            SpawnEnemy(healthMultiplier);
            currentEnemyCount++;
            yield return new WaitForSeconds(spawnRate);
        }

        // Wait until all enemies are defeated
        while (GameObject.FindGameObjectsWithTag("Enemy").Length > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }

        isWaveActive = false;
        Debug.Log($"Wave {currentWave} completed!");

        if (shopUIAnimator != null)
            shopUIAnimator.SetTrigger("goDown");
    }

    void SpawnEnemy(float healthMultiplier)
    {
        // Select a random spawn point from predefined positions
        Vector3 spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Instantiate enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // Set MoneyManager and health multiplier on the enemy
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            if (moneyManager != null)
            {
                enemyScript.moneyManager = moneyManager;
            }
            enemyScript.health *= healthMultiplier; // Scale enemy health
        }
    }

    // Called by Enemy when it dies to update count
    public void EnemyDied()
    {
        currentEnemyCount--;
    }
}