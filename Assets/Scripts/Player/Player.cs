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
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
    private const float StompHeightThreshold = 0.4f;
    private const float FallSpeedThreshold = 0.5f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

    private InputReader _input;
    private Mover _mover;
    private Jumper _jumper;
    private GroundDetector _ground;
    private PlayerCollision _collision;
    private FallDetector _fallDetector;
    private Health _health;
    private SpriteFacing _facing;
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private bool _isDead;

    public event Action<Pickup> PickupContacted;
    public event Action LevelFinished;
    public event Action<Collision2D> EnemyContacted;

    private void Awake()
    {
        _input = GetComponent<InputReader>();
        _mover = GetComponent<Mover>();
        _jumper = GetComponent<Jumper>();
        _ground = GetComponent<GroundDetector>();
        _collision = GetComponent<PlayerCollision>();
        _fallDetector = GetComponent<FallDetector>();
        _health = GetComponent<Health>();
        _facing = GetComponent<SpriteFacing>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        _collision.TriggerEntered += OnTriggerEntered;
        _collision.CollisionEntered += OnCollisionEntered;
        _health.Died += OnDied;
        _fallDetector.FellToDeath += OnDied;
    }

    private void Start()
    {
        if (_input == null)
        {
            Debug.LogError($"InputReader not found on {gameObject.name}.", gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (_isDead)
        {
            _mover.Stop();
            return;
        }

        _mover.Move(_input.Direction);
        _jumper.ApplyPhysics(Time.fixedDeltaTime);
    }

    private void Update()
    {
        if (_isDead)
        {
            return;
        }

        float direction = _input.Direction;

        _jumper.Tick(_ground.IsGrounded, _input.IsJumpPressed, _input.IsJumpHeld, Time.deltaTime);
        UpdateAnimator(direction);

        if (Mathf.Abs(direction) > 0.01f)
        {
            _facing.Face(direction);
        }
    }

    private void OnDisable()
    {
        _collision.TriggerEntered -= OnTriggerEntered;
        _collision.CollisionEntered -= OnCollisionEntered;
        _health.Died -= OnDied;
        _fallDetector.FellToDeath -= OnDied;
    }

    private void OnTriggerEntered(Collider2D other)
    {
        if (other.TryGetComponent(out Pickup pickup))
        {
            PickupContacted?.Invoke(pickup);
            return;
        }

        if (other.TryGetComponent(out FinishTrigger _))
        {
            LevelFinished?.Invoke();
        }
    }

    private void OnCollisionEntered(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out Health enemyHealth) == false)
        {
            return;
        }

        if (enemyHealth.IsAlive == false)
        {
            return;
        }

        bool isStomp =
            transform.position.y > collision.transform.position.y + StompHeightThreshold &&
            _rigidbody.velocity.y < -FallSpeedThreshold;

        if (isStomp == false)
        {
            EnemyContacted?.Invoke(collision);
        }
    }

    private void UpdateAnimator(float direction)
    {
        _animator.SetFloat(SpeedHash, Mathf.Abs(direction));
        _animator.SetBool(IsGroundedHash, _ground.IsGrounded);
    }

    private void OnDied()
    {
        _isDead = true;
        _mover.Stop();
    }
}
