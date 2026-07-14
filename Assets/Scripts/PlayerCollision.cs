using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerCollision : MonoBehaviour
{
    private const float StompHeightThreshold = 0.4f;
    private const float FallSpeedThreshold = 0.5f;

    [SerializeField] private GameSession _gameSession;

    private PlayerHealth _playerHealth;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _playerHealth = GetComponent<PlayerHealth>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Coin coin))
        {
            coin.Collect();
            return;
        }

        if (other.TryGetComponent(out HealthPickup healthPickup))
        {
            healthPickup.Collect(gameObject);
            return;
        }

        if (other.TryGetComponent(out FinishTrigger _))
        {
            _gameSession?.FinishLevel();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out EnemyDeath enemyDeath) == false)
        {
            return;
        }

        if (enemyDeath.IsDead)
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

        _playerHealth.TakeDamage(collision.transform.position);
    }
}
