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

    public int TotalCoinsCollected { get; }
    public int EnemiesDefeated { get; }
    public int TotalCoinsInLevel { get; }
    public int TotalEnemiesInLevel { get; }
    public float PlayTime { get; }
}

[RequireComponent(typeof(CoinView))]
[RequireComponent(typeof(GameOverView))]
[RequireComponent(typeof(FinishView))]
public class GameSession : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader _input;
    [SerializeField] private MonoBehaviour[] _gameplayComponents;
    [SerializeField] private Player _player;
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
    private CoinView _coinView;
    private GameOverView _gameOverView;
    private FinishView _finishView;

    public GameState State => _state;

    public void AddCoin(int amount)
    {
        if (_state != GameState.Playing)
        {
            return;
        }

        _totalCoinsCollected += amount;
        _coinView.Render(_totalCoinsCollected);
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

    private void Awake()
    {
        _coinView = GetComponent<CoinView>();
        _gameOverView = GetComponent<GameOverView>();
        _finishView = GetComponent<FinishView>();

        if (_input != null &&
            _player != null &&
            _playerHealth != null &&
            _fallDetector != null)
        {
            return;
        }

        Debug.LogError($"GameSession has missing required references on {gameObject.name}.", gameObject);
        enabled = false;
    }

    private void OnEnable()
    {
        TogglePlayerEvents(true);
        ToggleEnemyEvents(true);
    }

    private void Start()
    {
        if (_gameplayComponents == null || _gameplayComponents.Length == 0)
        {
            Debug.LogWarning($"GameSession: gameplayComponents array is empty on {gameObject.name}.", gameObject);
        }

        CountLevelPickups();
        CountEnemies();
        _coinView.Render(_totalCoinsCollected);
    }

    private void Update()
    {
        if (_input.IsRestartPressed)
        {
            if (_state != GameState.Playing)
            {
                RestartLevel();
            }

            return;
        }

        if (_state == GameState.Playing)
        {
            _playTime += Time.deltaTime;
        }
    }

    private void OnDisable()
    {
        TogglePlayerEvents(false);
        ToggleEnemyEvents(false);
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
        if (_player == null)
        {
            return;
        }

        if (_playerHealth == null)
        {
            return;
        }

        if (_fallDetector == null)
        {
            return;
        }

        if (subscribe)
        {
            _player.PickupContacted += OnPickupContacted;
            _player.EnemyContacted += OnEnemyContacted;
            _player.LevelFinished += OnLevelFinished;
            _playerHealth.Died += OnPlayerDied;
            _fallDetector.FellToDeath += OnPlayerDied;
        }
        else
        {
            _player.PickupContacted -= OnPickupContacted;
            _player.EnemyContacted -= OnEnemyContacted;
            _player.LevelFinished -= OnLevelFinished;
            _playerHealth.Died -= OnPlayerDied;
            _fallDetector.FellToDeath -= OnPlayerDied;
        }
    }

    private void CountEnemies()
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

            _totalEnemiesInLevel++;
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

            Health enemyHealth = enemy.GetComponent<Health>();

            if (subscribe)
            {
                enemyHealth.Died += OnEnemyDied;
                enemy.Died += OnEnemyBrainDied;
            }
            else
            {
                enemyHealth.Died -= OnEnemyDied;
                enemy.Died -= OnEnemyBrainDied;
            }
        }
    }

    private void OnEnemyBrainDied(EnemyBrain enemy)
    {
        enemy.Died -= OnEnemyBrainDied;
        Destroy(enemy.gameObject, 2f);
    }

    private void OnPickupContacted(Pickup pickup)
    {
        switch (pickup.Type)
        {
            case PickupType.Coin:
                AddCoin(pickup.Amount);
                pickup.Collect();
                break;

            case PickupType.Health:
                if (_playerHealth.Heal(pickup.Amount))
                {
                    pickup.Collect();
                }
                break;

            default:
                Debug.LogError($"Unsupported pickup type: {pickup.Type}.", pickup);
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
        _gameOverView.Show(BuildStats());
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
        _finishView.Show(BuildStats());
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
