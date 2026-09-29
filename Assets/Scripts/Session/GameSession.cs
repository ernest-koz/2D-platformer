using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CoinView))]
[RequireComponent(typeof(GameOverView))]
[RequireComponent(typeof(FinishView))]
public class GameSession : MonoBehaviour
{
    private const float EnemyDestructionDelay = 2f;

    [Header("References")]
    [SerializeField] private Player _player;

    [Header("Enemies")]
    [SerializeField] private EnemyBrain[] _enemies = new EnemyBrain[0];

    [Header("Coin Spawners")]
    [SerializeField] private PickupSpawner[] _coinSpawners = new PickupSpawner[0];

    private GameState _state = GameState.Playing;
    private int _totalCoinsCollected;
    private int _enemiesDefeated;
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
    }

    private void OnEnable()
    {
        SubscribePlayerEvents();
        SubscribeEnemyEvents();
    }

    private void Start()
    {
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
        UnsubscribePlayerEvents();
        UnsubscribeEnemyEvents();
    }

    private void OnValidate()
    {
        if (_player == null)
        {
            Debug.LogError($"Player not assigned on {gameObject.name}.", gameObject);
        }

        for (int i = 0; i < _enemies.Length; i++)
        {
            if (_enemies[i] == null)
            {
                Debug.LogError($"Enemy at index {i} not assigned on {gameObject.name}.", gameObject);
            }
        }

        for (int i = 0; i < _coinSpawners.Length; i++)
        {
            if (_coinSpawners[i] == null)
            {
                Debug.LogError($"Coin spawner at index {i} not assigned on {gameObject.name}.", gameObject);
            }
        }
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

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private bool IsPlaying()
    {
        return _state == GameState.Playing;
    }

    private void RegisterEnemyKill()
    {
        if (IsPlaying() == false)
        {
            return;
        }

        _enemiesDefeated++;
    }

    private void SubscribePlayerEvents()
    {
        _player.PickupContacted += OnPickupContacted;
        _player.LevelFinished += OnLevelFinished;
        _player.Died += OnPlayerDied;
        _player.RestartRequested += OnRestartRequested;
    }

    private void UnsubscribePlayerEvents()
    {
        _player.PickupContacted -= OnPickupContacted;
        _player.LevelFinished -= OnLevelFinished;
        _player.Died -= OnPlayerDied;
        _player.RestartRequested -= OnRestartRequested;
    }

    private void SubscribeEnemyEvents()
    {
        foreach (EnemyBrain enemy in _enemies)
        {
            enemy.Died += OnEnemyDied;
        }
    }

    private void UnsubscribeEnemyEvents()
    {
        foreach (EnemyBrain enemy in _enemies)
        {
            if (enemy == null)
            {
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
                CollectCoin(pickup);
                break;

            case PickupType.Health:
                CollectHealthPickup(pickup);
                break;

            default:
                Debug.LogError($"Unsupported pickup type: {pickup.Type}.", pickup);
                break;
        }
    }

    private void CollectCoin(Pickup pickup)
    {
        AddCoin(pickup.Amount);
        pickup.Collect();
    }

    private void CollectHealthPickup(Pickup pickup)
    {
        if (_player.Heal(pickup.Amount))
        {
            pickup.Collect();
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
        int totalCoinsInLevel = 0;

        foreach (PickupSpawner spawner in _coinSpawners)
        {
            totalCoinsInLevel += spawner.TotalCount;
        }

        return new SessionStats(
            _totalCoinsCollected,
            _enemiesDefeated,
            _playTime,
            totalCoinsInLevel,
            _enemies.Length);
    }
}

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
    public float PlayTime { get; }
    public int TotalCoinsInLevel { get; }
    public int TotalEnemiesInLevel { get; }
}
