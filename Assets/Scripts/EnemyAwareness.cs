using UnityEngine;

public class EnemyAwareness : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float _detectRange = 5f;
    [SerializeField] private float _chaseRange = 7f;
    [SerializeField] private LayerMask _playerLayer;

    [Header("References")]
    [SerializeField] private Transform _playerTarget;
    [SerializeField] private EnemyLocomotion _locomotion;
    [SerializeField] private EnemyStriker _striker;
    [SerializeField] private EnemyDeath _death;
    [SerializeField] private GameSession _gameSession;

    private State _state = State.Patrol;
    private Animator _animator;
    private Rigidbody2D _rigidbody;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
    private const float AttackGizmoHeightFraction = 0.5f;
    private const float DefaultAttackGizmoRadius = 1f;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (_death == null)
        {
            return;
        }

        _death.Died += OnEnemyDied;
    }

    private void OnDisable()
    {
        if (_death == null)
        {
            return;
        }

        _death.Died -= OnEnemyDied;
    }

    private void Start()
    {
        if (_playerTarget == null)
        {
            Debug.LogError(
                $"EnemyAwareness {name}: playerTarget not assigned. Enemy will patrol without chasing.",
                gameObject);
        }

        if (_locomotion == null)
        {
            Debug.LogError(
                $"EnemyAwareness {name}: Locomotion not assigned. Enemy cannot move.",
                gameObject);
        }

        if (_striker == null)
        {
            Debug.LogError(
                $"EnemyAwareness {name}: Striker not assigned. Enemy cannot attack.",
                gameObject);
        }

        if (_death == null)
        {
            Debug.LogError(
                $"EnemyAwareness {name}: Death not assigned. Enemy death handling disabled.",
                gameObject);
        }
    }

    private void Update()
    {
        if (_state == State.Dead)
        {
            return;
        }

        _animator?.SetFloat(SpeedHash, Mathf.Abs(_rigidbody.velocity.x));
    }

    private void FixedUpdate()
    {
        if (_state == State.Dead)
        {
            return;
        }

        if (_gameSession == null)
        {
            _locomotion?.Stop();
            return;
        }

        if (_gameSession.State == GameState.Playing)
        {
        }
        else
        {
            _locomotion?.Stop();
            return;
        }

        switch (_state)
        {
            case State.Patrol:
                TickPatrol();
                break;

            case State.Chase:
                TickChase();
                break;

            case State.Attack:
                TickAttack();
                break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_locomotion == null)
        {
            return;
        }

        Vector3 position = transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(position, _detectRange);

        float attackRadius = _striker?.AttackRange ?? DefaultAttackGizmoRadius;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(
            position + Vector3.up * attackRadius * AttackGizmoHeightFraction,
            attackRadius);
    }

    private void OnEnemyDied(EnemyDeath death)
    {
        _state = State.Dead;

        _locomotion?.Stop();
        _striker?.CancelWindup();
        this.enabled = false;
    }

    private void TickPatrol()
    {
        if (_playerTarget == null)
        {
            _locomotion?.Patrol();
            return;
        }

        float distanceToPlayer = _playerTarget.position.x - transform.position.x;

        if (Mathf.Abs(distanceToPlayer) <= _detectRange)
        {
            _state = State.Chase;
            return;
        }

        _locomotion?.Patrol();
    }

    private void TickChase()
    {
        if (_playerTarget == null || _locomotion == null)
        {
            _state = State.Patrol;
            return;
        }

        float distanceToPlayer = _playerTarget.position.x - transform.position.x;
        float absoluteDistance = Mathf.Abs(distanceToPlayer);

        if (absoluteDistance > _chaseRange)
        {
            _state = State.Patrol;
            return;
        }

        if (absoluteDistance <= (_striker?.AttackRange ?? 0f))
        {
            _state = State.Attack;
            return;
        }

        if (_locomotion.HasGroundAhead() == false)
        {
            _locomotion.Stop();
            _state = State.Patrol;
            return;
        }

        _locomotion.Chase(_playerTarget.position);
    }

    private void TickAttack()
    {
        if (_playerTarget == null || _locomotion == null || _striker == null)
        {
            _state = State.Patrol;
            return;
        }

        _locomotion.Stop();

        _locomotion.FaceTowards(_playerTarget.position);

        if (_striker.IsOnCooldown == false)
        {
            _striker.BeginWindup();

            _animator?.SetTrigger(AttackTriggerHash);
        }

        bool attackCompleted = _striker.TickWindup(
            transform.position,
            _locomotion.FacingVector,
            _playerLayer);

        if (attackCompleted)
        {
            _state = State.Chase;
        }
    }

    private enum State
    {
        Patrol,
        Chase,
        Attack,
        Dead
    }
}
