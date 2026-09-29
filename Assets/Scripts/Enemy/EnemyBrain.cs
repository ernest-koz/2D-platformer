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
public class EnemyBrain : MonoBehaviour, IStompable, ITargetable
{
    private const float DeathVelocityY = -9f;

    [SerializeField, Min(0f)] private float _invincibilityDuration = 1f;

    private Health _health;
    private EnemyStrike _strike;
    private EnemyTargeting _targeting;
    private EnemyPatrol _patrol;
    private EnemyChase _chase;
    private SpriteFacing _facing;
    private Mover _mover;
    private GroundDetector _ground;
    private EnemyAnimator _animator;
    private Rigidbody2D _rigidbody;
    private Collider2D _collider;
    private Invincibility _invincibility;
    private State _state = State.Patrol;
    private bool _isSuspended;

    public event Action<EnemyBrain> Died;

    public bool IsAvailable
    {
        get
        {
            if (_isSuspended)
            {
                return false;
            }

            return _health.IsAlive;
        }
    }

    public Vector3 Position => transform.position;
    public bool IsTargetable => IsAvailable;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _strike = GetComponent<EnemyStrike>();
        _targeting = GetComponent<EnemyTargeting>();
        _patrol = GetComponent<EnemyPatrol>();
        _chase = GetComponent<EnemyChase>();
        _facing = GetComponent<SpriteFacing>();
        _mover = GetComponent<Mover>();
        _ground = GetComponent<GroundDetector>();
        _animator = new EnemyAnimator(GetComponent<Animator>());
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _invincibility = new Invincibility(_invincibilityDuration);
    }

    private void OnEnable()
    {
        _health.Died += OnHealthDepleted;
    }

    private void Update()
    {
        if (_isSuspended)
        {
            return;
        }

        if (_health.IsAlive == false)
        {
            return;
        }

        _invincibility.Tick(Time.deltaTime);
        _animator.SetSpeed(Mathf.Abs(_rigidbody.velocity.x));
    }

    private void FixedUpdate()
    {
        if (_isSuspended)
        {
            return;
        }

        if (_health.IsAlive == false)
        {
            return;
        }

        _ground.Refresh();
        float deltaTime = Time.fixedDeltaTime;
        _strike.TickCooldown(deltaTime);

        switch (_state)
        {
            case State.Patrol:
                TickPatrol(deltaTime);
                break;

            case State.Chase:
                TickChase(deltaTime);
                break;

            case State.Attack:
                TickAttack(deltaTime);
                break;
        }
    }

    private void OnDisable()
    {
        _health.Died -= OnHealthDepleted;
        _mover.Stop();
    }

    public void Suspend()
    {
        if (_isSuspended)
        {
            return;
        }

        _isSuspended = true;
        _strike.CancelWindup();
        _mover.Stop();
        _animator.SetSpeed(0f);
    }

    public void TakeStompDamage(int amount, Vector2 sourcePosition)
    {
        if (IsAvailable == false)
        {
            return;
        }

        _health.TakeDamage(amount, sourcePosition);
    }

    public int TakeDamage(int amount, Vector2 sourcePosition)
    {
        if (IsAvailable == false)
        {
            return 0;
        }

        return _invincibility.ApplyDamage(_health, amount, sourcePosition);
    }

    private void TickPatrol(float deltaTime)
    {
        ITargetable target = _targeting.FindNearest(_targeting.DetectRange);

        if (target == null)
        {
            _patrol.Tick(deltaTime);
            return;
        }

        _state = State.Chase;
    }

    private void TickChase(float deltaTime)
    {
        ITargetable target = _targeting.FindNearest(_targeting.ChaseRange);

        if (_chase.Tick(target, deltaTime) == false)
        {
            _state = State.Patrol;
            return;
        }

        if (IsInAttackRange(target))
        {
            _state = State.Attack;
        }
    }

    private void TickAttack(float deltaTime)
    {
        _mover.Stop();

        ITargetable target = _targeting.FindNearest(_targeting.ChaseRange);

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
            _animator.PlayAttack();
        }

        if (_strike.TickWindup(deltaTime))
        {
            _state = State.Chase;
        }
    }

    private bool IsInAttackRange(ITargetable target)
    {
        float absoluteDistance = Mathf.Abs(target.Position.x - transform.position.x);
        return absoluteDistance <= _strike.AttackRange;
    }

    private void OnHealthDepleted()
    {
        if (_isSuspended)
        {
            return;
        }

        if (_state == State.Dead)
        {
            return;
        }

        _state = State.Dead;
        _strike.CancelWindup();
        _mover.Stop();
        _collider.enabled = false;
        _rigidbody.velocity = new Vector2(0f, DeathVelocityY);
        _animator.SetSpeed(0f);
        _animator.PlayDeath();
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
