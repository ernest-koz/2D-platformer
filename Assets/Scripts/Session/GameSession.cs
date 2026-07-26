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
    private const float EnemyDestructionDelay = 2f;

    [Header("References")]
    [SerializeField] private Player _player;

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

    private void Awake()
    {
        _coinView = GetComponent<CoinView>();
        _gameOverView = GetComponent<GameOverView>();
        _finishView = GetComponent<FinishView>();

        if (_player == null)
        {
            Debug.LogError($"Player not assigned on {gameObject.name}.", gameObject);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        TogglePlayerEvents(true);
        ToggleEnemyEvents(true);
    }

    private void Start()
    {
        CountLevelPickups();
        CountEnemies();
        _coinView.Render(_totalCoinsCollected);
    }

    private void Update()
    {
        if (IsPlaying() == false)
        {
            return;
        }

        _playTime += Time.deltaTime;
    }

    private void OnDisable()
    {
        TogglePlayerEvents(false);
        ToggleEnemyEvents(false);
    }

    public void AddCoin(int amount)
    {
        if (IsPlaying() == false)
        {
            return;
        }

        _totalCoinsCollected += amount;
        _coinView.Render(_totalCoinsCollected);
    }

    public void RegisterEnemyKill()
    {
        if (IsPlaying() == false)
        {
            return;
        }

        _enemiesDefeated++;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private bool IsPlaying()
    {
        return _state == GameState.Playing;
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

    private void CountEnemies()
    {
        _totalEnemiesInLevel = 0;

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

    private void TogglePlayerEvents(bool subscribe)
    {
        if (_player == null)
        {
            return;
        }

        if (subscribe)
        {
            _player.PickupContacted += OnPickupContacted;
            _player.LevelFinished += OnLevelFinished;
            _player.Died += OnPlayerDied;
            _player.RestartRequested += OnRestartRequested;
            return;
        }

        _player.PickupContacted -= OnPickupContacted;
        _player.LevelFinished -= OnLevelFinished;
        _player.Died -= OnPlayerDied;
        _player.RestartRequested -= OnRestartRequested;
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
                enemy.Died += OnEnemyDied;
                continue;
            }

            enemy.Died -= OnEnemyDied;
        }
    }

    private void OnPickupContacted(Pickup pickup)
    {
        if (IsPlaying() == false)
        {
            return;
        }

        if (pickup.IsCollected)
        {
            return;
        }

        switch (pickup.Type)
        {
            case PickupType.Coin:
                AddCoin(pickup.Amount);
                pickup.Collect();
                break;

            case PickupType.Health:
                if (_player.Heal(pickup.Amount))
                {
                    pickup.Collect();
                }

                break;

            default:
                Debug.LogError($"Unsupported pickup type: {pickup.Type}.", pickup);
                break;
        }
    }

    private void OnLevelFinished()
    {
        FinishLevel();
    }

    private void OnPlayerDied()
    {
        GameOver();
    }

    private void OnEnemyDied(EnemyBrain enemy)
    {
        enemy.Died -= OnEnemyDied;
        RegisterEnemyKill();
        Destroy(enemy.gameObject, EnemyDestructionDelay);
    }

    private void OnRestartRequested()
    {
        if (IsPlaying())
        {
            return;
        }

        RestartLevel();
    }

    private void GameOver()
    {
        if (IsPlaying() == false)
        {
            return;
        }

        _state = GameState.GameOver;
        SuspendGameplay();
        _gameOverView.Show(BuildStats());
    }

    private void FinishLevel()
    {
        if (IsPlaying() == false)
        {
            return;
        }

        _state = GameState.Finish;
        SuspendGameplay();
        _finishView.Show(BuildStats());
    }

    private void SuspendGameplay()
    {
        _player.Suspend();

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

            enemy.Suspend();
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
