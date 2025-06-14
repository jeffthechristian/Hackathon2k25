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

    void Start()
    {
        GameObject headObj = GameObject.FindGameObjectWithTag("Head");
        if (headObj != null)
            headTransform = headObj.transform;
        else
            Debug.LogError("No object with tag 'Head' found!");

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (isInMouth && !isVomiting)
        {
            drinkingTime += Time.deltaTime;

            if (!audioSource.isPlaying)
            {
                audioSource.clip = drinkingSound;
                audioSource.loop = true;
                audioSource.Play();
            }

            if (drinkingTime >= drinkDurationToVomit)
            {
                StartCoroutine(VomitRoutine());
            }
        }
        else
        {
            if (audioSource.isPlaying && audioSource.clip == drinkingSound)
                audioSource.Stop();
        }
    }

    private System.Collections.IEnumerator VomitRoutine()
    {
        isVomiting = true;
        audioSource.Stop();
        audioSource.PlayOneShot(vomitingSound);

        float elapsed = 0f;
        while (elapsed < vomitDuration)
        {
            SpawnVomitProjectile();
            elapsed += vomitSpawnInterval;
            yield return new WaitForSeconds(vomitSpawnInterval);
        }

        Destroy(gameObject); // Beer is consumed
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

        Destroy(vomit, 2f); // Auto destroy after 2 seconds
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
}
