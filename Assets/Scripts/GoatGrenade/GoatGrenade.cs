using UnityEngine;
using System.Collections.Generic;
using Oculus.Interaction; // Meta XR Interaction SDK namespace

[System.Serializable]
public class GoatSound
{
    public AudioClip clip;
    public float baseDamage;
}

[RequireComponent(typeof(AudioSource), typeof(Rigidbody), typeof(Grabbable))]
public class GoatGrenade : MonoBehaviour
{
    public List<GoatSound> goatSounds;
    public AudioClip explosionSound;
    public float explosionRadius = 5f;
    public AnimationCurve damageFalloff = AnimationCurve.Linear(0, 1, 1, 0);
    public float throwThreshold = 1.5f; // Velocity threshold for a throw (m/s)
    public GameObject explosionParticlePrefab; // Reference to particle effect prefab

    private AudioSource audioSource;
    private Rigidbody rb;
    private Grabbable grabbable;
    private bool hasExploded = false;
    private GoatSound selectedGoat;
    private bool wasGrabbed;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        grabbable = GetComponent<Grabbable>();
        wasGrabbed = false;

        // Subscribe to pointer events
        grabbable.WhenPointerEventRaised += HandlePointerEvent;
    }

    void Update()
    {
        // Track grab state
        wasGrabbed = grabbable.SelectingPointsCount > 0;
    }

    void HandlePointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Unselect && wasGrabbed)
        {
            // Check velocity after a short delay to ensure throw velocity is applied
            StartCoroutine(CheckThrow());
        }
    }

    System.Collections.IEnumerator CheckThrow()
    {
        // Wait one frame to ensure ThrowWhenUnselected has applied velocity
        yield return new WaitForEndOfFrame();

        float velocityMagnitude = rb.linearVelocity.magnitude;
        if (velocityMagnitude > throwThreshold && goatSounds.Count > 0)
        {
            // Play goat sound on throw
            selectedGoat = goatSounds[Random.Range(0, goatSounds.Count)];
            audioSource.pitch = Random.Range(0.7f, 1.3f);
            audioSource.PlayOneShot(selectedGoat.clip);
            Debug.Log($"Goat grenade thrown! Velocity: {velocityMagnitude} m/s");
        }
        else
        {
            Debug.Log($"Goat grenade dropped. Velocity: {velocityMagnitude} m/s");
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

        // Spawn particle effect
        if (explosionParticlePrefab != null)
        {
            GameObject particleInstance = Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
            // Destroy particle after 3 seconds
            Destroy(particleInstance, 3f);
        }

        if (selectedGoat != null)
        {
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
                        Debug.Log($"GoatGrenade damaged {hit.name} for {finalDamage:F1} and applied slow");
                        enemy.TakeDamage(finalDamage);
                        Debug.Log($"GoatGrenade damaged {hit.name} for {finalDamage:F1}");
                    }
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

    void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        grabbable.WhenPointerEventRaised -= HandlePointerEvent;
    }
}