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
    }

    public List<SpawnablePrefab> spawnables = new List<SpawnablePrefab>();
    public Transform spawnLocation;
    public MoneyManager moneyManager;

    public void SpawnByID(int id)
    {
        var match = spawnables.Find(p => p.id == id);

        if (match == null || match.prefab == null)
        {
            Debug.LogWarning($"Prefab with ID {id} not found.");
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

        Instantiate(match.prefab, spawnLocation.position, spawnLocation.rotation);
        moneyManager.ObjectBought();
    }
}
