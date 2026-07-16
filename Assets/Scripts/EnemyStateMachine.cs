using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyStateMachine : MonoBehaviour
{
    private const float AttackGizmoHeightFraction = 0.5f;
    private const float DefaultAttackGizmoRadius = 1f;

    [Header("References")]
    [SerializeField] private EnemyBehaviour _behaviour;
    [SerializeField] private EnemyTargeting _targeting;
    [SerializeField] private EnemyDeath _death;

    private State _state = State.Patrol;

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
        if (_behaviour == null)
        {
            Debug.LogError($"EnemyStateMachine {name}: Behaviour not assigned.", gameObject);
        }

        if (_targeting == null)
        {
            Debug.LogError($"EnemyStateMachine {name}: Targeting not assigned.", gameObject);
        }

        if (_death == null)
        {
            Debug.LogError($"EnemyStateMachine {name}: Death not assigned.", gameObject);
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

    private void OnDisable()
    {
        if (_death == null)
        {
            return;
        }

        _death.Died -= OnEnemyDied;

        if (_behaviour == null)
        {
            return;
        }

        _behaviour.Stop();
    }

    private void OnDrawGizmosSelected()
    {
        if (_targeting == null)
        {
            return;
        }

        Vector3 position = transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(position, _targeting.DetectRange);

        float attackRadius = _behaviour == null ? DefaultAttackGizmoRadius : _behaviour.AttackRange;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(
            position + Vector3.up * attackRadius * AttackGizmoHeightFraction,
            attackRadius);
    }

    private void TickPatrol()
    {
        ITargetable target = _targeting.FindNearestTarget(_targeting.DetectRange);

        if (target == null)
        {
            _behaviour.Patrol();
            return;
        }

        _state = State.Chase;
    }

    private void TickChase()
    {
        ITargetable target = _targeting.FindNearestTarget(_targeting.ChaseRange);

        if (target == null)
        {
            _state = State.Patrol;
            return;
        }

        if (IsInAttackRange(target))
        {
            _state = State.Attack;
            return;
        }

        if (_behaviour.HasGroundAhead == false)
        {
            _state = State.Patrol;
            return;
        }

        _behaviour.Chase(target);
    }

    private void TickAttack()
    {
        _behaviour.Stop();

        ITargetable target = _targeting.FindNearestTarget(_targeting.ChaseRange);

        if (target == null)
        {
            return;
        }

        _behaviour.FaceTowards(target);

        if (_behaviour.IsStrikerOnCooldown)
        {
            return;
        }

        _behaviour.BeginAttack();

        if (_behaviour.TickAttackWindup())
        {
            _state = State.Chase;
        }
    }

    private bool IsInAttackRange(ITargetable target)
    {
        float absoluteDistance = Mathf.Abs(target.Position.x - transform.position.x);

        return absoluteDistance <= _behaviour.AttackRange;
    }

    private void OnEnemyDied(EnemyDeath death)
    {
        _state = State.Dead;

        _behaviour.CancelStrikerWindup();
        this.enabled = false;
    }

    private enum State
    {
        Patrol,
        Chase,
        Attack,
        Dead
    }
}
