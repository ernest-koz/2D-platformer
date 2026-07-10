using UnityEngine;

public class EnemyAwareness : MonoBehaviour
{
    private enum State { Patrol, Chase, Attack, Dead }

    [Header("Detection")]
    [SerializeField] private float _detectRange = 5f;
    [SerializeField] private float _chaseRange = 7f;
    [SerializeField] private LayerMask _playerLayer;

    [Header("Refs")]
    [SerializeField] private Transform _playerTarget;
    [SerializeField] private EnemyLocomotion _locomotion;
    [SerializeField] private EnemyStriker _striker;
    [SerializeField] private EnemyDeath _death;

    private State _state = State.Patrol;
    private Animator _animator;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();

        if (_death != null)
        {
            _death.Died += OnEnemyDied;
        }
    }

    private void OnDestroy()
    {
        if (_death != null)
        {
            _death.Died -= OnEnemyDied;
        }
    }

    private void Start()
    {
        if (_playerTarget == null)
        {
            Debug.LogError(
                $"EnemyAwareness {name}: playerTarget not assigned. Enemy will patrol without chasing.",
                gameObject);
        }
    }

    private void Update()
    {
        if (_state == State.Dead)
        {
            return;
        }

        if (_animator != null && _locomotion != null)
        {
            _animator.SetFloat(SpeedHash, Mathf.Abs(_locomotion.FacingDirection));
        }
    }

    private void FixedUpdate()
    {
        if (_state == State.Dead)
        {
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

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(
            position + Vector3.up * (_striker != null ? _striker.AttackRange * 0.5f : 0.5f),
            _striker != null ? _striker.AttackRange : 1f);
    }

    private void OnEnemyDied(EnemyDeath death)
    {
        _state = State.Dead;

        if (_locomotion != null)
        {
            _locomotion.Stop();
        }

        if (_striker != null)
        {
            _striker.Reset();
        }

        this.enabled = false;
    }

    private void TickPatrol()
    {
        float playerPositionX = (_playerTarget != null)
            ? _playerTarget.position.x
            : Mathf.Infinity;

        float distanceToPlayer = playerPositionX - transform.position.x;

        if (_playerTarget != null && Mathf.Abs(distanceToPlayer) <= _detectRange)
        {
            _state = State.Chase;
            return;
        }

        if (_locomotion != null)
        {
            _locomotion.Patrol();
        }
    }

    private void TickChase()
    {
        const float PatrolMargin = 2f;

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

        float leftX = _locomotion.LeftBoundary;
        float rightX = _locomotion.RightBoundary;

        bool isOutsidePatrolZone = transform.position.x < leftX - PatrolMargin
                                || transform.position.x > rightX + PatrolMargin;

        if (isOutsidePatrolZone)
        {
            _state = State.Patrol;
            return;
        }

        bool isPlayerFarOutside = _playerTarget.position.x < leftX - PatrolMargin
                               || _playerTarget.position.x > rightX + PatrolMargin;

        if (isPlayerFarOutside)
        {
            _state = State.Patrol;
            return;
        }

        if (absoluteDistance <= (_striker != null ? _striker.AttackRange : 0f))
        {
            _state = State.Attack;
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

        if (_animator != null && _striker.IsOnCooldown == false)
        {
            _striker.BeginWindup();
            _animator.SetTrigger(AttackTriggerHash);
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
}
