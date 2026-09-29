using System;
using UnityEngine;

[RequireComponent(typeof(InputReader))]
[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(Jumper))]
[RequireComponent(typeof(GroundDetector))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(SpriteFacing))]
[RequireComponent(typeof(PlayerStomp))]
[RequireComponent(typeof(PlayerCollision))]
[RequireComponent(typeof(FallDetector))]
[RequireComponent(typeof(DamageKnockback))]
[RequireComponent(typeof(DamageFlicker))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(HealthUI))]
[RequireComponent(typeof(Vampirism))]
public class Player : MonoBehaviour, ITargetable
{
    private const int ContactDamage = 1;
    private const float MovementInputThreshold = 0.01f;

    [SerializeField, Min(0f)] private float _invincibilityDuration = 1f;

    private InputReader _input;
    private Mover _mover;
    private Jumper _jumper;
    private GroundDetector _ground;
    private PlayerCollision _collision;
    private PlayerStomp _stomp;
    private FallDetector _fallDetector;
    private Health _health;
    private SpriteFacing _facing;
    private DamageKnockback _knockback;
    private DamageFlicker _flicker;
    private PlayerAnimator _animator;
    private HealthUI _healthUI;
    private Vampirism _vampirism;
    private Invincibility _invincibility;
    private bool _hasDied;
    private bool _isSuspended;
    private bool _hasFinished;

    public event Action<Pickup> PickupContacted;
    public event Action LevelFinished;
    public event Action Died;
    public event Action RestartRequested;

    public Vector3 Position => transform.position;
    public bool IsTargetable => IsInactive() == false;

    private void Awake()
    {
        _input = GetComponent<InputReader>();
        _mover = GetComponent<Mover>();
        _jumper = GetComponent<Jumper>();
        _ground = GetComponent<GroundDetector>();
        _collision = GetComponent<PlayerCollision>();
        _stomp = GetComponent<PlayerStomp>();
        _fallDetector = GetComponent<FallDetector>();
        _health = GetComponent<Health>();
        _facing = GetComponent<SpriteFacing>();
        _knockback = GetComponent<DamageKnockback>();
        _flicker = GetComponent<DamageFlicker>();
        _animator = GetComponent<PlayerAnimator>();
        _healthUI = GetComponent<HealthUI>();
        _vampirism = GetComponent<Vampirism>();
        _invincibility = new Invincibility(_invincibilityDuration);
    }

    private void OnEnable()
    {
        _collision.TriggerEntered += OnTriggerEntered;
        _collision.CollisionEntered += OnCollisionEntered;
        _fallDetector.FellToDeath += OnFellToDeath;
        _health.Changed += OnHealthChanged;
        _health.Damaged += OnDamaged;
        _health.Died += OnHealthDepleted;
        _vampirism.Drained += OnVampirismDrained;
        _invincibility.Changed += OnInvincibilityChanged;
    }

    private void Start()
    {
        _healthUI.Render(_health.Current, _health.Maximum);
    }

    private void Update()
    {
        _input.Read();

        if (_input.IsRestartPressed)
        {
            RestartRequested?.Invoke();
        }

        if (IsInactive())
        {
            return;
        }

        _invincibility.Tick(Time.deltaTime);
        _fallDetector.Check();

        if (_hasDied)
        {
            return;
        }

        if (_input.IsVampirismPressed)
        {
            _vampirism.Activate();
        }

        float direction = _input.Direction;

        _jumper.Tick(_ground.IsGrounded, _input.IsJumpPressed, _input.IsJumpHeld, Time.deltaTime);
        _animator.SetMovement(Mathf.Abs(direction), _ground.IsGrounded);
        _flicker.Tick(Time.time);

        if (Mathf.Abs(direction) <= MovementInputThreshold)
        {
            return;
        }

        _facing.Face(direction);
    }

    private void FixedUpdate()
    {
        if (IsInactive())
        {
            _mover.Stop();
            return;
        }

        _ground.Refresh();
        _stomp.TryStomp();
        _mover.Move(_input.Direction, Time.fixedDeltaTime);
        _jumper.ApplyPhysics(Time.fixedDeltaTime);
    }

    private void OnDisable()
    {
        _collision.TriggerEntered -= OnTriggerEntered;
        _collision.CollisionEntered -= OnCollisionEntered;
        _fallDetector.FellToDeath -= OnFellToDeath;
        _health.Changed -= OnHealthChanged;
        _health.Damaged -= OnDamaged;
        _health.Died -= OnHealthDepleted;
        _vampirism.Drained -= OnVampirismDrained;
        _invincibility.Changed -= OnInvincibilityChanged;
    }

    public bool Heal(int amount)
    {
        if (IsInactive())
        {
            return false;
        }

        return _health.ReceiveHealing(amount);
    }

    public int TakeDamage(int amount, Vector2 sourcePosition)
    {
        if (IsInactive())
        {
            return 0;
        }

        return _invincibility.ApplyDamage(_health, amount, sourcePosition);
    }

    public void Suspend()
    {
        if (_isSuspended)
        {
            return;
        }

        _isSuspended = true;
        _input.Block();
        _mover.Stop();
        _vampirism.Interrupt();
        _flicker.StopFlickering();
        _animator.SetMovement(0f, _ground.IsGrounded);
    }

    private void OnTriggerEntered(Collider2D other)
    {
        if (IsInactive())
        {
            return;
        }

        if (_hasFinished)
        {
            return;
        }

        if (other.TryGetComponent(out Pickup pickup))
        {
            PickupContacted?.Invoke(pickup);
            return;
        }

        if (other.TryGetComponent(out FinishTrigger _))
        {
            _hasFinished = true;
            LevelFinished?.Invoke();
        }
    }

    private void OnCollisionEntered(Collision2D collision)
    {
        if (IsInactive())
        {
            return;
        }

        if (collision.collider.TryGetComponent(out IStompable enemy) == false)
        {
            return;
        }

        if (enemy.IsAvailable == false)
        {
            return;
        }

        if (_stomp.CanStomp(collision.collider))
        {
            return;
        }

        TakeDamage(ContactDamage, collision.transform.position);
    }

    private bool IsInactive()
    {
        if (_isSuspended)
        {
            return true;
        }

        if (_hasDied)
        {
            return true;
        }

        return false;
    }

    private void OnHealthChanged(int current, int maximum)
    {
        _healthUI.Render(current, maximum);
    }

    private void OnDamaged(Vector2 source)
    {
        _animator.PlayHurt();
        _knockback.Apply(source);
    }

    private void OnInvincibilityChanged(bool isInvincible)
    {
        if (isInvincible)
        {
            _flicker.StartFlickering();
            return;
        }

        _flicker.StopFlickering();
    }

    private void OnHealthDepleted()
    {
        Die();
    }

    private void OnFellToDeath()
    {
        Die();
    }

    private void Die()
    {
        if (_hasDied)
        {
            return;
        }

        _hasDied = true;
        _input.Block();
        _mover.Stop();
        _vampirism.Interrupt();
        _flicker.StopFlickering();
        _animator.PlayDeath();
        Died?.Invoke();
    }

    private void OnVampirismDrained(int amount)
    {
        Heal(amount);
    }
}
