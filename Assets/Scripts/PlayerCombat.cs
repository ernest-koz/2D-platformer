using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Stomp")]
    [SerializeField] private float _stompBounceForce = 14f;
    [SerializeField] private float _stompCheckRadius = 0.35f;
    [SerializeField] private Transform _stompCheck;
    [SerializeField] private LayerMask _enemyLayer;

    private PlayerMovement _playerMovement;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (_stompCheck == null)
        {
            return;
        }

        if (_playerMovement.IsFalling == false)
        {
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(_stompCheck.position, _stompCheckRadius, _enemyLayer);

        if (hit == null || transform.position.y <= hit.bounds.max.y)
        {
            return;
        }

        EnemyDeath enemyDeath = hit.GetComponentInParent<EnemyDeath>();

        if (enemyDeath == null)
        {
            return;
        }

        if (enemyDeath.IsDead == false)
        {
            enemyDeath.Die();
            Bounce(_stompBounceForce);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_stompCheck == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_stompCheck.position, _stompCheckRadius);
    }

    private void Bounce(float force)
    {
        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, force);
    }
}
