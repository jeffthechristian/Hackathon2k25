using UnityEngine;

public class WallSection : MonoBehaviour
{
    private WallManager wallManager;

    void Start()
    {
        // Find the WallManager in the parent or scene
        wallManager = GetComponentInParent<WallManager>() ?? FindObjectOfType<WallManager>();
        if (wallManager == null)
        {
            Debug.LogError("WallSection: No WallManager found!");
        }
    }

    public void TakeDamage(float damage)
    {
        if (wallManager != null)
        {
            wallManager.TakeDamage(damage);
        }
    }
}