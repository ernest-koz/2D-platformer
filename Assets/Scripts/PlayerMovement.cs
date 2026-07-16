using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    private const float InputDeadzone = 0.01f;
    private const float FallThreshold = 0.5f;
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5.5f;
    [SerializeField] private float _jumpForce = 15f;
    [SerializeField] private float _moveSmoothTime = 0.08f;

    [Header("Ground check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.22f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _enemyLayer;

    [Header("Tuning")]
    [SerializeField] private float _coyoteTime = 0.10f;
    [SerializeField] private float _jumpBufferTime = 0.12f;
    [SerializeField] private float _fallMultiplier = 2.4f;
    [SerializeField] private float _lowJumpMultiplier = 2f;

    [Header("References")]
    [SerializeField] private PlayerInput _input;
    [SerializeField] private SpriteFacing _facing;

    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private float _horizontalInput;
    private bool _isGrounded;
    private bool _isJumpHeld;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _horizontalVelocitySmoothing;
    private bool _isDead;

    public bool IsGrounded => _isGrounded;
    public bool IsMoving => Mathf.Abs(_horizontalInput) > InputDeadzone;
    public bool IsFalling => _rigidbody.velocity.y < -FallThreshold;
    public float VerticalVelocity => _rigidbody.velocity.y;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        if (_facing == null)
        {
            _facing = GetComponent<SpriteFacing>();
        }

        if (_input == null)
        {
            Debug.LogError($"PlayerMovement: PlayerInput not assigned on {gameObject.name}. Drag PlayerInput component in the inspector.", gameObject);
            this.enabled = false;
            return;
        }

        if (_groundCheck == null)
        {
            Debug.LogError($"PlayerMovement: GroundCheck not assigned on {gameObject.name}. Assign a Transform child for ground detection.", gameObject);
            this.enabled = false;
            return;
        }
    }

    private void FixedUpdate()
    {
        if (_isDead)
        {
            _rigidbody.velocity = Vector2.zero;
            return;
        }

        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer | _enemyLayer);

        float targetHorizontalVelocity = _horizontalInput * _moveSpeed;
        float horizontalVelocity = Mathf.SmoothDamp(
            _rigidbody.velocity.x,
            targetHorizontalVelocity,
            ref _horizontalVelocitySmoothing,
            _moveSmoothTime);

        float verticalVelocity = _rigidbody.velocity.y;

        if (verticalVelocity < 0f)
        {
            verticalVelocity += Physics2D.gravity.y * (_fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (verticalVelocity > 0f && _isJumpHeld == false)
        {
            verticalVelocity += Physics2D.gravity.y * (_lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }

        if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
        {
            verticalVelocity = _jumpForce;
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;

            _animator.SetTrigger(JumpTriggerHash);
        }

        _rigidbody.velocity = new Vector2(horizontalVelocity, verticalVelocity);
    }

    private void Update()
    {
        if (_isDead)
        {
            return;
        }

        _horizontalInput = _input.HorizontalInput;

        if (_input.IsJumpPressed)
        {
            _jumpBufferTimer = _jumpBufferTime;
        }

        _isJumpHeld = _input.IsJumpHeld;

        _jumpBufferTimer -= Time.deltaTime;
        _coyoteTimer = _isGrounded ? _coyoteTime : _coyoteTimer - Time.deltaTime;

        _animator.SetFloat(SpeedHash, Mathf.Abs(_horizontalInput));
        _animator.SetBool(IsGroundedHash, _isGrounded);

        if (Mathf.Abs(_horizontalInput) > InputDeadzone)
        {
            _facing.Face(_horizontalInput);
        }
    }

    private void OnDisable()
    {
        ResetRigidbodyVelocity();

        _horizontalInput = 0f;
        _jumpBufferTimer = 0f;
        _isJumpHeld = false;
    }

    private void ResetRigidbodyVelocity()
    {
        if (_rigidbody == null)
        {
            return;
        }

        _rigidbody.velocity = new Vector2(0f, _rigidbody.velocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        if (_groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
    }

    public void SetDead(bool isDead)
    {
        _isDead = isDead;

        if (isDead)
        {
            _rigidbody.velocity = Vector2.zero;
        }
    }
}
