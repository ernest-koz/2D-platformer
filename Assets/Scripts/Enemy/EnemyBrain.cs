using System;
using UnityEngine;

[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PatrolRoute))]
[RequireComponent(typeof(EnemyStrike))]
[RequireComponent(typeof(SpriteFacing))]
[RequireComponent(typeof(EnemyTargeting))]
[RequireComponent(typeof(GroundDetector))]
[RequireComponent(typeof(EnemyPatrol))]
[RequireComponent(typeof(EnemyChase))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyBrain : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DieHash = Animator.StringToHash("Die");

    private Health _health;
    private EnemyStrike _strike;
    private EnemyTargeting _targeting;
    private EnemyPatrol _patrol;
    private EnemyChase _chase;
    private SpriteFacing _facing;
    private Mover _mover;
    private Animator _animator;
    private Rigidbody2D _rigidbody;
    private Collider2D _collider;

    private State _state = State.Patrol;

    public event Action<EnemyBrain> Died;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _strike = GetComponent<EnemyStrike>();
        _targeting = GetComponent<EnemyTargeting>();
        _patrol = GetComponent<EnemyPatrol>();
        _chase = GetComponent<EnemyChase>();
        _facing = GetComponent<SpriteFacing>();
        _mover = GetComponent<Mover>();
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        _health.Died += OnDied;
    }

    private void Update()
    {
        if (_health.IsAlive)
        {
            _animator.SetFloat(SpeedHash, Mathf.Abs(_rigidbody.velocity.x));
        }
    }

    private void FixedUpdate()
    {
        if (_health.IsAlive == false)
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
        _health.Died -= OnDied;
        _mover.Stop();
    }

    private void TickPatrol()
    {
        ITargetable target = _targeting.FindNearestTarget(_targeting.DetectRange);

        if (target == null)
        {
            _patrol.Tick();
            return;
        }

        _state = State.Chase;
    }

    private void TickChase()
    {
        ITargetable target = _targeting.FindNearestTarget(_targeting.ChaseRange);

        if (_chase.Tick(target) == false)
        {
            _state = State.Patrol;
            return;
        }

        if (IsInAttackRange(target))
        {
            _state = State.Attack;
        }
    }

    private void TickAttack()
    {
        _mover.Stop();

        ITargetable target = _targeting.FindNearestTarget(_targeting.ChaseRange);

        if (target == null)
        {
            _strike.CancelWindup();
            _state = State.Patrol;
            return;
        }

        if (IsInAttackRange(target) == false)
        {
            _strike.CancelWindup();
            _state = State.Chase;
            return;
        }

        _facing.Face(target.Position.x - transform.position.x);

        if (_strike.IsOnCooldown)
        {
            return;
        }

        if (_strike.BeginWindup())
        {
            _animator.SetTrigger(AttackHash);
        }

        if (_strike.TickWindup())
        {
            _state = State.Chase;
        }
    }

    private bool IsInAttackRange(ITargetable target)
    {
        float absoluteDistance = Mathf.Abs(target.Position.x - transform.position.x);

        return absoluteDistance <= _strike.AttackRange;
    }

    private void OnDied()
    {
        _state = State.Dead;
        _mover.Stop();

        _collider.enabled = false;

        _rigidbody.velocity = new Vector2(0f, -9f);

        _animator.SetTrigger(DieHash);

        Died?.Invoke(this);
    }

    private enum State
    {
        Patrol,
        Chase,
        Attack,
        Dead
    }
}
