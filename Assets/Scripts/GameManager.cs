using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI (optional)")]
    [SerializeField] private Text coinText;
    [SerializeField] private string coinFormat = "Coins: {0}";

    [Header("State")]
    private int _totalCoinsCollected = 0;

    public int Coins => _totalCoinsCollected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        UpdateUI();
    }

    public void AddCoin(int amount)
    {
        _totalCoinsCollected += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = string.Format(coinFormat, _totalCoinsCollected);
    }
}
