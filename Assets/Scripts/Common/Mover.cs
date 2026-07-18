using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Mover : MonoBehaviour
{
    [Header("Tuning")]
    [SerializeField] private float _moveSpeed = 5.5f;
    [SerializeField] private float _smoothTime = 0.08f;

    private Rigidbody2D _rigidbody;
    private float _velocitySmoothing;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void Move(float direction)
    {
        if (_smoothTime > 0f)
        {
            float targetVelocityX = direction * _moveSpeed;
            float smoothVelocityX = Mathf.SmoothDamp(
                _rigidbody.velocity.x,
                targetVelocityX,
                ref _velocitySmoothing,
                _smoothTime);

            _rigidbody.velocity = new Vector2(smoothVelocityX, _rigidbody.velocity.y);
        }
        else
        {
            _rigidbody.velocity = new Vector2(direction * _moveSpeed, _rigidbody.velocity.y);
        }
    }

    public void SetVelocityX(float velocityX)
    {
        _rigidbody.velocity = new Vector2(velocityX, _rigidbody.velocity.y);
    }

    public void Jump(float force)
    {
        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, force);
    }

    public void Stop()
    {
        _rigidbody.velocity = new Vector2(0f, _rigidbody.velocity.y);
    }
}
