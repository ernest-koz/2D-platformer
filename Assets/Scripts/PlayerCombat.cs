using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Stomp")]
    [SerializeField] private float _stompBounceForce = 14f;
    [SerializeField] private float _stompCheckRadius = 0.35f;
    [SerializeField] private Transform _stompCheck;
    [SerializeField] private LayerMask _enemyLayer;

    [Header("Knockback on hit")]
    [SerializeField] private float _selfKnockbackX = 4.5f;
    [SerializeField] private float _selfKnockbackY = 7.5f;

    private PlayerMovement _playerMovement;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (_stompCheck == null)
        {
            return;
        }

        if (_playerMovement.IsFalling == false)
        {
            return;
        }

        var hit = Physics2D.OverlapCircle(_stompCheck.position, _stompCheckRadius, _enemyLayer);

        if (hit == null || transform.position.y <= hit.bounds.max.y)
        {
            return;
        }

        var enemyDeath = hit.GetComponentInParent<EnemyDeath>();

        if (enemyDeath != null && enemyDeath.IsDead == false)
        {
            enemyDeath.Die();
            Bounce(_stompBounceForce);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_stompCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_stompCheck.position, _stompCheckRadius);
        }
    }

    public void ApplyKnockbackFrom(Vector2 source)
    {
        Vector2 direction = ((Vector2)transform.position - source).normalized;
        _rigidbody.velocity = new Vector2(direction.x * _selfKnockbackX, _selfKnockbackY);
    }

    private void Bounce(float force)
    {
        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, force);
    }
}
