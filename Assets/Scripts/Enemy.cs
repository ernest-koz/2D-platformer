using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack, Dead }

    [Header("Patrol")]
    [SerializeField] private float patrolSpeed = 1.6f;
    [SerializeField] private float leftX = -3f;
    [SerializeField] private float rightX = 3f;
    [SerializeField] private bool startFacingRight = true;

    [Header("Detection / Chase")]
    [SerializeField] private float detectRange = 5f;
    [SerializeField] private float chaseRange = 7f; // give up chase when player this far
    [SerializeField] private float chaseSpeed = 2.8f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.0f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float attackWindup = 0.25f;
    [SerializeField] private LayerMask obstacleLayer; // blocks attack ray (Ground + anything solid)

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float deathDelay = 0.6f;
    [SerializeField] private float knockbackResist = 2f;

    [Header("Refs")]
    [SerializeField] private Animator animator;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private int _direction = 1;
    private int _currentHealth;
    private State _state = State.Patrol;
    private float _lastAttackTime = -999f;
    private float _stateTimer = 0f;
    private float _flickerTimer = 0f;
    private SpriteRenderer[] _renderers;
    [SerializeField] private Transform _playerTarget;
    private bool _startedAttack = false;

    public bool IsDead => _state == State.Dead;
    public State CurrentState => _state;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
    private static readonly int HurtTriggerHash = Animator.StringToHash("Hurt");
    private static readonly int DieTriggerHash = Animator.StringToHash("Die");

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _currentHealth = maxHealth;
        _direction = startFacingRight ? 1 : -1;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        _renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (_playerTarget == null)
        {
            Debug.LogWarning($"Enemy {name}: _playerTarget not assigned in inspector. Will look up by tag (less efficient).", this);
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) _playerTarget = go.transform;
        }
    }

    private void Update()
    {
        if (_flickerTimer > 0f)
        {
            _flickerTimer -= Time.deltaTime;
            bool visible = Mathf.FloorToInt(Time.time * 24f) % 2 == 0;
            foreach (var r in _renderers) if (r != null) r.enabled = visible;
        }
        else
        {
            foreach (var r in _renderers) if (r != null) r.enabled = true;
        }

        if (_state == State.Dead) return;

        _stateTimer -= Time.deltaTime;

        // Update animator
        if (animator != null) animator.SetFloat(SpeedHash, Mathf.Abs(_rb.velocity.x));
    }

    private void FixedUpdate()
    {
        if (_state == State.Dead)
        {
            _rb.velocity = Vector2.zero;
            return;
        }

        switch (_state)
        {
            case State.Patrol:   TickPatrol(); break;
            case State.Chase:    TickChase();  break;
            case State.Attack:   TickAttack(); break;
        }
    }

    private void TickPatrol()
    {
        float px = (_playerTarget != null) ? _playerTarget.position.x : Mathf.Infinity;
        float dx = px - transform.position.x;

        // Detect player
        if (_playerTarget != null && Mathf.Abs(dx) <= detectRange)
        {
            _state = State.Chase;
            return;
        }

        // Patrol movement
        _rb.velocity = new Vector2(_direction * patrolSpeed, _rb.velocity.y);

        float x = transform.position.x;
        if (_direction > 0 && x >= rightX) Flip();
        else if (_direction < 0 && x <= leftX) Flip();
    }

    private void TickChase()
    {
        if (_playerTarget == null) { _state = State.Patrol; return; }
        float dx = _playerTarget.position.x - transform.position.x;
        float adx = Mathf.Abs(dx);

        // Give up chase
        if (adx > chaseRange) { _state = State.Patrol; return; }

        // Don't leave patrol zone
        float patrolMargin = 2f;
        bool outsidePatrol = transform.position.x < leftX - patrolMargin
                          || transform.position.x > rightX + patrolMargin;
        if (outsidePatrol) { _state = State.Patrol; return; }

        // If player is well outside patrol range, give up
        bool playerFarOutside = _playerTarget.position.x < leftX - patrolMargin
                             || _playerTarget.position.x > rightX + patrolMargin;
        if (playerFarOutside) { _state = State.Patrol; return; }

        // In attack range → attack
        if (adx <= attackRange)
        {
            _state = State.Attack;
            _stateTimer = 0f;
            _startedAttack = false;
            return;
        }

        // Face the player
        int targetDir = dx > 0 ? 1 : -1;
        if (targetDir != _direction) Flip();

        // Chase movement (constrained to patrol zone)
        _rb.velocity = new Vector2(_direction * chaseSpeed, _rb.velocity.y);
    }

    private void TickAttack()
    {
        _rb.velocity = new Vector2(0f, _rb.velocity.y); // stop while attacking

        if (_playerTarget == null) { _state = State.Patrol; return; }

        float dx = _playerTarget.position.x - transform.position.x;
        int targetDir = dx > 0 ? 1 : -1;
        if (targetDir != _direction) Flip();

        // Windup then strike
        if (_stateTimer < 0f && !_startedAttack)
        {
            _startedAttack = true;
            _stateTimer = attackWindup;
            if (animator != null) animator.SetTrigger(AttackTriggerHash);
            return;
        }

        if (_startedAttack && _stateTimer < 0f)
        {
            // Strike moment
            DoAttack();
            _state = State.Chase;
            _lastAttackTime = Time.time;
            _startedAttack = false;
        }
    }

    private void DoAttack()
    {
        if (Time.time - _lastAttackTime < attackCooldown) return;

        Vector2 attackOrigin = (Vector2)transform.position + Vector2.up * 0.8f;
        // Include obstacleLayer so walls/ground block the attack ray
        var hit = Physics2D.CircleCast(attackOrigin, attackRange * 0.6f,
            GetFacing(), attackRange, playerLayer | obstacleLayer);
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            var health = hit.collider.GetComponentInParent<PlayerHealth>();
            if (health != null) health.TakeDamage(attackDamage, transform.position);
        }
    }

    public void TakeDamage(int amount, Vector2 source)
    {
        if (_state == State.Dead) return;
        _currentHealth -= amount;
        _flickerTimer = 0.3f;

        if (animator != null) animator.SetTrigger(HurtTriggerHash);

        // Small knockback
        Vector2 dir = ((Vector2)transform.position - source).normalized;
        _rb.velocity = new Vector2(dir.x * knockbackResist, _rb.velocity.y);

        if (_currentHealth <= 0) Die();
        else
        {
            // Briefly stunned after hit
            _state = State.Chase;
            _stateTimer = 0.2f;
        }
    }

    public void Die()
    {
        if (_state == State.Dead) return;
        _state = State.Dead;
        if (animator != null) animator.SetTrigger(DieTriggerHash);
        if (_col != null) _col.enabled = false;
        _rb.velocity = Vector2.zero;
        Destroy(gameObject, deathDelay);
    }

    private void Flip()
    {
        _direction *= -1;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    public Vector2 GetFacing() => _direction > 0 ? Vector2.right : Vector2.left;

    private void OnDrawGizmosSelected()
    {
        // Patrol bounds
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(leftX, transform.position.y - 1, 0), new Vector3(leftX, transform.position.y + 1, 0));
        Gizmos.DrawLine(new Vector3(rightX, transform.position.y - 1, 0), new Vector3(rightX, transform.position.y + 1, 0));
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(leftX, transform.position.y, 0), new Vector3(rightX, transform.position.y, 0));
        // Detect range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        // Attack range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere((Vector2)transform.position + Vector2.up * 0.8f, attackRange);
    }
}
