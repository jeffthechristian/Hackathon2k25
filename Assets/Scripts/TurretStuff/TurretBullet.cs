using UnityEngine;
using UnityEngine.Audio;

public class TurretBullet : MonoBehaviour
{
    public float damage = 10f;
    private AudioSource audioSource;
    public AudioClip shootSFX;

    public void Start()
    {
        if (audioSource != null && shootSFX != null)
        {
            audioSource.pitch = Random.Range(0.6f, 1.4f);
            audioSource.PlayOneShot(shootSFX);
        }
    }
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
