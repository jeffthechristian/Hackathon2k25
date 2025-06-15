using UnityEngine;

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