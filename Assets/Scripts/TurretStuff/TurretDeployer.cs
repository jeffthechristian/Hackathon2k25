using UnityEngine;


public class TurretDeployer : MonoBehaviour
{
    private bool deployed = false;

    void OnCollisionEnter(Collision collision)
    {
        if (!deployed && collision.gameObject.CompareTag("Ground"))
        {
            deployed = true;
            Debug.Log("Turret deployed!");

            // Disable grabbing after deployment
            var grab = GetComponent<OVRGrabbable>();
            if (grab != null)
            {
                grab.enabled = false;
            }

            // Freeze physical movement yea in the engine yea
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            // Moving ze turret upright (Y-axis up)
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            var turret = GetComponent<Turret>();
            if (turret != null)
            {
                turret.Activate();
                Debug.Log("Turret script activated.");
            }
        }
    }
}
