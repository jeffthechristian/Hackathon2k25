using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    [SerializeField]
    private int startingMoney = 100;

    public int CurrentMoney { get; private set; }

    [SerializeField]
    private TextMeshProUGUI moneyText;

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
}
