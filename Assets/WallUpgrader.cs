using UnityEngine;

public class WallUpgrader : MonoBehaviour
{
    public GameObject wall1Prefab; // Prefab for Wall1
    public GameObject wall2Prefab; // Prefab for Wall2
    public Transform wallSpawnPoint; // Where to spawn the walls
    public GameObject currentWall; // Track the currently instantiated wall


    private void OnEnable()
    {
        SpawnWall1();
    }
    public void UpgradeWall()
    {
        if (wall1Prefab == null || wall2Prefab == null || wallSpawnPoint == null)
        {
            Debug.LogError("WallUpgrader: Wall1Prefab, Wall2Prefab, or WallSpawnPoint is not assigned!");
            return;
        }

        // Check if the current wall is Wall1 (by checking its tag)
        if (currentWall != null && currentWall.CompareTag("Wall1"))
        {
            Destroy(currentWall);
            currentWall = null;
            Debug.Log("Wall1 destroyed for upgrade");
        }
        else if (currentWall != null)
        {
            Debug.Log("Upgrade skipped: Current wall is not Wall1");
            return;
        }

        // Instantiate Wall2
        currentWall = Instantiate(wall2Prefab, wallSpawnPoint.position, wallSpawnPoint.rotation);
        Debug.Log("Wall upgraded to Wall2");
    }

    public void SpawnWall1()
    {
        if (wall1Prefab == null || wallSpawnPoint == null)
        {
            Debug.LogError("WallUpgrader: Wall1Prefab or WallSpawnPoint is not assigned!");
            return;
        }

        // If there's an existing wall, don't spawn a new one
        if (currentWall != null)
        {
            Debug.Log("Wall1 not spawned: A wall is already present");
            return;
        }

        // Instantiate Wall1
        currentWall = Instantiate(wall1Prefab, wallSpawnPoint.position, wallSpawnPoint.rotation);
        Debug.Log("Wall1 spawned");
    }

    public void DestroyCurrentWall()
    {
        if (currentWall != null)
        {
            Destroy(currentWall);
            currentWall = null;
            Debug.Log("Current wall destroyed");
        }
    }

    // Helper method to check if a wall is present
    public bool HasWall()
    {
        return currentWall != null;
    }
}