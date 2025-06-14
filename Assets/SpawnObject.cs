using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnablePrefab
    {
        public int id;
        public GameObject prefab;
        public int cost;
        public Transform[] spawnPositions = new Transform[4]; // 4 predefined spawn positions
        [HideInInspector] public int spawnCount = 0; // Track how many times it has been spawned
    }

    public List<SpawnablePrefab> spawnables = new List<SpawnablePrefab>();
    public MoneyManager moneyManager;

    public void SpawnByID(int id)
    {
        var match = spawnables.Find(p => p.id == id);

        if (match == null || match.prefab == null)
        {
            Debug.LogWarning($"Prefab with ID {id} not found.");
            return;
        }

        if (match.spawnCount >= 4)
        {
            Debug.LogWarning($"Buy limit reached for item with ID {id}.");
            return;
        }

        if (!moneyManager)
        {
            Debug.LogWarning("MoneyManager not assigned.");
            return;
        }

        if (!moneyManager.SpendMoney(match.cost))
        {
            return;
        }

        Transform spawnPos = match.spawnPositions[match.spawnCount]; // Use next available position
        Instantiate(match.prefab, spawnPos.position, spawnPos.rotation);

        match.spawnCount++;
        moneyManager.ObjectBought();
    }
}
