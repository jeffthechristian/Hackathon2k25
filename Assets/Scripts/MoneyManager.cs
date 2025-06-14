using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    [SerializeField]
    private int startingMoney = 100;

    public int CurrentMoney { get; private set; }

    [SerializeField]
    private TextMeshProUGUI moneyText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gainMoney;
    [SerializeField] private AudioClip notEnough;
    [SerializeField] private AudioClip itemBought;

    private void Start()
    {
        CurrentMoney = startingMoney;
        UpdateMoneyUI();
    }

    public bool SpendMoney(int amount)
    {
        if (amount > CurrentMoney)
        {
            Debug.LogWarning("Attempted to spend more than available.");
            PlaySound(notEnough);
            return false;
        }

        CurrentMoney -= amount;
        UpdateMoneyUI();
        return true;
    }

    public void AddMoney(int amount)
    {
        CurrentMoney += amount;
        UpdateMoneyUI();
        PlaySound(gainMoney);
    }

    private void UpdateMoneyUI()
    {
        if (moneyText)
        {
            moneyText.text = $"Money: ${CurrentMoney}";
        }
        else
        {
            Debug.LogWarning("Money Text UI not assigned.");
        }
    }

    public void ObjectBought()
    {
        PlaySound(itemBought);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
