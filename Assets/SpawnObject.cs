using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    public GameObject objectToSpawn;
    public Transform spawnLocation;

    public void SpawnObjects()
    {
        if (objectToSpawn && spawnLocation)
        {
            Instantiate(objectToSpawn, spawnLocation.position, spawnLocation.rotation);
        }
    }
}
