using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GoatSound
{
    public AudioClip clip;
    public float baseDamage;
}

[RequireComponent(typeof(AudioSource), typeof(Rigidbody))]
public class GoatGrenade : MonoBehaviour
{
    public List<GoatSound> goatSounds;
    public AudioClip explosionSound;
    public float explosionRadius = 5f;
    public AnimationCurve damageFalloff = AnimationCurve.Linear(0, 1, 1, 0);

    private AudioSource audioSource;
    private Rigidbody rb;
    private bool hasExploded = false;
    private GoatSound selectedGoat;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        // Play goat sound on throw
        if (goatSounds.Count > 0)
        {
            selectedGoat = goatSounds[Random.Range(0, goatSounds.Count)];
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(selectedGoat.clip);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

        if (collision.collider.CompareTag("Ground") || collision.collider.CompareTag("Enemy"))
        {
            hasExploded = true;
            Explode();
        }
    }

    void Explode()
    {
        // Play explosion sound
        if (explosionSound != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(explosionSound);
        }

        Debug.Log("Goat grenade exploded with base damage: " + selectedGoat.baseDamage);

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float dist = Vector3.Distance(hit.transform.position, transform.position);
                float t = Mathf.Clamp01(dist / explosionRadius);
                float falloff = damageFalloff.Evaluate(t);
                float finalDamage = selectedGoat.baseDamage * falloff;

                var enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.ApplySlow(1f, 5f);
                    enemy.TakeDamage(finalDamage);
                    Debug.Log($"GoatGrenade damaged {hit.name} for {finalDamage:F1}");
                }
            }
        }

        // Hide model and destroy after explosion sound finishes
        StartCoroutine(DestroyAfterSound());
    }

    System.Collections.IEnumerator DestroyAfterSound()
    {
        // Hide mesh
        HideGrenade(gameObject);
        float delay = explosionSound ? explosionSound.length : 0.5f;
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    void HideGrenade(GameObject obj)
    {
        foreach (var renderer in obj.GetComponents<Renderer>())
            renderer.enabled = false;

        foreach (Transform child in obj.transform)
            HideGrenade(child.gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
