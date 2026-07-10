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

    private const float StompCheckVerticalOffset = -0.95f;

    private PlayerMovement _playerMovement;
    private PlayerHealth _playerHealth;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _playerHealth = GetComponent<PlayerHealth>();
        _rigidbody = GetComponent<Rigidbody2D>();

        if (_enemyLayer == 0)
        {
            _enemyLayer = 1 << 9;
        }

        if (_stompCheck == null)
        {
            var stompCheckGameObject = new GameObject("StompCheck");
            stompCheckGameObject.transform.SetParent(transform, false);
            stompCheckGameObject.transform.localPosition = new Vector3(0f, StompCheckVerticalOffset, 0f);
            _stompCheck = stompCheckGameObject.transform;
        }
    }

    private void FixedUpdate()
    {
        if (_playerMovement.IsFalling && _stompCheck != null)
        {
            var hit = Physics2D.OverlapCircle(_stompCheck.position, _stompCheckRadius, _enemyLayer);

            if (hit != null)
            {
                var enemyDeath = hit.GetComponentInParent<EnemyDeath>();

                if (enemyDeath != null && enemyDeath.IsDead == false)
                {
                    enemyDeath.Die();
                    Bounce(_stompBounceForce);
                }
            }
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
        if (_rigidbody == null)
        {
            return;
        }

        Vector2 direction = ((Vector2)transform.position - source).normalized;
        _rigidbody.velocity = new Vector2(direction.x * _selfKnockbackX, _selfKnockbackY);
    }

    private void Bounce(float force)
    {
        if (_rigidbody != null)
        {
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, force);
        }
    }
}
