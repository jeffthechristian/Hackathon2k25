using UnityEngine;
using UnityEngine.UI; // For UI health bar (optional)

public class WallManager : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    public GameObject[] wallSections; // Assign all wall sections in the Inspector
    public Slider healthBar; // Optional: Assign a UI Slider for health bar visualization

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
        gameObject.SetActive(false);
    }
}