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
    [SerializeField] private InputReader _input;
    [SerializeField] private MonoBehaviour[] _gameplayComponents;
    [SerializeField] private Player _player;
    [SerializeField] private PlayerCollisionHandler _playerCollision;
    [SerializeField] private Health _playerHealth;
    [SerializeField] private FallDetector _fallDetector;

    [Header("Enemies")]
    [SerializeField] private EnemyBrain[] _enemies;

    [Header("Coin Spawners")]
    [SerializeField] private PickupSpawner[] _coinSpawners;

    private GameState _state = GameState.Playing;
    private int _totalCoinsCollected;
    private int _enemiesDefeated;
    private int _totalCoinsInLevel;
    private int _totalEnemiesInLevel;
    private float _playTime;

    public event Action<int> CoinChanged;
    public event Action<SessionStats> GameOverStarted;
    public event Action<SessionStats> LevelFinished;

    public GameState State => _state;

    private void Start()
    {
        if (_input == null)
        {
            Debug.LogError($"GameSession: InputReader not assigned on {gameObject.name}. Restart will not work.", gameObject);
        }

        if (_playerCollision == null)
        {
            Debug.LogError($"GameSession: PlayerCollisionHandler not assigned on {gameObject.name}.", gameObject);
        }

        if (_playerHealth == null)
        {
            Debug.LogError($"GameSession: Health not assigned on {gameObject.name}.", gameObject);
        }

        if (_fallDetector == null)
        {
            Debug.LogError($"GameSession: FallDetector not assigned on {gameObject.name}.", gameObject);
        }

        if (_gameplayComponents == null)
        {
            Debug.LogWarning($"GameSession: gameplayComponents not assigned on {gameObject.name}.", gameObject);
        }
        else if (_gameplayComponents.Length == 0)
        {
            Debug.LogWarning($"GameSession: gameplayComponents array is empty on {gameObject.name}.", gameObject);
        }

        CountLevelPickups();
        TogglePlayerEvents(true);
        ToggleEnemyEvents(true);
    }

    private void OnDestroy()
    {
        TogglePlayerEvents(false);
        ToggleEnemyEvents(false);
    }

    private void Update()
    {
        if (_input.IsRestartPressed == false)
        {
            if (_state == GameState.Playing)
            {
                _playTime += Time.deltaTime;
            }

            return;
        }

        if (_state == GameState.Playing)
        {
            return;
        }

        RestartLevel();
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
        if (_state == GameState.Playing)
        {
            _enemiesDefeated++;
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void CountLevelPickups()
    {
        _totalCoinsInLevel = 0;

        if (_coinSpawners == null)
        {
            return;
        }

        foreach (PickupSpawner spawner in _coinSpawners)
        {
            if (spawner == null)
            {
                continue;
            }

            _totalCoinsInLevel += spawner.TotalCount;
        }
    }

    private void TogglePlayerEvents(bool subscribe)
    {
        if (_playerCollision == null || _playerHealth == null || _fallDetector == null)
        {
            return;
        }

        if (subscribe)
        {
            _playerCollision.PickupCollected += OnPickupCollected;
            _playerCollision.EnemyContacted += OnEnemyContacted;
            _playerCollision.LevelFinished += OnLevelFinished;
            _playerHealth.Died += OnPlayerDied;
            _fallDetector.FellToDeath += OnPlayerDied;
        }
        else
        {
            _playerCollision.PickupCollected -= OnPickupCollected;
            _playerCollision.EnemyContacted -= OnEnemyContacted;
            _playerCollision.LevelFinished -= OnLevelFinished;
            _playerHealth.Died -= OnPlayerDied;
            _fallDetector.FellToDeath -= OnPlayerDied;
        }
    }

    private void ToggleEnemyEvents(bool subscribe)
    {
        if (_enemies == null)
        {
            return;
        }

        foreach (EnemyBrain enemy in _enemies)
        {
            if (enemy == null)
            {
                continue;
            }

            if (subscribe)
            {
                _totalEnemiesInLevel++;
            }

            Health enemyHealth = enemy.GetComponent<Health>();

            if (subscribe)
            {
                enemyHealth.Died += OnEnemyDied;
            }
            else
            {
                enemyHealth.Died -= OnEnemyDied;
            }
        }
    }

    private void OnPickupCollected(Pickup pickup)
    {
        switch (pickup.Type)
        {
            case PickupType.Coin:
                AddCoin(pickup.Amount);
                break;

            case PickupType.Health:
                if (_playerHealth.IsAlive == false)
                {
                    break;
                }

                if (_playerHealth.CurrentHealth < _playerHealth.MaximumHealth)
                {
                    _playerHealth.Heal(pickup.Amount);
                }
                break;
        }
    }

    private void OnEnemyContacted(Collision2D collision)
    {
        _playerHealth.TakeDamage(1, collision.transform.position);
    }

    private void OnLevelFinished()
    {
        FinishLevel();
    }

    private void OnPlayerDied()
    {
        GameOver();
    }

    private void OnEnemyDied()
    {
        RegisterEnemyKill();
    }

    private void GameOver()
    {
        if (_state != GameState.Playing)
        {
            return;
        }

        _state = GameState.GameOver;

        if (_input != null)
        {
            _input.IsBlocked = true;
        }

        SuspendGameplay();
        GameOverStarted?.Invoke(BuildStats());
    }

    private void FinishLevel()
    {
        if (_state != GameState.Playing)
        {
            return;
        }

        _state = GameState.Finish;

        if (_input != null)
        {
            _input.IsBlocked = true;
        }

        SuspendGameplay();
        LevelFinished?.Invoke(BuildStats());
    }

    private void SuspendGameplay()
    {
        if (_gameplayComponents == null)
        {
            return;
        }

        foreach (MonoBehaviour component in _gameplayComponents)
        {
            if (component == null)
            {
                continue;
            }

            component.enabled = false;
        }
    }

    private SessionStats BuildStats()
    {
        return new SessionStats(
            _totalCoinsCollected,
            _enemiesDefeated,
            _playTime,
            _totalCoinsInLevel,
            _totalEnemiesInLevel);
    }
}
