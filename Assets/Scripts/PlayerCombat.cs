using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Melee attack")]
    [SerializeField] private float attackRange = 1.1f;
    [SerializeField] private float attackRadius = 0.55f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 0.45f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Stomp")]
    [SerializeField] private float stompBounceForce = 14f;
    [SerializeField] private float stompCheckRadius = 0.35f;
    [SerializeField] private Transform stompCheck; // place at player's feet

    [Header("Knockback on hit")]
    [SerializeField] private float selfKnockbackX = 4.5f;
    [SerializeField] private float selfKnockbackY = 7.5f;

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

        if (stompCheck == null)
        {
            var go = new GameObject("StompCheck");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, -0.95f, 0f);
            stompCheck = go.transform;
        }
    }

    private void Update()
    {
        bool attackKey = Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0);
        if (attackKey) TryAttack();
    }

    private void FixedUpdate()
    {
        // Stomp detection: only when falling
        if (_controller.IsFalling && stompCheck != null)
        {
            var hit = Physics2D.OverlapCircle(stompCheck.position, stompCheckRadius, enemyLayer);
            if (hit != null)
            {
                var enemy = hit.GetComponentInParent<Enemy>();
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.Die(); // instant kill on stomp (Mario-style)
                    Bounce(stompBounceForce);
                }
            }
        }
    }

    private void TryAttack()
    {
        if (Time.time - _lastAttackTime < attackCooldown) return;
        if (!_health.IsAlive) return;
        _lastAttackTime = Time.time;

        if (_animator != null) _animator.SetTrigger(AttackTriggerHash);

        Vector2 dir = _controller.GetFacingDirection();
        Vector2 origin = (Vector2)transform.position + dir * (attackRange * 0.5f);

        var hits = Physics2D.CircleCastAll(origin, attackRadius, dir, attackRange * 0.5f, enemyLayer);
        bool hitSomething = false;
        foreach (var h in hits)
        {
            var enemy = h.collider.GetComponentInParent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(attackDamage, transform.position);
                hitSomething = true;
            }
        }

        // Apply small knockback to self if we hit something
        if (hitSomething && _rb != null)
        {
            _rb.velocity = new Vector2(-dir.x * 1.5f, _rb.velocity.y);
        }
    }

    private void Bounce(float force)
    {
        if (_rb != null)
            _rb.velocity = new Vector2(_rb.velocity.x, force);
    }

    public void ApplyKnockbackFrom(Vector2 source)
    {
        if (_rb == null) return;
        Vector2 dir = ((Vector2)transform.position - source).normalized;
        _rb.velocity = new Vector2(dir.x * selfKnockbackX, selfKnockbackY);
    }

    private void OnDrawGizmosSelected()
    {
        if (stompCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(stompCheck.position, stompCheckRadius);
        }
        Vector2 dir = _controller != null ? _controller.GetFacingDirection() : Vector2.right;
        Vector2 origin = (Vector2)transform.position + dir * (attackRange * 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, attackRadius);
    }
}
