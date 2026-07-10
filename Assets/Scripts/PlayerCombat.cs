using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Melee attack")]
    [SerializeField] private float _attackRange = 1.1f;
    [SerializeField] private float _attackRadius = 0.55f;
    [SerializeField] private float _attackCooldown = 0.45f;
    [SerializeField] private LayerMask _enemyLayer;

    [Header("Stomp")]
    [SerializeField] private float _stompBounceForce = 14f;
    [SerializeField] private float _stompCheckRadius = 0.35f;
    [SerializeField] private Transform _stompCheck;

    [Header("Knockback on hit")]
    [SerializeField] private float _selfKnockbackX = 4.5f;
    [SerializeField] private float _selfKnockbackY = 7.5f;
    [SerializeField] private float _meleeSelfKnockbackX = 1.5f;

    [Header("References")]
    [SerializeField] private PlayerInput _input;

    private const float StompCheckVerticalOffset = -0.95f;
    private const float AttackRangeHalfFraction = 0.5f;

    private PlayerMovement _playerMovement;
    private PlayerHealth _playerHealth;
    private Animator _animator;
    private Rigidbody2D _rigidbody;
    private float _lastAttackTime = -999f;

    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _playerHealth = GetComponent<PlayerHealth>();
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();

        if (_stompCheck == null)
        {
            var stompCheckGameObject = new GameObject("StompCheck");
            stompCheckGameObject.transform.SetParent(transform, false);
            stompCheckGameObject.transform.localPosition = new Vector3(0f, StompCheckVerticalOffset, 0f);
            _stompCheck = stompCheckGameObject.transform;
        }
    }

    private void Update()
    {
        if (_input.AttackPressed)
        {
            TryAttack();
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

        Vector2 direction = _playerMovement != null
            ? _playerMovement.GetFacingDirection()
            : Vector2.right;

        Vector2 origin = (Vector2)transform.position + direction * (_attackRange * AttackRangeHalfFraction);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, _attackRadius);
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

    private void TryAttack()
    {
        if (Time.time - _lastAttackTime < _attackCooldown)
        {
            return;
        }

        if (_playerHealth.IsAlive == false)
        {
            return;
        }

        _lastAttackTime = Time.time;

        if (_animator != null)
        {
            _animator.SetTrigger(AttackTriggerHash);
        }

        Vector2 direction = _playerMovement.GetFacingDirection();
        Vector2 origin = (Vector2)transform.position + direction * (_attackRange * AttackRangeHalfFraction);

        var hits = Physics2D.CircleCastAll(
            origin,
            _attackRadius,
            direction,
            _attackRange * AttackRangeHalfFraction,
            _enemyLayer);

        bool hasHitTarget = false;

        foreach (var hit in hits)
        {
            var enemyDeath = hit.collider.GetComponentInParent<EnemyDeath>();

            if (enemyDeath != null && enemyDeath.IsDead == false)
            {
                enemyDeath.Die();
                hasHitTarget = true;
            }
        }

        if (hasHitTarget && _rigidbody != null)
        {
            _rigidbody.velocity = new Vector2(-direction.x * _meleeSelfKnockbackX, _rigidbody.velocity.y);
        }
    }

    private void Bounce(float force)
    {
        if (_rigidbody != null)
        {
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, force);
        }
    }
}
