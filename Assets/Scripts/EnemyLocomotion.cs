using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteFacing))]
public class EnemyLocomotion : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float _patrolSpeed = 1.6f;
    [SerializeField] private float _leftX = -3f;
    [SerializeField] private float _rightX = 3f;

    [Header("Chase")]
    [SerializeField] private float _chaseSpeed = 2.8f;

    [Header("Ground check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.2f;
    [SerializeField] private float _edgeCheckOffset = 0.5f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("References")]
    [SerializeField] private SpriteFacing _facing;

    private Rigidbody2D _rigidbody;

    public Vector2 FacingVector => _facing.FacingVector;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        if (_facing == null)
        {
            _facing = GetComponent<SpriteFacing>();
        }

        if (_groundCheck == null)
        {
            Debug.LogError(
                $"EnemyLocomotion: GroundCheck not assigned on {gameObject.name}. Edge detection disabled — enemy may fall off platforms.",
                gameObject);
        }
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

    public void Patrol()
    {
        _rigidbody.velocity = new Vector2(_facing.FacingDirection * _patrolSpeed, _rigidbody.velocity.y);

        float currentX = transform.position.x;

        if (_facing.FacingDirection > 0 && currentX >= _rightX)
        {
            _facing.Flip();
        }
        else if (_facing.FacingDirection < 0 && currentX <= _leftX)
        {
            _facing.Flip();
        }
        else if (HasGroundAhead() == false)
        {
            _facing.Flip();
        }
    }

    public void Chase(Vector3 targetPosition)
    {
        float distanceToTarget = targetPosition.x - transform.position.x;
        _facing.Face(distanceToTarget);

        _rigidbody.velocity = new Vector2(_facing.FacingDirection * _chaseSpeed, _rigidbody.velocity.y);
    }

    public void Stop()
    {
        _rigidbody.velocity = new Vector2(0f, _rigidbody.velocity.y);
    }

    public void FaceTowards(Vector3 targetPosition)
    {
        float distanceToTarget = targetPosition.x - transform.position.x;
        _facing.Face(distanceToTarget);
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

        if (Physics2D.OverlapCircle(checkPosition, _groundCheckRadius, _groundLayer) == null)
        {
            return false;
        }

        return true;
    }
}
