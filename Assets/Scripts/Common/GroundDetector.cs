using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [Header("Ground check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField, Min(0.01f)] private float _groundCheckRadius = 0.22f;
    [SerializeField, Min(0f)] private float _aheadCheckDistance = 0.85f;
    [SerializeField] private LayerMask _groundLayer;

    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        if (_groundCheck == false)
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

    public bool HasGroundAhead(float directionX)
    {
        if (_groundCheck == null)
        {
            return false;
        }

        Vector2 checkOrigin = new Vector2(
            _groundCheck.position.x + directionX * _aheadCheckDistance,
            _groundCheck.position.y);

        Collider2D hit = Physics2D.OverlapCircle(checkOrigin, _groundCheckRadius, _groundLayer);

        return hit == null == false;
    }
}
