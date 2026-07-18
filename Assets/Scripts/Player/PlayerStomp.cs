using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerStomp : MonoBehaviour
{
    [Header("Stomp")]
    [SerializeField] private float _bounceForce = 14f;
    [SerializeField] private float _checkRadius = 0.35f;
    [SerializeField] private LayerMask _enemyLayer;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (_rigidbody.velocity.y >= 0f)
        {
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, _checkRadius, _enemyLayer);

        if (hit == null)
        {
            return;
        }

        if (transform.position.y <= hit.bounds.max.y)
        {
            return;
        }

        if (hit.TryGetComponent(out Health enemyHealth) == false)
        {
            return;
        }

        if (enemyHealth.IsAlive == false)
        {
            return;
        }

        enemyHealth.TakeDamage(enemyHealth.MaximumHealth, transform.position);

        Bounce();
    }

    private void Bounce()
    {
        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _bounceForce);
    }
}
