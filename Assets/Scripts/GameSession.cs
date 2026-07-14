using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    GameOver,
    Finish
}

public readonly struct SessionStats
{
    public int TotalCoinsCollected { get; }
    public int EnemiesDefeated { get; }
    public int TotalCoinsInLevel { get; }
    public int TotalEnemiesInLevel { get; }
    public float PlayTime { get; }

    public SessionStats(
        int totalCoinsCollected,
        int enemiesDefeated,
        float playTime,
        int totalCoinsInLevel,
        int totalEnemiesInLevel)
    {
        TotalCoinsCollected = totalCoinsCollected;
        EnemiesDefeated = enemiesDefeated;
        PlayTime = playTime;
        TotalCoinsInLevel = totalCoinsInLevel;
        TotalEnemiesInLevel = totalEnemiesInLevel;
    }
}

public class GameSession : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput _input;

    private GameState _state = GameState.Playing;
    private int _totalCoinsCollected;
    private int _enemiesDefeated;
    private int _totalCoinsInLevel;
    private int _totalEnemiesInLevel;
    private float _playTime;

    public GameState State => _state;

    public event Action<int> CoinChanged;
    public event Action<SessionStats> GameOverStarted;
    public event Action<SessionStats> LevelFinished;

    private void Update()
    {
        if (_input.IsRestartPressed && (_state == GameState.GameOver || _state == GameState.Finish))
        {
            RestartLevel();
            return;
        }

        if (_state == GameState.Playing)
        {
            _playTime += Time.deltaTime;
        }
    }

    public void AddCoin(int amount)
    {
        if (_state == GameState.Playing)
        {
            _totalCoinsCollected += amount;
            CoinChanged?.Invoke(_totalCoinsCollected);
        }
    }

    public void RegisterEnemyKill()
    {
        if (_state == GameState.Playing)
        {
            _enemiesDefeated++;
        }
    }

    public void RegisterCoin()
    {
        _totalCoinsInLevel++;
    }

    public void RegisterEnemy()
    {
        _totalEnemiesInLevel++;
    }

    public void GameOver()
    {
        if (_state == GameState.Playing)
        {
            _state = GameState.GameOver;
            GameOverStarted?.Invoke(BuildStats());
        }
    }

    public void FinishLevel()
    {
        if (_state == GameState.Playing)
        {
            _state = GameState.Finish;
            LevelFinished?.Invoke(BuildStats());
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private SessionStats BuildStats() =>
        new SessionStats(
            _totalCoinsCollected,
            _enemiesDefeated,
            _playTime,
            _totalCoinsInLevel,
            _totalEnemiesInLevel);
}
