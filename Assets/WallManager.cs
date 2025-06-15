using UnityEngine;
using UnityEngine.UI;

public class WallManager : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    public Slider healthBar; // Optional: Assign a UI Slider for health bar visualization
    private WallUpgrader wallUpgrader; // Reference to WallUpgrader

    void Awake()
    {
        wallUpgrader = FindObjectOfType<WallUpgrader>();
        if (wallUpgrader == null)
        {
            Debug.LogError("WallManager: WallUpgrader not found in scene!");
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"Wall took {damage} damage. Remaining Health: {currentHealth}");
        UpdateHealthBar();

        if (currentHealth <= 0f)
        {
            DestroyWall();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
    }

    void DestroyWall()
    {
        if (wallUpgrader != null)
        {
            wallUpgrader.DestroyCurrentWall();
        }
        currentHealth = maxHealth; // Reset health for next wall
        UpdateHealthBar();
    }

    // Method to reset health when a new wall is spawned
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }
}