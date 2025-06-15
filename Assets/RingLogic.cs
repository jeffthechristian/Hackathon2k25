using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RingLogic : MonoBehaviour
{
    [SerializeField] private float maxHealth = 500f;
    private float currentHealth;
    public TextMeshPro health;

    private void Start()
    {
        currentHealth = maxHealth;
        health.text = currentHealth.ToString();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"Ring took {amount} damage. Remaining Health: {currentHealth}");
        health.text = currentHealth.ToString();
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        SceneManager.LoadScene("Lost");
    }
}
