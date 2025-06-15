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
    private Transform currentTarget;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        // Validate firePoint
        if (firePoint == null)
            Debug.LogError("FirePoint is not assigned!", this);
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
            // Allow vertical aiming (remove dir.y = 0)
            Vector3 dir = (nearestEnemy.position - transform.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);

            // Debug: Visualize aiming direction
            Debug.DrawRay(firePoint.position, firePoint.forward * range, Color.red, 0.1f);

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
        currentTarget = target;
        animator.Play("shoot");

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        isShooting = false;
    }

    void OnShootEvent()
    {
        if (currentTarget == null) return;

        if (audioSource != null && shootSFX != null && Random.value < 0.2f)
        {
            audioSource.pitch = Random.Range(0.5f, 1.5f);
            audioSource.PlayOneShot(shootSFX);
        }

        // Instantiate bullet at firePoint
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Use firePoint's forward direction for bullet trajectory
            rb.useGravity = false; // Disable gravity for straight shots
            rb.linearVelocity = firePoint.forward * shootPower;

            // Debug: Visualize bullet trajectory
            Debug.DrawRay(firePoint.position, firePoint.forward * range, Color.green, 2f);
        }
        else
        {
            Debug.LogWarning("Bullet prefab is missing Rigidbody component!", bullet);
        }
    }

    // Optional: Visualize range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}