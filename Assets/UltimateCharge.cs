using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UltimateCharge : MonoBehaviour
{
    public float chargeDuration = 120f; // 2 minutes
    public GameObject ultimateObject;   // Object to activate when charged
    public TextMeshProUGUI statusText;  // UI Text to show status
    public AudioClip successSound;
    public AudioClip failSound;
    public AudioClip chargedSound;      // New: Sound when fully charged

    private float currentChargeTime = 0f;
    private bool isCharged = false;
    private bool hasPlayedChargedSound = false;
    private AudioSource audioSource;

    void Start()
    {
        if (ultimateObject != null)
            ultimateObject.SetActive(false);

        if (statusText != null)
            statusText.text = "Not Charged";


        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (!isCharged)
        {
            currentChargeTime += Time.deltaTime;

            if (currentChargeTime >= chargeDuration)
            {
                isCharged = true;
                hasPlayedChargedSound = false; // Reset flag just in case
            }

            if (statusText != null)
                statusText.text = "Not Charged";
        }

        if (isCharged && !hasPlayedChargedSound)
        {
            if (ultimateObject != null)
                ultimateObject.SetActive(true);

            if (statusText != null)
                statusText.text = "Launch Attack";

            PlaySound(chargedSound);
            hasPlayedChargedSound = true;
        }
    }

    void OnUltimatePressed()
    {
        if (!isCharged)
        {
            PlaySound(failSound);
            return;
        }

        // Successful activation
        PlaySound(successSound);

        if (ultimateObject != null)
            ultimateObject.SetActive(false);

        // Reset the charge
        isCharged = false;
        currentChargeTime = 0f;
        hasPlayedChargedSound = false;

        if (statusText != null)
            statusText.text = "Not Charged";
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
