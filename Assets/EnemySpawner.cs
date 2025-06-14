using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // The Enemy prefab to spawn
    public float minSpawnRadius = 5f; // Minimum distance from target to spawn
    public float maxSpawnRadius = 15f; // Maximum distance from target to spawn
    public float spawnRate = 5f; // Time between spawns in seconds
    public int maxEnemies = 10; // Maximum number of enemies at once
    public MoneyManager moneyManager; // Reference to MoneyManager

    private Transform target; // The target object (Ring)
    private int currentEnemyCount = 0; // Track active enemies

    void Start()
    {
        // Find the target object with the "Ring" tag
        target = GameObject.FindGameObjectWithTag("Ring").transform;
        if (target == null)
        {
            Debug.LogError("Ring not found! Please ensure an object with tag 'Ring' exists.");
            return;
        }

        // Start spawning enemies
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        while (true)
        {
            // Only spawn if below max enemy count
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
                currentEnemyCount++;
            }
            yield return new WaitForSeconds(spawnRate);
        }
    }

    void SpawnEnemy()
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

        // Set MoneyManager reference on the enemy
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null && moneyManager != null)
        {
            enemyScript.moneyManager = moneyManager;
        }

    }
}