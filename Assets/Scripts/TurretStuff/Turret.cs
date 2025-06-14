using UnityEngine;

public class Turret : MonoBehaviour
{
    public float shootPower = 15f;
    public float range = 10f;
    public float fireRate = 1f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    private bool isActive = false;
    private float fireCooldown;

    public void Activate()
    {
        isActive = true;
    }

    void Update()
    {
        if (!isActive) return;

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
            // Rotate towards ze enemiez
            Vector3 dir = (nearestEnemy.position - transform.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
            if (fireCooldown <= 0f)
            {
                Shoot(nearestEnemy);
                fireCooldown = fireRate;
            }
        }
    }

    void Shoot(Transform target)
    {
        //Ze bullet appear and fly in ze diretction
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (target.position - firePoint.position).normalized;
            rb.linearVelocity = direction * shootPower;
        }
    }
}
