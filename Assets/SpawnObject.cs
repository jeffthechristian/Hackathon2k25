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
        public Quaternion spawnRotation = Quaternion.identity; // Single rotation for all spawn positions
        [HideInInspector] public int spawnCount = 0; // Track how many times it has been spawned
        [HideInInspector] public List<GameObject> spawnedObjects = new List<GameObject>(); // Track spawned objects
    }

    public List<SpawnablePrefab> spawnables = new List<SpawnablePrefab>();
    public MoneyManager moneyManager;
    public WallUpgrader wallUpgrader;

    public int repairCost = 50; // Cost to repair
    public int upgradeCost = 100; // Cost to upgrade from Wall1 to Wall2

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
        GameObject spawnedObject = Instantiate(match.prefab, spawnPos.position, match.spawnRotation); // Use single rotation
        match.spawnCount++;
        match.spawnedObjects.Add(spawnedObject); // Track the spawned object
        moneyManager.ObjectBought();

        // Attach a script to the spawned object to notify when it's removed
        var tracker = spawnedObject.AddComponent<SpawnedObjectTracker>();
        tracker.Initialize(this, match); // Pass SpawnManager and SpawnablePrefab to the tracker
    }

    // Called when a spawned object is removed
    public void OnObjectRemoved(SpawnablePrefab prefab)
    {
        if (prefab.spawnCount > 0)
        {
            prefab.spawnCount--; // Decrease spawn count
            prefab.spawnedObjects.RemoveAll(obj => obj == null); // Clean up null references
            Debug.Log($"Spawn count for item ID {prefab.id} decreased to {prefab.spawnCount}.");
        }
    }

    public void RepairWall()
    {
        if (!moneyManager)
        {
            Debug.LogWarning("MoneyManager not assigned.");
            return;
        }

        if (!wallUpgrader)
        {
            Debug.LogWarning("WallUpgrader not assigned.");
            return;
        }

        // Check if either wall1 or wall2 is already active
        if (wallUpgrader.wall1 != null && wallUpgrader.wall2 != null)
        {
            if (wallUpgrader.wall1.activeSelf || wallUpgrader.wall2.activeSelf)
            {
                Debug.Log("Wall repair skipped: Wall1 or Wall2 is already active.");
                return;
            }
        }
        else
        {
            Debug.LogWarning("WallUpgrader: Wall1 or Wall2 is not assigned.");
            return;
        }

        if (!moneyManager.SpendMoney(repairCost))
        {
            Debug.Log("Not enough money to repair the wall.");
            return;
        }

        wallUpgrader.EnableWall1();
        moneyManager.ObjectBought();
        Debug.Log($"Wall repaired for {repairCost} coins.");
    }

    public void UpgradeWall()
    {
        if (!moneyManager)
        {
            Debug.LogWarning("MoneyManager not assigned.");
            return;
        }

        if (!wallUpgrader)
        {
            Debug.LogWarning("WallUpgrader not assigned.");
            return;
        }

        // Check if wall2 is already active
        if (wallUpgrader.wall2 != null && wallUpgrader.wall2.activeSelf)
        {
            Debug.Log("Wall upgrade skipped: Wall2 is already active.");
            return;
        }

        if (!moneyManager.SpendMoney(upgradeCost))
        {
            Debug.Log("Not enough money to upgrade the wall.");
            return;
        }

        wallUpgrader.UpgradeWall();
        moneyManager.ObjectBought();
        Debug.Log($"Wall upgraded for {upgradeCost} coins.");
    }
}

// Script to attach to spawned objects to track their lifecycle
public class SpawnedObjectTracker : MonoBehaviour
{
    private SpawnManager spawnManager;
    private SpawnManager.SpawnablePrefab prefab;

    public void Initialize(SpawnManager manager, SpawnManager.SpawnablePrefab prefab)
    {
        this.spawnManager = manager;
        this.prefab = prefab;
    }

    private void OnDestroy()
    {
        // Notify SpawnManager when this object is destroyed
        if (spawnManager != null)
        {
            spawnManager.OnObjectRemoved(prefab);
        }
    }
}