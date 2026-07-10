using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerCollision : MonoBehaviour
{
    private const float StompHeightThreshold = 0.4f;
    private const float FallSpeedThreshold = 0.5f;

    private PlayerHealth _playerHealth;
    private PlayerCombat _playerCombat;
    private Rigidbody2D _rigidbody;
    private GameSession _gameSession;

    private void Awake()
    {
        _playerHealth = GetComponent<PlayerHealth>();
        _playerCombat = GetComponent<PlayerCombat>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _gameSession = GetComponent<GameSession>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Coin coin))
        {
            coin.Collect(gameObject);
            return;
        }

        if (other.TryGetComponent(out HealthPickup healthPickup))
        {
            healthPickup.Collect(gameObject);
            return;
        }

        if (other.TryGetComponent(out FinishTrigger _))
        {
            if (_gameSession != null)
            {
                _gameSession.FinishLevel();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out EnemyDeath enemyDeath) == false)
        {
            return;
        }

        if (enemyDeath == null || enemyDeath.IsDead)
        {
            return;
        }

        bool isPlayerAboveAndFalling =
            transform.position.y > collision.transform.position.y + StompHeightThreshold &&
            _rigidbody.velocity.y < -FallSpeedThreshold;

        if (isPlayerAboveAndFalling)
        {
            return;
        }

        if (_playerHealth != null)
        {
            _playerHealth.TakeDamage(collision.transform.position);
        }
    }
}
