using UnityEngine;
using UnityEngine.UI;

public class GameSessionUI : MonoBehaviour
{
    [Header("UI - Coins (HUD)")]
    [SerializeField] private Text _coinText;
    [SerializeField] private string _coinFormat = "Монеты: {0}";

    [Header("UI - Game Over")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Text _gameOverCoinText;
    [SerializeField] private Text _gameOverTimeText;
    [SerializeField] private Text _gameOverEnemyText;

    [Header("UI - Finish")]
    [SerializeField] private GameObject _finishPanel;
    [SerializeField] private Text _finishCoinText;
    [SerializeField] private Text _finishTimeText;
    [SerializeField] private Text _finishEnemyText;

    [Header("Result formats")]
    [SerializeField] private string _coinResultFormat = "Монет собрано: {0} из {1}";
    [SerializeField] private string _timeResultFormat = "Время: {0:F1} с";
    [SerializeField] private string _enemyResultFormat = "Повержено врагов: {0} из {1}";

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
        ShowResults(_gameOverPanel, _gameOverCoinText, _gameOverTimeText, _gameOverEnemyText, stats);
    }

    private void OnLevelFinished(SessionStats stats)
    {
        ShowResults(_finishPanel, _finishCoinText, _finishTimeText, _finishEnemyText, stats);
    }

    private void ShowResults(
        GameObject panel,
        Text coinText,
        Text timeText,
        Text enemyText,
        SessionStats stats)
    {
        SetText(coinText, _coinResultFormat, stats.TotalCoinsCollected, stats.TotalCoinsInLevel);
        SetText(timeText, _timeResultFormat, stats.PlayTime);
        SetText(enemyText, _enemyResultFormat, stats.EnemiesDefeated, stats.TotalEnemiesInLevel);
        ShowPanel(panel);
    }

    private static void SetText(Text text, string format, params object[] args)
    {
        if (text == null)
        {
            return;
        }

        text.text = string.Format(format, args);
    }

    private static void ShowPanel(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        panel.SetActive(true);
    }
}
