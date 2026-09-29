using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DamageKnockback : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _knockbackX = 4.5f;
    [SerializeField, Min(0f)] private float _knockbackY = 7.5f;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void Apply(Vector2 source)
    {
        Vector2 direction = ((Vector2)transform.position - source).normalized;
        _rigidbody.velocity = new Vector2(direction.x * _knockbackX, _knockbackY);
    }
}
