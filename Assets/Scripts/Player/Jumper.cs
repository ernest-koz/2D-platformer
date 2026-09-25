using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class Jumper : MonoBehaviour
{
    [Header("Jump")]
    [SerializeField, Min(0f)] private float _jumpForce = 15f;
    [SerializeField, Min(0f)] private float _coyoteDuration = 0.10f;
    [SerializeField, Min(0f)] private float _jumpBufferDuration = 0.12f;
    [SerializeField, Min(1f)] private float _fallMultiplier = 2.4f;
    [SerializeField, Min(1f)] private float _lowJumpMultiplier = 2f;

    private Rigidbody2D _rigidbody;
    private float _remainingJumpBufferTime;
    private float _remainingCoyoteTime;
    private bool _isJumpHeld;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void Tick(bool isGrounded, bool isJumpPressed, bool isJumpHeld, float deltaTime)
    {
        if (isJumpPressed)
        {
            _remainingJumpBufferTime = _jumpBufferDuration;
        }

        _isJumpHeld = isJumpHeld;
        _remainingJumpBufferTime -= deltaTime;
        _remainingCoyoteTime = isGrounded ? _coyoteDuration : _remainingCoyoteTime - deltaTime;
    }

    public void ApplyPhysics(float fixedDeltaTime)
    {
        ApplyVariableGravity(fixedDeltaTime);

        if (_remainingJumpBufferTime <= 0f)
        {
            return;
        }

        if (_remainingCoyoteTime <= 0f)
        {
            return;
        }

        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _jumpForce);
        _remainingJumpBufferTime = 0f;
        _remainingCoyoteTime = 0f;
    }

    private void ApplyVariableGravity(float fixedDeltaTime)
    {
        float verticalVelocity = _rigidbody.velocity.y;

        if (verticalVelocity < 0f)
        {
            verticalVelocity += Physics2D.gravity.y * (_fallMultiplier - 1f) * fixedDeltaTime;
        }
        else if (verticalVelocity > 0f)
        {
            if (_isJumpHeld)
            {
                return;
            }

            verticalVelocity += Physics2D.gravity.y * (_lowJumpMultiplier - 1f) * fixedDeltaTime;
        }

        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, verticalVelocity);
    }
}
