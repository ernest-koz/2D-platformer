using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAwareness : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
    private const float AttackGizmoHeightFraction = 0.5f;
    private const float DefaultAttackGizmoRadius = 1f;

    [Header("Detection")]
    [SerializeField] private float _detectRange = 5f;
    [SerializeField] private float _chaseRange = 7f;
    [SerializeField] private LayerMask _playerLayer;

    [Header("References")]
    [SerializeField] private EnemyLocomotion _locomotion;
    [SerializeField] private EnemyStriker _striker;
    [SerializeField] private EnemyDeath _death;

    private readonly Collider2D[] _targetBuffer = new Collider2D[8];
    private State _state = State.Patrol;
    private Animator _animator;
    private Rigidbody2D _rigidbody;

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

    private void Start()
    {
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

    private void Update()
    {
        if (_state == State.Dead)
        {
            return;
        }

        _animator?.SetFloat(SpeedHash, Mathf.Abs(_rigidbody.velocity.x));
    }

    private void OnDisable()
    {
        if (_death != null)
        {
            _death.Died -= OnEnemyDied;
        }

        _locomotion?.Stop();
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

    private void TickPatrol()
    {
        ITargetable target = FindNearestTarget(_detectRange);

        if (target != null)
        {
            _state = State.Chase;
            return;
        }

        _locomotion?.Patrol();
    }

    private void TickChase()
    {
        if (_locomotion == null || _striker == null)
        {
            _state = State.Patrol;
            return;
        }

        ITargetable target = FindNearestTarget(_chaseRange);

        if (target == null)
        {
            _state = State.Patrol;
            return;
        }

        float absoluteDistance = Mathf.Abs(target.Position.x - transform.position.x);

        if (absoluteDistance <= _striker.AttackRange)
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

        _locomotion.Chase(target.Position);
    }

    private void TickAttack()
    {
        if (_locomotion == null || _striker == null)
        {
            _state = State.Patrol;
            return;
        }

        _locomotion.Stop();

        ITargetable target = FindNearestTarget(_chaseRange);

        if (target != null)
        {
            _locomotion.FaceTowards(target.Position);
        }

        if (_striker.IsOnCooldown == false)
        {
            _striker.BeginWindup();

            _animator?.SetTrigger(AttackTriggerHash);
        }

        bool isAttackCompleted = _striker.TickWindup(
            transform.position,
            _locomotion.FacingVector,
            _playerLayer);

        if (isAttackCompleted)
        {
            _state = State.Chase;
        }
    }

    private void OnEnemyDied(EnemyDeath death)
    {
        _state = State.Dead;

        _striker?.CancelWindup();
        this.enabled = false;
    }

    private ITargetable FindNearestTarget(float range)
    {
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, range, _targetBuffer, _playerLayer);

        ITargetable nearest = null;
        float nearestSqrDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (_targetBuffer[i].TryGetComponent(out ITargetable target) == false)
            {
                continue;
            }

            if (target.IsTargetable == false)
            {
                continue;
            }

            float sqrDistance = (target.Position - transform.position).sqrMagnitude;

            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = target;
            }
        }

        return nearest;
    }

    private enum State
    {
        Patrol,
        Chase,
        Attack,
        Dead
    }
}
