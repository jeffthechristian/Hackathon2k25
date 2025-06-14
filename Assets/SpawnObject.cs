using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Audio;

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

    public AudioClip uiaiuiai;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
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

        // Check if a wall is already present
        if (wallUpgrader.HasWall())
        {
            Debug.Log("Wall repair skipped: A wall is already present.");
            return;
        }

        if (!moneyManager.SpendMoney(repairCost))
        {
            Debug.Log("Not enough money to repair the wall.");
            return;
        }

        wallUpgrader.SpawnWall1();
        // Reset health on the WallManager attached to the new wall
        var wallManager = wallUpgrader.GetComponentInChildren<WallManager>();
        if (wallManager != null)
        {
            wallManager.ResetHealth();
        }
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

        // Check if Wall2 is already present (by checking the current wall type)
        if (wallUpgrader.HasWall() && wallUpgrader.currentWall.CompareTag("Wall2"))
        {
            Debug.Log("Wall upgrade skipped: Wall2 is already present.");
            return;
        }

        if (!moneyManager.SpendMoney(upgradeCost))
        {
            Debug.Log("Not enough money to upgrade the wall.");
            return;
        }

        wallUpgrader.UpgradeWall();
        // Reset health on the WallManager attached to the new wall
        var wallManager = wallUpgrader.GetComponentInChildren<WallManager>();
        if (wallManager != null)
        {
            wallManager.ResetHealth();
        }
        moneyManager.ObjectBought();
        Debug.Log($"Wall upgraded for {upgradeCost} coins.");
    }
    public void PlayUIAUIA()
    {
        if (uiaiuiai == null)
        {
            Debug.LogWarning("Action sound clip not assigned!");
            return;
        }

        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource not found!");
            return;
        }

        StartCoroutine(PlayUIAUIACor(uiaiuiai, 2f));
    }
    private IEnumerator PlayUIAUIACor(AudioClip clip, float duration)
    {
        audioSource.clip = clip;
        audioSource.Play();
        yield return new WaitForSecondsRealtime(duration);
        audioSource.Stop();
    }
}