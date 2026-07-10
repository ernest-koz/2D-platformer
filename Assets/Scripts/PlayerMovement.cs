using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
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
    [SerializeField] private GameSession _gameSession;
    [SerializeField] private PlayerInput _input;

    private const float InputDeadzone = 0.01f;
    private const float FallThreshold = 0.5f;
    private const float GroundCheckVerticalOffset = -0.85f;

    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private float _horizontalInput;
    private bool _isGrounded;
    private bool _jumpHeld;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _horizontalVelocitySmoothing;
    private bool _facingRight = true;
    private bool _isDead;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");

    public bool IsGrounded => _isGrounded;
    public bool IsMoving => Mathf.Abs(_horizontalInput) > InputDeadzone;
    public bool IsFalling => _rigidbody.velocity.y < -FallThreshold;
    public float VerticalVelocity => _rigidbody.velocity.y;

    private bool IsGameplayActive =>
        _gameSession == null || _gameSession.State == GameState.Playing;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        if (_input == null)
        {
            Debug.LogError($"PlayerMovement: PlayerInput not assigned on {gameObject.name}. Drag PlayerInput component in the inspector.", gameObject);
        }

        if (_groundCheck == null)
        {
            var groundCheckGameObject = new GameObject("GroundCheck");
            groundCheckGameObject.transform.SetParent(transform, false);
            groundCheckGameObject.transform.localPosition = new Vector3(0f, GroundCheckVerticalOffset, 0f);
            _groundCheck = groundCheckGameObject.transform;
        }
    }

    private void Update()
    {
        if (_isDead)
        {
            return;
        }

        if (IsGameplayActive == false)
        {
            _horizontalInput = 0f;
            _jumpBufferTimer = 0f;

            if (_rigidbody != null)
            {
                _rigidbody.velocity = new Vector2(0f, _rigidbody.velocity.y);
            }

            if (_animator != null)
            {
                _animator.SetFloat(SpeedHash, 0f);
            }

            return;
        }

        _horizontalInput = _input != null ? _input.HorizontalInput : 0f;

        if (_input != null && _input.JumpPressed)
        {
            _jumpBufferTimer = _jumpBufferTime;
        }

        _jumpHeld = _input != null && _input.JumpHeld;

        _jumpBufferTimer -= Time.deltaTime;
        _coyoteTimer = _isGrounded ? _coyoteTime : _coyoteTimer - Time.deltaTime;

        if (_animator != null)
        {
            _animator.SetFloat(SpeedHash, Mathf.Abs(_horizontalInput));
            _animator.SetBool(IsGroundedHash, _isGrounded);
        }

        if (_horizontalInput > InputDeadzone && _facingRight == false)
        {
            Flip();
        }
        else if (_horizontalInput < -InputDeadzone && _facingRight)
        {
            Flip();
        }
    }

    private void FixedUpdate()
    {
        if (_isDead)
        {
            _rigidbody.velocity = Vector2.zero;
            return;
        }

        _isGrounded = _groundCheck != null &&
            Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer | _enemyLayer);

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
        else if (verticalVelocity > 0f && _jumpHeld == false)
        {
            verticalVelocity += Physics2D.gravity.y * (_lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }

        if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
        {
            verticalVelocity = _jumpForce;
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;

            if (_animator != null)
            {
                _animator.SetTrigger(JumpTriggerHash);
            }
        }

        _rigidbody.velocity = new Vector2(horizontalVelocity, verticalVelocity);
    }

    private void OnDrawGizmosSelected()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }
    }

    public void SetDead(bool dead)
    {
        _isDead = dead;

        if (dead && _rigidbody != null)
        {
            _rigidbody.velocity = Vector2.zero;
        }
    }

    public void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    public Vector2 GetFacingDirection() =>
        _facingRight ? Vector2.right : Vector2.left;
}
