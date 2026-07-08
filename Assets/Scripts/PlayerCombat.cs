using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Melee attack")]
    [FormerlySerializedAs("attackRange")] [SerializeField] private float _attackRange = 1.1f;
    [FormerlySerializedAs("attackRadius")] [SerializeField] private float _attackRadius = 0.55f;
    [FormerlySerializedAs("attackDamage")] [SerializeField] private int _attackDamage = 1;
    [FormerlySerializedAs("attackCooldown")] [SerializeField] private float _attackCooldown = 0.45f;
    [FormerlySerializedAs("enemyLayer")] [SerializeField] private LayerMask _enemyLayer;

    [Header("Stomp")]
    [FormerlySerializedAs("stompBounceForce")] [SerializeField] private float _stompBounceForce = 14f;
    [FormerlySerializedAs("stompCheckRadius")] [SerializeField] private float _stompCheckRadius = 0.35f;
    [FormerlySerializedAs("stompCheck")] [SerializeField] private Transform _stompCheck;

    [Header("Knockback on hit")]
    [FormerlySerializedAs("selfKnockbackX")] [SerializeField] private float _selfKnockbackX = 4.5f;
    [FormerlySerializedAs("selfKnockbackY")] [SerializeField] private float _selfKnockbackY = 7.5f;
    [FormerlySerializedAs("meleeSelfKnockbackX")] [SerializeField] private float _meleeSelfKnockbackX = 1.5f;

    private PlayerController _controller;
    private PlayerHealth _health;
    private Animator _animator;
    private Rigidbody2D _rb;
    private float _lastAttackTime = -999f;

    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _health = GetComponent<PlayerHealth>();
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();

        if (_stompCheck == null)
        {
            var go = new GameObject("StompCheck");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, -0.95f, 0f);
            _stompCheck = go.transform;
        }
    }

    private void Update()
    {
        bool attackKey = Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0);

        if (attackKey) TryAttack();
    }

    private void FixedUpdate()
    {
        if (_controller.IsFalling && _stompCheck != null)
        {
            var hit = Physics2D.OverlapCircle(_stompCheck.position, _stompCheckRadius, _enemyLayer);

            if (hit != null)
            {
                var enemy = hit.GetComponentInParent<Enemy>();

                if (enemy != null && enemy.IsDead == false)
                {
                    enemy.Die();
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

        Vector2 dir = _controller != null ? _controller.GetFacingDirection() : Vector2.right;
        Vector2 origin = (Vector2)transform.position + dir * (_attackRange * 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, _attackRadius);
    }

    public void ApplyKnockbackFrom(Vector2 source)
    {
        if (_rb == null) return;

        Vector2 dir = ((Vector2)transform.position - source).normalized;
        _rb.velocity = new Vector2(dir.x * _selfKnockbackX, _selfKnockbackY);
    }

    private void TryAttack()
    {
        if (Time.time - _lastAttackTime < _attackCooldown) return;

        if (_health.IsAlive == false) return;

        _lastAttackTime = Time.time;

        if (_animator != null) _animator.SetTrigger(AttackTriggerHash);

        Vector2 dir = _controller.GetFacingDirection();
        Vector2 origin = (Vector2)transform.position + dir * (_attackRange * 0.5f);

        var hits = Physics2D.CircleCastAll(origin, _attackRadius, dir, _attackRange * 0.5f, _enemyLayer);
        bool hitSomething = false;

        foreach (var h in hits)
        {
            var enemy = h.collider.GetComponentInParent<Enemy>();

            if (enemy != null && enemy.IsDead == false)
            {
                enemy.TakeDamage(_attackDamage, transform.position);
                hitSomething = true;
            }
        }

        if (hitSomething && _rb != null)
        {
            _rb.velocity = new Vector2(-dir.x * _meleeSelfKnockbackX, _rb.velocity.y);
        }
    }

    private void Bounce(float force)
    {
        if (_rb != null)
            _rb.velocity = new Vector2(_rb.velocity.x, force);
    }
}
