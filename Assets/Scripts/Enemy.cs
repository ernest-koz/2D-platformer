using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack, Dead }

    [Header("Patrol")]
    [FormerlySerializedAs("patrolSpeed")] [SerializeField] private float _patrolSpeed = 1.6f;
    [FormerlySerializedAs("leftX")] [SerializeField] private float _leftX = -3f;
    [FormerlySerializedAs("rightX")] [SerializeField] private float _rightX = 3f;
    [FormerlySerializedAs("startFacingRight")] [SerializeField] private bool _startFacingRight = true;

    [Header("Detection / Chase")]
    [FormerlySerializedAs("detectRange")] [SerializeField] private float _detectRange = 5f;
    [FormerlySerializedAs("chaseRange")] [SerializeField] private float _chaseRange = 7f;
    [FormerlySerializedAs("chaseSpeed")] [SerializeField] private float _chaseSpeed = 2.8f;
    [FormerlySerializedAs("playerLayer")] [SerializeField] private LayerMask _playerLayer;

    [Header("Attack")]
    [FormerlySerializedAs("attackRange")] [SerializeField] private float _attackRange = 1.0f;
    [FormerlySerializedAs("attackDamage")] [SerializeField] private int _attackDamage = 1;
    [FormerlySerializedAs("attackCooldown")] [SerializeField] private float _attackCooldown = 1.2f;
    [FormerlySerializedAs("attackWindup")] [SerializeField] private float _attackWindup = 0.25f;
    [FormerlySerializedAs("attackOriginHeight")] [SerializeField] private float _attackOriginHeight = 0.8f;
    [FormerlySerializedAs("obstacleLayer")] [SerializeField] private LayerMask _obstacleLayer;

    [Header("Health")]
    [FormerlySerializedAs("maxHealth")] [SerializeField] private int _maxHealth = 3;
    [FormerlySerializedAs("deathDelay")] [SerializeField] private float _deathDelay = 0.6f;
    [FormerlySerializedAs("knockbackResist")] [SerializeField] private float _knockbackResist = 2f;

    [Header("Refs")]
    [FormerlySerializedAs("animator")] [SerializeField] private Animator _animator;
    [SerializeField] private Transform _playerTarget;

    private const float FlickerFrequency = 24f;
    private const float HurtFlickerDuration = 0.3f;
    private const float StunDuration = 0.2f;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private int _direction = 1;
    private int _currentHealth;
    private State _state = State.Patrol;
    private float _lastAttackTime = -999f;
    private float _stateTimer = 0f;
    private float _flickerTimer = 0f;
    private SpriteRenderer[] _renderers;
    private bool _startedAttack = false;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
    private static readonly int HurtTriggerHash = Animator.StringToHash("Hurt");
    private static readonly int DieTriggerHash = Animator.StringToHash("Die");

    public bool IsDead => _state == State.Dead;
    public State CurrentState => _state;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _currentHealth = _maxHealth;
        _direction = _startFacingRight ? 1 : -1;

        if (_animator == null) _animator = GetComponentInChildren<Animator>();

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
            bool visible = Mathf.FloorToInt(Time.time * FlickerFrequency) % 2 == 0;

            foreach (var r in _renderers) if (r != null) r.enabled = visible;
        }
        else
        {
            foreach (var r in _renderers) if (r != null) r.enabled = true;
        }

        if (_state == State.Dead) return;

        _stateTimer -= Time.deltaTime;

        if (_animator != null) _animator.SetFloat(SpeedHash, Mathf.Abs(_rb.velocity.x));
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(_leftX, transform.position.y - 1, 0), new Vector3(_leftX, transform.position.y + 1, 0));
        Gizmos.DrawLine(new Vector3(_rightX, transform.position.y - 1, 0), new Vector3(_rightX, transform.position.y + 1, 0));
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(_leftX, transform.position.y, 0), new Vector3(_rightX, transform.position.y, 0));
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _detectRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere((Vector2)transform.position + Vector2.up * _attackOriginHeight, _attackRange);
    }

    public void TakeDamage(int amount, Vector2 source)
    {
        if (_state == State.Dead) return;

        _currentHealth -= amount;
        _flickerTimer = HurtFlickerDuration;

        if (_animator != null) _animator.SetTrigger(HurtTriggerHash);

        Vector2 dir = ((Vector2)transform.position - source).normalized;
        _rb.velocity = new Vector2(dir.x * _knockbackResist, _rb.velocity.y);

        if (_currentHealth <= 0) Die();
        else
        {
            _state = State.Chase;
            _stateTimer = StunDuration;
        }
    }

    public void Die()
    {
        if (_state == State.Dead) return;

        _state = State.Dead;

        if (_animator != null) _animator.SetTrigger(DieTriggerHash);

        if (_col != null) _col.enabled = false;

        _rb.velocity = Vector2.zero;

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterEnemyKill();

        Destroy(gameObject, _deathDelay);
    }

    public Vector2 GetFacing() =>
        _direction > 0 ? Vector2.right : Vector2.left;

    private void TickPatrol()
    {
        float px = (_playerTarget != null) ? _playerTarget.position.x : Mathf.Infinity;
        float dx = px - transform.position.x;

        if (_playerTarget != null && Mathf.Abs(dx) <= _detectRange)
        {
            _state = State.Chase;
            return;
        }

        _rb.velocity = new Vector2(_direction * _patrolSpeed, _rb.velocity.y);

        float x = transform.position.x;

        if (_direction > 0 && x >= _rightX) Flip();
        else if (_direction < 0 && x <= _leftX) Flip();
    }

    private void TickChase()
    {
        const float PatrolMargin = 2f;

        if (_playerTarget == null) { _state = State.Patrol; return; }

        float dx = _playerTarget.position.x - transform.position.x;
        float adx = Mathf.Abs(dx);

        if (adx > _chaseRange) { _state = State.Patrol; return; }

        bool outsidePatrol = transform.position.x < _leftX - PatrolMargin
                          || transform.position.x > _rightX + PatrolMargin;

        if (outsidePatrol) { _state = State.Patrol; return; }

        bool playerFarOutside = _playerTarget.position.x < _leftX - PatrolMargin
                             || _playerTarget.position.x > _rightX + PatrolMargin;

        if (playerFarOutside) { _state = State.Patrol; return; }

        if (adx <= _attackRange)
        {
            _state = State.Attack;
            _stateTimer = 0f;
            _startedAttack = false;
            return;
        }

        int targetDir = dx > 0 ? 1 : -1;

        if (targetDir != _direction) Flip();

        _rb.velocity = new Vector2(_direction * _chaseSpeed, _rb.velocity.y);
    }

    private void TickAttack()
    {
        _rb.velocity = new Vector2(0f, _rb.velocity.y);

        if (_playerTarget == null) { _state = State.Patrol; return; }

        float dx = _playerTarget.position.x - transform.position.x;
        int targetDir = dx > 0 ? 1 : -1;

        if (targetDir != _direction) Flip();

        if (_stateTimer < 0f && _startedAttack == false)
        {
            _startedAttack = true;
            _stateTimer = _attackWindup;

            if (_animator != null) _animator.SetTrigger(AttackTriggerHash);
            return;
        }

        if (_startedAttack && _stateTimer < 0f)
        {
            DoAttack();
            _state = State.Chase;
            _lastAttackTime = Time.time;
            _startedAttack = false;
        }
    }

    private void DoAttack()
    {
        if (Time.time - _lastAttackTime < _attackCooldown) return;

        Vector2 attackOrigin = (Vector2)transform.position + Vector2.up * _attackOriginHeight;
        var hit = Physics2D.CircleCast(attackOrigin, _attackRange * 0.6f,
            GetFacing(), _attackRange, _playerLayer | _obstacleLayer);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            var health = hit.collider.GetComponentInParent<PlayerHealth>();

            if (health != null) health.TakeDamage(_attackDamage, transform.position);
        }
    }

    private void Flip()
    {
        _direction *= -1;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }
}
