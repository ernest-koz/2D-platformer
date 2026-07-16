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

        if (_animator == null)
        {
            return;
        }

        _animator.SetFloat(SpeedHash, Mathf.Abs(_rigidbody.velocity.x));
    }

    private void OnDisable()
    {
        if (_death == null)
        {
            return;
        }

        _death.Died -= OnEnemyDied;

        if (_locomotion == null)
        {
            return;
        }

        _locomotion.Stop();
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

        float attackRadius = _striker == null ? DefaultAttackGizmoRadius : _striker.AttackRange;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(
            position + Vector3.up * attackRadius * AttackGizmoHeightFraction,
            attackRadius);
    }

    private void TickPatrol()
    {
        ITargetable target = FindNearestTarget(_detectRange);

        if (target == null)
        {
            _locomotion.Patrol();
            return;
        }

        _state = State.Chase;
    }

    private void TickChase()
    {
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
        _locomotion.Stop();

        ITargetable target = FindNearestTarget(_chaseRange);

        if (target == null)
        {
            return;
        }

        _locomotion.FaceTowards(target.Position);

        if (_striker.IsOnCooldown == false)
        {
            _striker.BeginWindup();
            TriggerAttackAnimation();
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

        if (_striker == null)
        {
            this.enabled = false;
            return;
        }

        _striker.CancelWindup();
        this.enabled = false;
    }

    private void TriggerAttackAnimation()
    {
        if (_animator == null)
        {
            return;
        }

        _animator.SetTrigger(AttackTriggerHash);
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
