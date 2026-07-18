using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody2D))]
public class DamageKnockback : MonoBehaviour
{
    [SerializeField] private float _knockbackX = 4.5f;
    [SerializeField] private float _knockbackY = 7.5f;

    private Health _health;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        _health.Damaged += OnDamaged;
    }

    private void OnDisable()
    {
        _health.Damaged -= OnDamaged;
    }

    private void OnDamaged(Vector2 source)
    {
        Vector2 direction = ((Vector2)transform.position - source).normalized;
        _rigidbody.velocity = new Vector2(direction.x * _knockbackX, _knockbackY);
    }
}
