using UnityEngine;
using System.Collections;

public class PigeonAirstrike : MonoBehaviour
{
    [Header("Pigeon Settings")]
    public GameObject pigeonPrefab;
    public AudioClip pigeonFlyingSound;

    public AudioClip explosionSound;
    public float approachDistance = 100f;
    public float approachHeight = 15f;
    public float approachSpeed = 20f;
    public float diveSpeed = 30f;
    public float diveTriggerDistance = 10f;

    [Header("Explosion Settings")]
    public GameObject explosionEffect;
    public float explosionRadius = 30f;

    [Header("Target")]
    public Transform targetObject;

    void Start()
    {
        if (targetObject != null)
        {
            LaunchAirstrike(targetObject);
        }
    }

    public void LaunchAirstrike(Transform target)
    {
        Vector3 toTarget = target.position - transform.position;
        Vector3 spawnDir = -toTarget.normalized;

        Vector3 spawnPos = target.position + spawnDir * approachDistance;
        spawnPos.y = approachHeight;

        GameObject pigeon = Instantiate(pigeonPrefab, spawnPos, Quaternion.identity);

        if (pigeonFlyingSound != null)
        {
            AudioSource audioSource = pigeon.AddComponent<AudioSource>();
            audioSource.clip = pigeonFlyingSound;
            audioSource.Play();
        }

        StartCoroutine(PigeonApproachAndDive(pigeon, target));
    }


    private IEnumerator PigeonApproachAndDive(GameObject pigeon, Transform target)
    {
        bool diving = false;

        while (pigeon != null)
        {
            Vector3 targetPos = target.position;
            float distanceToTarget = Vector3.Distance(pigeon.transform.position, targetPos);

            if (!diving && distanceToTarget <= diveTriggerDistance)
            {
                diving = true;
            }

            Vector3 moveDirection;
            float speed;

            if (diving)
            {
                moveDirection = (targetPos - pigeon.transform.position).normalized;
                speed = diveSpeed;
            }
            else
            {
                Vector3 flatTarget = new Vector3(targetPos.x, approachHeight, targetPos.z);
                moveDirection = (flatTarget - pigeon.transform.position).normalized;
                speed = approachSpeed;
            }

            pigeon.transform.position += moveDirection * speed * Time.deltaTime;
            pigeon.transform.rotation = Quaternion.LookRotation(moveDirection);

            if (diving && distanceToTarget < 0.5f)
            {
                StartCoroutine(Explode(pigeon, pigeon.transform.position));
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator Explode(GameObject pigeon, Vector3 position)
    {
        if (explosionEffect != null)
            Instantiate(explosionEffect, position, Quaternion.identity);

        // Play explosion sound
        AudioSource audioSource = pigeon.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = pigeon.AddComponent<AudioSource>();

        if (explosionSound != null)
        {
            audioSource.clip = explosionSound;
            audioSource.loop = false;
            audioSource.Play();
        }

        // Hide pigeon visuals immediately
        HideObjRenderers(pigeon);

        // Damage enemies in radius
        Collider[] affected = Physics.OverlapSphere(position, explosionRadius);
        foreach (var col in affected)
        {
            if (col.CompareTag("Enemy"))
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(200f);
                }
            }
        }

        // Wait for sound to finish playing
        if (audioSource.clip != null)
            yield return new WaitForSeconds(audioSource.clip.length);

        Destroy(pigeon);
    }


    void HideObjRenderers(GameObject obj)
    {
        foreach (var renderer in obj.GetComponents<Renderer>())
        {
            renderer.enabled = false;
        }
        foreach (Transform child in obj.transform)
        {
            HideObjRenderers(child.gameObject);
        }
    }


}
