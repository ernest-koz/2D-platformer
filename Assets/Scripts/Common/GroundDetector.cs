using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [Header("Ground check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.22f;
    [SerializeField] private LayerMask _groundLayer;

    public bool IsGrounded { get; private set; }

    public bool HasGroundAhead(float directionX)
    {
        if (_groundCheck == null)
        {
            return false;
        }

        Vector2 checkOrigin = new Vector2(
            _groundCheck.position.x + directionX * 0.85f,
            _groundCheck.position.y);

        return Physics2D.OverlapCircle(checkOrigin, _groundCheckRadius, _groundLayer) != null;
    }

    private void Awake()
    {
        if (_groundCheck != null)
        {
            return;
        }

        Debug.LogError($"GroundCheck Transform not assigned on {gameObject.name}.", gameObject);
        enabled = false;
    }

    private void FixedUpdate()
    {
        Collider2D hit = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);

        if (hit == null)
        {
            IsGrounded = false;
            return;
        }

        IsGrounded = true;
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
}
