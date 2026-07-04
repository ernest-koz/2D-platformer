using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.5f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float moveSmoothTime = 0.08f;

    [Header("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.22f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Tuning")]
    [SerializeField] private float coyoteTime = 0.10f;
    [SerializeField] private float jumpBufferTime = 0.12f;
    [SerializeField] private float fallMultiplier = 2.4f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    private Rigidbody2D _rb;
    private Animator _animator;
    private float _horizontalInput;
    private bool _isGrounded;
    private bool _wantsJump;
    private bool _jumpHeld;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _horizontalVelSmoothing;
    private bool _facingRight = true;

    public bool IsGrounded => _isGrounded;
    public bool IsMoving => Mathf.Abs(_horizontalInput) > 0.01f;
    public bool IsFalling => _rb.velocity.y < -0.5f;
    public float VerticalVelocity => _rb.velocity.y;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        if (groundCheck == null)
        {
            var go = new GameObject("GroundCheck");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, -0.85f, 0f);
            groundCheck = go.transform;
        }
    }

    private void Update()
    {
        // === Movement input (KeyCode = layout-independent) ===
        _horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  _horizontalInput -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) _horizontalInput += 1f;

        // === Jump input (multiple keys for accessibility) ===
        bool jumpKey =
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow);
        if (jumpKey)
            _jumpBufferTimer = jumpBufferTime;

        _jumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

        // Timers
        _jumpBufferTimer -= Time.deltaTime;
        _coyoteTimer = _isGrounded ? coyoteTime : _coyoteTimer - Time.deltaTime;

        // Animator
        if (_animator != null)
        {
            _animator.SetFloat(SpeedHash, Mathf.Abs(_horizontalInput));
            _animator.SetBool(IsGroundedHash, _isGrounded);
        }

        // Flip sprite
        if (_horizontalInput > 0.01f && !_facingRight) Flip();
        else if (_horizontalInput < -0.01f && _facingRight) Flip();
    }

    private void FixedUpdate()
    {
        // Ground check
        _isGrounded = groundCheck != null &&
            Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Horizontal movement with smoothing
        float targetX = _horizontalInput * moveSpeed;
        float vx = Mathf.SmoothDamp(_rb.velocity.x, targetX, ref _horizontalVelSmoothing, moveSmoothTime);

        float vy = _rb.velocity.y;

        // Variable jump height + stronger gravity when falling
        if (vy < 0f)
            vy += Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        else if (vy > 0f && !_jumpHeld)
            vy += Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;

        // Buffered jump with coyote time
        if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
        {
            vy = jumpForce;
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
            if (_animator != null) _animator.SetTrigger(JumpTriggerHash);
        }

        _rb.velocity = new Vector2(vx, vy);
    }

    public void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    public Vector2 GetFacingDirection() => _facingRight ? Vector2.right : Vector2.left;

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
