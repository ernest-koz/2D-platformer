using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]

public class SpriteFacing : MonoBehaviour
{
    private const float FlipAngleDegrees = 180f;

    [SerializeField] private bool _isInitiallyFacingRight = true;

    private Quaternion _rightRotation;
    private Quaternion _leftRotation;
    private int _direction;

    public int Direction => _direction;
    public Vector2 Vector => _direction > 0 ? Vector2.right : Vector2.left;

    private void Awake()
    {
        _rightRotation = transform.localRotation;
        _leftRotation = _rightRotation * Quaternion.Euler(0f, FlipAngleDegrees, 0f);
        _direction = _isInitiallyFacingRight ? 1 : -1;

        ApplyRotation();
    }

    public void Flip()
    {
        _direction *= -1;
        ApplyRotation();
    }

    public void Face(float directionX)
    {
        if (directionX > 0f)
        {
            if (_direction < 0)
            {
                Flip();
            }

            return;
        }

        if (directionX < 0f)
        {
            if (_direction > 0)
            {
                Flip();
            }
        }
    }

    private void ApplyRotation()
    {
        transform.localRotation = _direction > 0 ? _rightRotation : _leftRotation;
    }
}
