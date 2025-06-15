using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UltimateCharge : MonoBehaviour
{
    public float chargeDuration = 120f; // 2 minutes
    public GameObject ultimateObject;   // Object to activate when charged
    public TextMeshProUGUI statusText;  // UI Text to show status
    public AudioClip chargedSound;      // New: Sound when fully charged

    private float currentChargeTime = 0f;
    private bool isCharged = false;
    private bool hasPlayedChargedSound = false;
    private AudioSource audioSource;
    public PigeonAirstrike airstrike;

    public Transform targetLoc;

    void Start()
    {
        if (ultimateObject != null)
            ultimateObject.SetActive(false);

        if (statusText != null)
            statusText.text = "Pidgeons Asleep";


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
                statusText.text = "Pidgeons Asleep";
        }

        if (isCharged && !hasPlayedChargedSound)
        {
            if (ultimateObject != null)
                ultimateObject.SetActive(true);

            if (statusText != null)
                statusText.text = "Launch Pidegons";

            PlaySound(chargedSound);
            hasPlayedChargedSound = true;
        }
    }

    public void OnUltimatePressed()
    {
        if (!isCharged)
        {
            return;
        }

        if (ultimateObject != null)
            ultimateObject.SetActive(false);

        // Reset the charge
        isCharged = false;
        currentChargeTime = 0f;
        hasPlayedChargedSound = false;

        if (statusText != null)
            statusText.text = "Not Charged";

        airstrike.LaunchAirstrike(targetLoc);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.clip = chargedSound;
            audioSource.Play();
    }
}
