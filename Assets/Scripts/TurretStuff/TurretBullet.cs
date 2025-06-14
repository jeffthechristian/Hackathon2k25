using UnityEngine;

public class TurretBullet : MonoBehaviour
{
    public float damage = 10f;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            var enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Enemy damaged: " + damage);
            }
        }
        Destroy(gameObject);
    }
}
