using UnityEngine;
using UnityEngine.SceneManagement;

public class RingLogic : MonoBehaviour
{
    [SerializeField] private float maxHealth = 500f;
    private float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"Ring took {amount} damage. Remaining Health: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        SceneManager.LoadScene("Restart");
    }
}
