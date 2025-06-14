using UnityEngine;
using System.Collections;
using Oculus.Interaction;

public class TurretDeployer : MonoBehaviour
{
    public Vector3 deployedScale = Vector3.one; // Set this in the Inspector
    private bool deployed = false;
    public AudioClip deploySFX;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!deployed && collision.gameObject.CompareTag("Ground"))
        {
            deployed = true;
            Debug.Log("Turret deployed!");

            // Disable grabbing after deployment
            var grab = GetComponent<Grabbable>();
            if (grab != null) grab.enabled = false;

            // Freeze physics
            var rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            // Snap turret upright — keep Y rotation, but reset X and Z
            transform.up = Vector3.up;
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

            // Start scaling coroutine
            StartCoroutine(DeployAndScale());
        }
    }

    IEnumerator DeployAndScale()
    {
        Vector3 initialScale = transform.localScale;
        float duration = 1f;
        float time = 0f;
        if (audioSource != null && deploySFX != null)
        {
            audioSource.pitch = Random.Range(0.6f, 1.4f);
            audioSource.PlayOneShot(deploySFX);
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            transform.localScale = Vector3.Lerp(initialScale, deployedScale, t);
            yield return null;
        }

        transform.localScale = deployedScale;

        // Activate turret after scaling
        var turret = GetComponent<Turret>();
        if (turret != null)
        {
            turret.Activate();
            Debug.Log("Turret script activated.");
        }
    }
}
