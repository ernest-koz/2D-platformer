using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(Rigidbody2D))]
public class DamageKnockback : MonoBehaviour
{
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private float _knockbackX = 4.5f;
    [SerializeField] private float _knockbackY = 7.5f;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        if (_playerHealth == null)
        {
            _playerHealth = GetComponent<PlayerHealth>();
        }

        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        _playerHealth.Damaged += OnDamaged;
    }

    private void OnDisable()
    {
        _playerHealth.Damaged -= OnDamaged;
    }

    private void OnDamaged(Vector2 source)
    {
        Vector2 direction = ((Vector2)transform.position - source).normalized;
        _rigidbody.velocity = new Vector2(direction.x * _knockbackX, _knockbackY);
    }
}
