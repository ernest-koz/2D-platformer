using UnityEngine;

[RequireComponent(typeof(InputReader))]
[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(GroundDetector))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(SpriteFacing))]
[RequireComponent(typeof(PlayerStomp))]
[RequireComponent(typeof(PlayerCollisionHandler))]
[RequireComponent(typeof(FallDetector))]
public class Player : MonoBehaviour
{
    [Header("Jump")]
    [SerializeField] private float _jumpForce = 15f;
    [SerializeField] private float _coyoteTime = 0.10f;
    [SerializeField] private float _jumpBufferTime = 0.12f;
    [SerializeField] private float _fallMultiplier = 2.4f;
    [SerializeField] private float _lowJumpMultiplier = 2f;

    private InputReader _input;
    private Mover _mover;
    private GroundDetector _ground;
    private Health _health;
    private SpriteFacing _facing;
    private Rigidbody2D _rigidbody;
    private Animator _animator;

    private int _speedHash;
    private int _isGroundedHash;
    private float _jumpBufferTimer;
    private float _coyoteTimer;
    private bool _isJumpHeld;
    private bool _isDead;

    public bool IsAlive => _isDead == false;

    private void Awake()
    {
        _input = GetComponent<InputReader>();
        _mover = GetComponent<Mover>();
        _ground = GetComponent<GroundDetector>();
        _health = GetComponent<Health>();
        _facing = GetComponent<SpriteFacing>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        _speedHash = Animator.StringToHash("Speed");
        _isGroundedHash = Animator.StringToHash("IsGrounded");
    }

    private void OnEnable()
    {
        _health.Died += OnDied;
    }

    private void OnDisable()
    {
        _health.Died -= OnDied;
    }

    private void Update()
    {
        if (_isDead)
        {
            return;
        }

        float direction = _input.Direction;

        ApplyTimers(direction);
        UpdateAnimator(direction);

        if (Mathf.Abs(direction) > 0.01f)
        {
            _facing.Face(direction);
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

        ApplyJumpPhysics();

        if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
        {
            _mover.Jump(_jumpForce);
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
        }
    }

    private void ApplyTimers(float direction)
    {
        if (_input.IsJumpPressed)
        {
            _jumpBufferTimer = _jumpBufferTime;
        }

        _isJumpHeld = _input.IsJumpHeld;
        _jumpBufferTimer -= Time.deltaTime;
        _coyoteTimer = _ground.IsGrounded ? _coyoteTime : _coyoteTimer - Time.deltaTime;
    }

    private void ApplyJumpPhysics()
    {
        float verticalVelocity = _rigidbody.velocity.y;

        if (verticalVelocity < 0f)
        {
            verticalVelocity += Physics2D.gravity.y * (_fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (verticalVelocity > 0f && _isJumpHeld == false)
        {
            verticalVelocity += Physics2D.gravity.y * (_lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }

        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, verticalVelocity);
    }

    private void UpdateAnimator(float direction)
    {
        if (_animator == null)
        {
            return;
        }

        _animator.SetFloat(_speedHash, Mathf.Abs(direction));
        _animator.SetBool(_isGroundedHash, _ground.IsGrounded);
    }

    private void OnDied()
    {
        _isDead = true;
        _mover.Stop();
    }
}
