using UnityEngine;
using TMPro;
using System.Collections;

public class MoneyManager : MonoBehaviour
{
    [SerializeField] private int startingMoney = 100;
    public int CurrentMoney { get; private set; }

    [SerializeField] private TextMeshProUGUI moneyText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gainMoney;
    [SerializeField] private AudioClip notEnough;
    [SerializeField] private AudioClip itemBought;

    private Color originalColor;
    private Vector3 originalPosition;
    private Coroutine colorCoroutine;

    private void Start()
    {
        CurrentMoney = startingMoney;

        if (moneyText != null)
        {
            originalColor = moneyText.color;
            originalPosition = moneyText.rectTransform.localPosition;
        }

        UpdateMoneyUI();
    }

    public bool SpendMoney(int amount)
    {
        if (amount > CurrentMoney)
        {
            Debug.LogWarning("Attempted to spend more than available.");
            PlaySound(notEnough);
            ShakeMoneyText();
            FadeMoneyTextColor(Color.red);
            return false;
        }

        CurrentMoney -= amount;
        UpdateMoneyUI();
        PlaySound(itemBought);
        FadeMoneyTextColor(Color.red);
        return true;
    }

    public void AddMoney(int amount)
    {
        CurrentMoney += amount;
        UpdateMoneyUI();
        PlaySound(gainMoney);
        FadeMoneyTextColor(Color.green);
    }

    private void UpdateMoneyUI()
    {
        if (moneyText)
        {
            moneyText.text = $"{CurrentMoney}";
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void FadeMoneyTextColor(Color targetColor)
    {
        if (colorCoroutine != null)
        {
            StopCoroutine(colorCoroutine);
        }
        colorCoroutine = StartCoroutine(FadeColorCoroutine(targetColor, 0.5f));
    }

    private IEnumerator FadeColorCoroutine(Color targetColor, float duration)
    {
        if (moneyText == null) yield break;

        moneyText.color = targetColor;

        float t = 0;
        while (t < duration)
        {
            moneyText.color = Color.Lerp(targetColor, originalColor, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        moneyText.color = originalColor;
    }

    private void ShakeMoneyText()
    {
        if (moneyText == null) return;
        StartCoroutine(ShakeCoroutine(0.3f, 5f));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        var rt = moneyText.rectTransform;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            rt.localPosition = originalPosition + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rt.localPosition = originalPosition;
        moneyText.color = originalColor;
    }

    public void ObjectBought()
    {
        PlaySound(itemBought);
    }
}
