using UnityEngine;
using UnityEngine.UI;

public class GameSessionUI : MonoBehaviour
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
    [SerializeField] private GameSession _gameSession;

    private void OnEnable()
    {
        if (_gameSession == null)
        {
            Debug.LogError($"GameSession not assigned on {gameObject.name}", gameObject);
            return;
        }

        _gameSession.CoinChanged += OnCoinChanged;
        _gameSession.GameOverStarted += OnGameOver;
        _gameSession.LevelFinished += OnLevelFinished;
    }

    private void OnDisable()
    {
        if (_gameSession == null)
        {
            return;
        }

        _gameSession.CoinChanged -= OnCoinChanged;
        _gameSession.GameOverStarted -= OnGameOver;
        _gameSession.LevelFinished -= OnLevelFinished;
    }

    private void OnCoinChanged(int totalCoins)
    {
        SetText(_coinText, _coinFormat, totalCoins);
    }

    private void OnGameOver(SessionStats stats)
    {
        SetText(_gameOverCoinText, _gameOverCoinFormat, stats.TotalCoinsCollected);
        SetText(_gameOverTimeText, _gameOverTimeFormat, stats.PlayTime);
        SetText(_gameOverEnemyText, _gameOverEnemyFormat, stats.EnemiesDefeated);
        ShowPanel(_gameOverPanel);
    }

    private void OnLevelFinished(SessionStats stats)
    {
        SetText(_finishCoinText, _finishCoinFormat, stats.TotalCoinsCollected);
        SetText(_finishTimeText, _finishTimeFormat, stats.PlayTime);
        SetText(_finishEnemyText, _finishEnemyFormat, stats.EnemiesDefeated);
        ShowPanel(_finishPanel);
    }

    private static void SetText(Text text, string format, object value)
    {
        if (text != null)
        {
            text.text = string.Format(format, value);
        }
    }

    private static void ShowPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }
}
