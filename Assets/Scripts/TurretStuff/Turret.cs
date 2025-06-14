using System.Collections;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public float shootPower = 15f;
    public float range = 10f;
    public float fireRate = 1f;
    public GameObject bulletPrefab;
    public Transform firePoint;
    private Animator animator;
    public AudioClip shootSFX;

    private AudioSource audioSource;
    private bool isActive = false;
    private float fireCooldown;
    private bool isShooting = false;

    public void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public void Activate()
    {
        isActive = true;
    }

    void Update()
    {
        if (!isActive || isShooting) return;

        fireCooldown -= Time.deltaTime;

        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        Transform nearestEnemy = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestEnemy = hit.transform;
                }
            }
        }

        if (nearestEnemy != null)
        {
            Vector3 dir = (nearestEnemy.position - transform.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);

            if (fireCooldown <= 0f)
            {
                StartCoroutine(DelayedShoot(nearestEnemy));
                fireCooldown = fireRate;
            }
        }
        else
        {
            animator.Play("idle"); // Play idle if no enemies
        }
    }


    IEnumerator DelayedShoot(Transform target)
    {
        isShooting = true;

        float hammerDuration = 0.2f;
        animator.Play("shoot");

        yield return new WaitForSeconds(hammerDuration);

        // Play shoot sound
        if (audioSource != null && shootSFX != null)
        {
            audioSource.pitch = Random.Range(0.5f, 1.5f);
            audioSource.PlayOneShot(shootSFX);
        }

        // Fire bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (target.position - firePoint.position).normalized;
            rb.linearVelocity = direction * shootPower;
        }

        isShooting = false;
    }


}
