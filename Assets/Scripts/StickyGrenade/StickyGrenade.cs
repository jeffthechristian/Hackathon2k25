using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class FartSound
{
    public AudioClip clip;
    public float baseDamage;
}

public class StickyGrenade : MonoBehaviour
{
    public float fuseTime = 2f;
    public float explosionRadius = 5f;
    public AnimationCurve damageFalloff = AnimationCurve.Linear(0, 1, 1, 0);
    public List<FartSound> fartSounds = new List<FartSound>();

    public AudioClip splatterSound;
    private AudioSource audioSource;

    private Rigidbody rb;
    private bool isStuck = false;
    private Transform stuckTo;

    private FartSound selectedFart;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isStuck) return;
        if (!collision.collider.CompareTag("Ground") && !collision.collider.CompareTag("Enemy"))
        {
            Debug.Log("Ignored collision with: " + collision.collider.name);
            return;
        }

        isStuck = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Play splat
        if (splatterSound)
        {
            audioSource.pitch = Random.Range(0.85f, 1.15f);
            audioSource.PlayOneShot(splatterSound);

        }


        // Start fuse
        StartCoroutine(FuseTimer());
    }

    IEnumerator FuseTimer()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    void Explode()
    {
        // Pick a fart
        if (fartSounds.Count > 0)
        {
            selectedFart = fartSounds[Random.Range(0, fartSounds.Count)];
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(selectedFart.clip);
        }
        Debug.Log("Grenade exploded with base damage: " + selectedFart.baseDamage);

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float distance = Vector3.Distance(hit.transform.position, transform.position);
                float t = Mathf.Clamp01(distance / explosionRadius);
                float falloff = damageFalloff.Evaluate(t);
                float finalDamage = selectedFart.baseDamage * falloff;

                var enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(finalDamage);
                    Debug.Log($"Damaged {hit.name} for {finalDamage:F1}");
                }
            }
            //Hide grenade, destroy after sound ends. ver smart yes?
            HideGrenade(gameObject);
            Destroy(gameObject, selectedFart.clip.length);
        }

        void HideGrenade(GameObject obj)
        {
            foreach (var renderer in obj.GetComponents<Renderer>())
            {
                renderer.enabled = false;
            }
            foreach (Transform child in obj.transform) HideGrenade(child.gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 0.5f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, explosionRadius);
        }
    }
}
