using UnityEngine;

public class BeerDrinking : MonoBehaviour
{
    public AudioClip drinkingSound;
    public AudioClip vomitingSound;
    public GameObject vomitPrefab;

    public float drinkDurationToVomit = 2f;
    public float vomitDuration = 2f;
    public float vomitSpawnInterval = 0.2f;
    public float vomitProjectileForce = 5f;

    private bool isInMouth = false;
    private float drinkingTime = 0f;
    private bool isVomiting = false;

    private AudioSource audioSource;
    private Transform headTransform;
    private ParticleSystem vomitParticles;

    private Coroutine vomitCoroutine;

    void Start()
    {
        GameObject headObj = GameObject.FindGameObjectWithTag("Head");
        if (headObj != null)
        {
            headTransform = headObj.transform;
            vomitParticles = headTransform.GetComponentInChildren<ParticleSystem>();
        }
        else
        {
            Debug.LogError("No object with tag 'Head' found!");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (isInMouth && !isVomiting)
        {
            drinkingTime += Time.deltaTime;

            if (!audioSource.isPlaying || audioSource.clip != drinkingSound)
            {
                audioSource.clip = drinkingSound;
                audioSource.loop = true;
                audioSource.Play();
            }

            if (drinkingTime >= drinkDurationToVomit)
            {
                if (vomitCoroutine == null)
                    vomitCoroutine = StartCoroutine(VomitRoutine());
            }
        }
        else if (!isVomiting && audioSource.isPlaying && audioSource.clip == drinkingSound)
        {
            audioSource.Stop();
        }
    }

    private System.Collections.IEnumerator VomitRoutine()
    {
        isVomiting = true;

        // Stop drinking sound
        audioSource.Stop();

        // Start vomit sound (looped)
        audioSource.clip = vomitingSound;
        audioSource.loop = true;
        audioSource.Play();

        // Start particles
        if (vomitParticles != null)
            vomitParticles.Play();

        float elapsed = 0f;
        while (elapsed < vomitDuration)
        {
            SpawnVomitProjectile();
            elapsed += vomitSpawnInterval;
            yield return new WaitForSeconds(vomitSpawnInterval);
        }

        // Cleanup and stop everything properly
        CleanupVomit();

        Destroy(gameObject); // Beer consumed
    }

    private void CleanupVomit()
    {
        if (audioSource != null)
            audioSource.Stop();

        if (vomitParticles != null && vomitParticles.isPlaying)
            vomitParticles.Stop();

        isVomiting = false;
        vomitCoroutine = null;
    }

    private void SpawnVomitProjectile()
    {
        if (vomitPrefab == null || headTransform == null) return;

        GameObject vomit = Instantiate(vomitPrefab, headTransform.position, Quaternion.identity);
        Rigidbody rb = vomit.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 randomDir = (headTransform.forward + Random.insideUnitSphere * 0.3f).normalized;
            rb.AddForce(randomDir * vomitProjectileForce, ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Head"))
        {
            isInMouth = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Head"))
        {
            isInMouth = false;
            drinkingTime = 0f;
        }
    }

    private void OnDestroy()
    {
        CleanupVomit(); // Ensure things are stopped if object is destroyed mid-vomit
    }
}
