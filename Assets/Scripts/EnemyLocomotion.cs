using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyLocomotion : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float _patrolSpeed = 1.6f;
    [SerializeField] private float _leftX = -3f;
    [SerializeField] private float _rightX = 3f;
    [SerializeField] private bool _startFacingRight = true;

    [Header("Chase")]
    [SerializeField] private float _chaseSpeed = 2.8f;

    [Header("Ground check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.2f;
    [SerializeField] private float _edgeCheckOffset = 0.25f;
    [SerializeField] private LayerMask _groundLayer;

    private Rigidbody2D _rigidbody;
    private int _direction;

    public int FacingDirection => _direction;
    public Vector2 FacingVector => _direction > 0 ? Vector2.right : Vector2.left;
    public float LeftBoundary => _leftX;
    public float RightBoundary => _rightX;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _direction = _startFacingRight ? 1 : -1;

        if (_groundCheck == null)
        {
            Debug.LogError(
                $"EnemyLocomotion: GroundCheck not assigned on {gameObject.name}. Edge detection disabled — enemy may fall off platforms.",
                gameObject);
        }
    }

    public void Patrol()
    {
        if (_direction == 0)
        {
            _direction = _startFacingRight ? 1 : -1;
        }

        _rigidbody.velocity = new Vector2(_direction * _patrolSpeed, _rigidbody.velocity.y);

        float currentX = transform.position.x;

        if (_direction > 0 && currentX >= _rightX)
        {
            Flip();
        }
        else if (_direction < 0 && currentX <= _leftX)
        {
            Flip();
        }
        else if (HasGroundAhead() == false)
        {
            Flip();
        }
    }

    public void Chase(Vector3 targetPosition)
    {
        if (_direction == 0)
        {
            _direction = _startFacingRight ? 1 : -1;
        }

        float distanceToTarget = targetPosition.x - transform.position.x;
        int targetDirection = distanceToTarget > 0 ? 1 : -1;

        if (targetDirection != _direction)
        {
            Flip();
        }

        _rigidbody.velocity = new Vector2(_direction * _chaseSpeed, _rigidbody.velocity.y);
    }

    public void Stop()
    {
        _rigidbody.velocity = new Vector2(0f, _rigidbody.velocity.y);
    }

    public void Flip()
    {
        _direction *= -1;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    public void FaceTowards(Vector3 targetPosition)
    {
        float distanceToTarget = targetPosition.x - transform.position.x;
        int targetDirection = distanceToTarget > 0 ? 1 : -1;

        if (targetDirection != _direction)
        {
            Flip();
        }
    }

    public bool HasGroundAhead()
    {
        if (_groundCheck == null)
        {
            return true;
        }

        Collider2D groundUnder = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);

        if (groundUnder == null)
        {
            return true;
        }

        Vector2 checkPosition = (Vector2)_groundCheck.position + FacingVector * _edgeCheckOffset;

        return Physics2D.OverlapCircle(checkPosition, _groundCheckRadius, _groundLayer) != null;
    }

    private void OnDrawGizmos()
    {
        if (_groundCheck == null)
        {
            return;
        }

        bool hasGround = HasGroundAhead();

        Gizmos.color = hasGround ? Color.green : Color.red;
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);

        Vector2 checkPosition = (Vector2)_groundCheck.position + FacingVector * _edgeCheckOffset;

        Gizmos.color = hasGround ? Color.green : Color.red;
        Gizmos.DrawWireSphere(checkPosition, _groundCheckRadius);
    }
}
