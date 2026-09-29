using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerStomp : MonoBehaviour
{
    [Header("Stomp")]
    [SerializeField, Min(0f)] private float _bounceForce = 14f;
    [SerializeField, Min(0f)] private float _checkRadius = 0.35f;
    [SerializeField, Min(1)] private int _damage = 1;
    [SerializeField] private LayerMask _enemyLayer;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnValidate()
    {
        if (_enemyLayer.value == 0)
        {
            Debug.LogError($"{nameof(PlayerStomp)} enemy layer not assigned on {gameObject.name}.", gameObject);
        }
    }

    public bool CanStomp(Collider2D enemyCollider)
    {
        if (IsStompHeight(enemyCollider) == false)
        {
            return false;
        }

        return IsFallingDown();
    }

    public void TryStomp()
    {
        if (IsFallingDown() == false)
        {
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, _checkRadius, _enemyLayer);

        if (hit == null)
        {
            return;
        }

        if (IsStompHeight(hit) == false)
        {
            return;
        }

        if (hit.TryGetComponent(out IStompable enemy) == false)
        {
            return;
        }

        if (enemy.IsAvailable == false)
        {
            return;
        }

        enemy.TakeStompDamage(_damage, transform.position);

        Bounce();
    }

    private bool IsStompHeight(Collider2D enemyCollider)
    {
        return transform.position.y > enemyCollider.bounds.max.y;
    }

    private bool IsFallingDown()
    {
        return _rigidbody.velocity.y < 0f;
    }

    private void Bounce()
    {
        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _bounceForce);
    }
}
