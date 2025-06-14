using UnityEngine;

public class Enemy : MonoBehaviour
{
    public MoneyManager moneyManager;
    public float health = 100f;

    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. Remaining Health: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
        moneyManager.AddMoney(10);
    }
}
