using UnityEngine;
using Oculus.Interaction; // Meta XR SDK

[RequireComponent(typeof(AudioSource), typeof(Rigidbody), typeof(Grabbable))]
public class SixPack : MonoBehaviour
{
    public float attractRadius = 10f;
    public float attractDuration = 5f;
    public float throwThreshold = 1.5f;
    public AudioClip distractionSound;
    public AudioClip flyingSound;
    public AudioClip landingSound;
    public GameObject visualEffectPrefab;

    private AudioSource audioSource;
    private Rigidbody rb;
    private Grabbable grabbable;
    private bool wasGrabbed;
    private bool hasLanded = false;
    private bool isActivated = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        grabbable = GetComponent<Grabbable>();

        grabbable.WhenPointerEventRaised += HandlePointerEvent;
    }

    void Update()
    {
        wasGrabbed = grabbable.SelectingPointsCount > 0;
    }

    void HandlePointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Unselect && wasGrabbed)
        {
            StartCoroutine(CheckThrow());
        }
    }

    System.Collections.IEnumerator CheckThrow()
    {
        yield return new WaitForEndOfFrame();

        if (rb.linearVelocity.magnitude > throwThreshold && flyingSound != null)
        {
            audioSource.clip = flyingSound;
            audioSource.loop = true;
            audioSource.pitch = Random.Range(0.85f, 1.15f);
            audioSource.Play();
            Debug.Log("SixPack thrown with flying sound");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasLanded) return;
        if (!collision.collider.CompareTag("Ground")) return;

        hasLanded = true;

        // Stop flying loop
        audioSource.Stop();

        // Play landing sound
        if (landingSound)
        {
            audioSource.PlayOneShot(landingSound);
        }

        StartCoroutine(DelayedActivation());
    }

    System.Collections.IEnumerator DelayedActivation()
    {
        yield return new WaitForSeconds(0.2f); // allow landing sound to play
        ActivateDistraction();
    }

    void ActivateDistraction()
    {
        if (isActivated) return;
        isActivated = true;

        rb.isKinematic = true;
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

        // Play distraction sound
        if (distractionSound)
        {
            audioSource.PlayOneShot(distractionSound);
        }

        // Visual effect
        if (visualEffectPrefab)
        {
            GameObject fx = Instantiate(visualEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, attractDuration);
        }

        // Attract up to 6 nearby enemies
        Collider[] hits = Physics.OverlapSphere(transform.position, attractRadius);
        int attractedCount = 0;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                var ai = hit.GetComponent<Enemy>();
                if (ai != null)
                {
                    ai.AttractTo(transform.position, attractDuration);
                    attractedCount++;

                    if (attractedCount >= 6)
                        break;
                }
            }
        }

        Destroy(gameObject, attractDuration);
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0.2f, 0.3f);
        Gizmos.DrawSphere(transform.position, attractRadius);
    }

    void OnDestroy()
    {
        grabbable.WhenPointerEventRaised -= HandlePointerEvent;
    }
}
