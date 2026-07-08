using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [FormerlySerializedAs("moveSpeed")] [SerializeField] private float _moveSpeed = 5.5f;
    [FormerlySerializedAs("jumpForce")] [SerializeField] private float _jumpForce = 15f;
    [FormerlySerializedAs("moveSmoothTime")] [SerializeField] private float _moveSmoothTime = 0.08f;

    [Header("Ground check")]
    [FormerlySerializedAs("groundCheck")] [SerializeField] private Transform _groundCheck;
    [FormerlySerializedAs("groundCheckRadius")] [SerializeField] private float _groundCheckRadius = 0.22f;
    [FormerlySerializedAs("groundLayer")] [SerializeField] private LayerMask _groundLayer;

    [Header("Tuning")]
    [FormerlySerializedAs("coyoteTime")] [SerializeField] private float _coyoteTime = 0.10f;
    [FormerlySerializedAs("jumpBufferTime")] [SerializeField] private float _jumpBufferTime = 0.12f;
    [FormerlySerializedAs("fallMultiplier")] [SerializeField] private float _fallMultiplier = 2.4f;
    [FormerlySerializedAs("lowJumpMultiplier")] [SerializeField] private float _lowJumpMultiplier = 2f;

    private const float InputDeadzone = 0.01f;
    private const float FallThreshold = 0.5f;

    private Rigidbody2D _rb;
    private Animator _animator;
    private float _horizontalInput;
    private bool _isGrounded;
    private bool _jumpHeld;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _horizontalVelSmoothing;
    private bool _facingRight = true;
    private bool _isDead;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");

    public bool IsGrounded => _isGrounded;
    public bool IsMoving => Mathf.Abs(_horizontalInput) > InputDeadzone;
    public bool IsFalling => _rb.velocity.y < -FallThreshold;
    public float VerticalVelocity => _rb.velocity.y;

    private bool IsGameplayActive =>
        GameManager.Instance == null || GameManager.Instance.State == GameState.Playing;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        if (_groundCheck == null)
        {
            var go = new GameObject("GroundCheck");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, -0.85f, 0f);
            _groundCheck = go.transform;
        }
    }

    private void Update()
    {
        if (_isDead) return;

        if (IsGameplayActive == false)
        {
            _horizontalInput = 0f;
            _jumpBufferTimer = 0f;

            if (_rb != null)
                _rb.velocity = new Vector2(0f, _rb.velocity.y);

            if (_animator != null)
                _animator.SetFloat(SpeedHash, 0f);

            return;
        }

        _horizontalInput = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  _horizontalInput -= 1f;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) _horizontalInput += 1f;

        bool jumpKey =
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow);

        if (jumpKey)
            _jumpBufferTimer = _jumpBufferTime;

        _jumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

        _jumpBufferTimer -= Time.deltaTime;
        _coyoteTimer = _isGrounded ? _coyoteTime : _coyoteTimer - Time.deltaTime;

        if (_animator != null)
        {
            _animator.SetFloat(SpeedHash, Mathf.Abs(_horizontalInput));
            _animator.SetBool(IsGroundedHash, _isGrounded);
        }

        if (_horizontalInput > InputDeadzone && _facingRight == false) Flip();
        else if (_horizontalInput < -InputDeadzone && _facingRight) Flip();
    }

    private void FixedUpdate()
    {
        if (_isDead)
        {
            _rb.velocity = Vector2.zero;
            return;
        }

        _isGrounded = _groundCheck != null &&
            Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);

        float targetX = _horizontalInput * _moveSpeed;
        float vx = Mathf.SmoothDamp(_rb.velocity.x, targetX, ref _horizontalVelSmoothing, _moveSmoothTime);

        float vy = _rb.velocity.y;

        if (vy < 0f)
            vy += Physics2D.gravity.y * (_fallMultiplier - 1f) * Time.fixedDeltaTime;
        else if (vy > 0f && _jumpHeld == false)
            vy += Physics2D.gravity.y * (_lowJumpMultiplier - 1f) * Time.fixedDeltaTime;

        if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
        {
            vy = _jumpForce;
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;

            if (_animator != null) _animator.SetTrigger(JumpTriggerHash);
        }

        _rb.velocity = new Vector2(vx, vy);
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

        if (dead && _rb != null)
            _rb.velocity = Vector2.zero;
    }

    public void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    public Vector2 GetFacingDirection() =>
        _facingRight ? Vector2.right : Vector2.left;
}
