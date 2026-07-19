using UnityEngine;

[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(Rigidbody2D))]
public class Jumper : MonoBehaviour
{
    [Header("Jump")]
    [SerializeField] private float _jumpForce = 15f;
    [SerializeField] private float _coyoteTime = 0.10f;
    [SerializeField] private float _jumpBufferTime = 0.12f;
    [SerializeField] private float _fallMultiplier = 2.4f;
    [SerializeField] private float _lowJumpMultiplier = 2f;

    private Mover _mover;
    private Rigidbody2D _rigidbody;
    private float _jumpBufferTimer;
    private float _coyoteTimer;
    private bool _isJumpHeld;

    public void Tick(bool isGrounded, bool isJumpPressed, bool isJumpHeld, float deltaTime)
    {
        if (isJumpPressed)
        {
            _jumpBufferTimer = _jumpBufferTime;
        }

        _isJumpHeld = isJumpHeld;
        _jumpBufferTimer -= deltaTime;
        _coyoteTimer = isGrounded ? _coyoteTime : _coyoteTimer - deltaTime;
    }

    public void ApplyPhysics(float fixedDeltaTime)
    {
        ApplyVariableGravity(fixedDeltaTime);

        if (_jumpBufferTimer <= 0f || _coyoteTimer <= 0f)
        {
            return;
        }

        _mover.Jump(_jumpForce);
        _jumpBufferTimer = 0f;
        _coyoteTimer = 0f;
    }

    private void Awake()
    {
        _mover = GetComponent<Mover>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void ApplyVariableGravity(float fixedDeltaTime)
    {
        float verticalVelocity = _rigidbody.velocity.y;

        if (verticalVelocity < 0f)
        {
            verticalVelocity += Physics2D.gravity.y * (_fallMultiplier - 1f) * fixedDeltaTime;
        }
        else if (verticalVelocity > 0f && _isJumpHeld == false)
        {
            verticalVelocity += Physics2D.gravity.y * (_lowJumpMultiplier - 1f) * fixedDeltaTime;
        }

        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, verticalVelocity);
    }
}
