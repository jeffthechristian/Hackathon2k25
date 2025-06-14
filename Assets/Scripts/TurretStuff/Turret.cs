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
    private Transform currentTarget; // Store target for Animation Event

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
            dir.y = 0;
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);

            // Check if turret is aligned with enemy
            float angleToEnemy = Vector3.Angle(transform.forward, dir);
            if (fireCooldown <= 0f && angleToEnemy < 5f)
            {
                StartCoroutine(PlayShootAnimation(nearestEnemy));
                fireCooldown = fireRate;
            }
        }
        else
        {
            animator.Play("idle");
        }
    }

    IEnumerator PlayShootAnimation(Transform target)
    {
        isShooting = true;
        currentTarget = target; // Store target for Animation Event
        animator.Play("shoot");

        // Wait for animation to complete (or use state machine to detect end)
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        isShooting = false;
    }

    // Called by Animation Event
    void OnShootEvent()
    {
        if (currentTarget == null) return;

        if (audioSource != null && shootSFX != null && Random.value < 0.2f)
        {
            audioSource.pitch = Random.Range(0.5f, 1.5f);
            audioSource.PlayOneShot(shootSFX);
        }


        // Fire bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (currentTarget.position - firePoint.position).normalized;
            rb.linearVelocity = direction * shootPower;
        }
    }
}