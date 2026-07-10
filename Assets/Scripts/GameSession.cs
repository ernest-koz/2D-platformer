using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    GameOver,
    Finish
}

public class GameSession : MonoBehaviour
{
    [Header("UI - Coins")]
    [SerializeField] private Text _coinText;
    [SerializeField] private string _coinFormat = "Монеты: {0}";

    [Header("UI - Game Over")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Text _gameOverCoinText;
    [SerializeField] private Text _gameOverTimeText;
    [SerializeField] private Text _gameOverEnemyText;
    [SerializeField] private string _gameOverCoinFormat = "Монет собрано: {0}";
    [SerializeField] private string _gameOverTimeFormat = "Время: {0:F1} с";
    [SerializeField] private string _gameOverEnemyFormat = "Повержено врагов: {0}";

    [Header("UI - Finish")]
    [SerializeField] private GameObject _finishPanel;
    [SerializeField] private Text _finishCoinText;
    [SerializeField] private Text _finishTimeText;
    [SerializeField] private Text _finishEnemyText;
    [SerializeField] private string _finishCoinFormat = "Монет собрано: {0}";
    [SerializeField] private string _finishTimeFormat = "Время: {0:F1} с";
    [SerializeField] private string _finishEnemyFormat = "Повержено врагов: {0}";

    [Header("References")]
    [SerializeField] private PlayerInput _input;

    private GameState _state = GameState.Playing;
    private int _totalCoinsCollected = 0;
    private int _enemiesDefeated = 0;
    private float _playTime = 0f;
    private float _finishTime = 0f;

    public GameState State => _state;
    public int Coins => _totalCoinsCollected;
    public int EnemiesDefeated => _enemiesDefeated;
    public float PlayTime => _playTime;

    private void Awake()
    {
        SetGameState(GameState.Playing);
    }

    private void Update()
    {
        if (_input.RestartPressed && (_state == GameState.GameOver || _state == GameState.Finish))
        {
            RestartLevel();
            return;
        }

        if (_state != GameState.Playing)
        {
            return;
        }

        _playTime += Time.deltaTime;
    }

    public void AddCoin(int amount)
    {
        if (_state != GameState.Playing)
        {
            return;
        }

        _totalCoinsCollected += amount;
        UpdateCoinUI();
    }

    public void RegisterEnemyKill()
    {
        if (_state != GameState.Playing)
        {
            return;
        }

        _enemiesDefeated++;
    }

    public void GameOver()
    {
        if (_state != GameState.Playing)
        {
            return;
        }

        SetGameState(GameState.GameOver);

        if (_gameOverCoinText != null)
        {
            _gameOverCoinText.text = string.Format(_gameOverCoinFormat, _totalCoinsCollected);
        }

        if (_gameOverTimeText != null)
        {
            _gameOverTimeText.text = string.Format(_gameOverTimeFormat, _playTime);
        }

        if (_gameOverEnemyText != null)
        {
            _gameOverEnemyText.text = string.Format(_gameOverEnemyFormat, _enemiesDefeated);
        }

        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
        }
    }

    public void FinishLevel()
    {
        if (_state != GameState.Playing)
        {
            return;
        }

        _finishTime = _playTime;
        SetGameState(GameState.Finish);

        if (_finishCoinText != null)
        {
            _finishCoinText.text = string.Format(_finishCoinFormat, _totalCoinsCollected);
        }

        if (_finishTimeText != null)
        {
            _finishTimeText.text = string.Format(_finishTimeFormat, _finishTime);
        }

        if (_finishEnemyText != null)
        {
            _finishEnemyText.text = string.Format(_finishEnemyFormat, _enemiesDefeated);
        }

        if (_finishPanel != null)
        {
            _finishPanel.SetActive(true);
        }
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
        if (_coinText != null)
        {
            _coinText.text = string.Format(_coinFormat, _totalCoinsCollected);
        }
    }
}
