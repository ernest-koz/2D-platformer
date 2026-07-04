using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    GameOver,
    Finish
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI - Coins")]
    [SerializeField] private Text coinText;
    [SerializeField] private string coinFormat = "Coins: {0}";

    [Header("UI - Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverCoinText;
    [SerializeField] private Text gameOverTimeText;
    [SerializeField] private string gameOverCoinFormat = "Coins: {0}";
    [SerializeField] private string gameOverTimeFormat = "Time: {0:F1}s";

    [Header("UI - Finish")]
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private Text finishCoinText;
    [SerializeField] private Text finishTimeText;
    [SerializeField] private string finishCoinFormat = "Coins collected: {0}";
    [SerializeField] private string finishTimeFormat = "Time: {0:F1}s";

    private GameState _state = GameState.Playing;
    private int _totalCoinsCollected = 0;
    private float _playTime = 0f;
    private float _finishTime = 0f;

    public GameState State => _state;
    public int Coins => _totalCoinsCollected;
    public float PlayTime => _playTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SetGameState(GameState.Playing);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && (_state == GameState.GameOver || _state == GameState.Finish))
        {
            RestartLevel();
            return;
        }

        if (_state != GameState.Playing) return;
        _playTime += Time.deltaTime;
    }

    public void AddCoin(int amount)
    {
        if (_state != GameState.Playing) return;
        _totalCoinsCollected += amount;
        UpdateCoinUI();
    }

    public void GameOver()
    {
        if (_state != GameState.Playing) return;
        SetGameState(GameState.GameOver);

        if (gameOverCoinText != null)
            gameOverCoinText.text = string.Format(gameOverCoinFormat, _totalCoinsCollected);
        if (gameOverTimeText != null)
            gameOverTimeText.text = string.Format(gameOverTimeFormat, _playTime);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void FinishLevel()
    {
        if (_state != GameState.Playing) return;
        _finishTime = _playTime;
        SetGameState(GameState.Finish);

        if (finishCoinText != null)
            finishCoinText.text = string.Format(finishCoinFormat, _totalCoinsCollected);
        if (finishTimeText != null)
            finishTimeText.text = string.Format(finishTimeFormat, _finishTime);

        if (finishPanel != null)
            finishPanel.SetActive(true);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SetGameState(GameState newState)
    {
        _state = newState;
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = string.Format(coinFormat, _totalCoinsCollected);
    }
}
