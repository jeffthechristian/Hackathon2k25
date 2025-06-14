using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // The Enemy prefab to spawn
    public float minSpawnRadius = 5f; // Minimum distance from target to spawn
    public float maxSpawnRadius = 15f; // Maximum distance from target to spawn
    public MoneyManager moneyManager; // Reference to MoneyManager

    private Transform target; // The target object (Ring)
    private int currentWave = 0; // Current wave number (0 means no wave active)
    private int currentEnemyCount = 0; // Track active enemies
    private bool isWaveActive = false; // Is a wave currently running
    private int[] enemiesPerWave = { 3, 6, 12, 18, 24, 32, 40 }; // Enemies per wave
    private float[] spawnRates = { 5f, 4f, 3f, 2.5f, 2f, 1.5f, 1f }; // Spawn rate per wave
    private float[] enemyHealthMultipliers = { 1f, 1.2f, 1.4f, 1.6f, 1.8f, 2.0f, 2.2f }; // Health multiplier per wave

    void Start()
    {
        // Find the target object with the "Ring" tag
        target = GameObject.FindGameObjectWithTag("Ring").transform;
        if (target == null)
        {
            Debug.LogError("Ring not found! Please ensure an object with tag 'Ring' exists.");
            return;
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

        currentWave++;
        isWaveActive = true;
        currentEnemyCount = 0;
        Debug.Log($"Starting Wave {currentWave}");

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
    }

    void SpawnEnemy(float healthMultiplier)
    {
        // Calculate random spawn position within the radius
        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector3 spawnPosition = new Vector3(
            target.position.x + randomCircle.x,
            target.position.y, // Assumes enemies spawn at same height as target
            target.position.z + randomCircle.y
        );

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