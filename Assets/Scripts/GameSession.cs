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
    public float PlayTime { get; }

    public SessionStats(int totalCoinsCollected, int enemiesDefeated, float playTime)
    {
        TotalCoinsCollected = totalCoinsCollected;
        EnemiesDefeated = enemiesDefeated;
        PlayTime = playTime;
    }
}

public class GameSession : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput _input;

    private GameState _state = GameState.Playing;
    private int _totalCoinsCollected = 0;
    private int _enemiesDefeated = 0;
    private float _playTime = 0f;

    public GameState State => _state;

    public event Action<int> CoinChanged;
    public event Action<SessionStats> GameOverStarted;
    public event Action<SessionStats> LevelFinished;

    private void Update()
    {
        if (_input != null && _input.RestartPressed && (_state == GameState.GameOver || _state == GameState.Finish))
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
        CoinChanged?.Invoke(_totalCoinsCollected);
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

        _state = GameState.GameOver;
        GameOverStarted?.Invoke(BuildStats());
    }

    public void FinishLevel()
    {
        if (_state != GameState.Playing)
        {
            return;
        }

        _state = GameState.Finish;
        LevelFinished?.Invoke(BuildStats());
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private SessionStats BuildStats() =>
        new SessionStats(_totalCoinsCollected, _enemiesDefeated, _playTime);
}
