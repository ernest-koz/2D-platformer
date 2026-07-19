using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFacing : MonoBehaviour
{
    [SerializeField] private bool _startsFacingRight = true;

    private Quaternion _rightRotation;
    private Quaternion _leftRotation;
    private int _direction;

    public int FacingDirection => _direction;
    public Vector2 FacingVector => _direction > 0 ? Vector2.right : Vector2.left;

    private void Awake()
    {
        _rightRotation = transform.localRotation;
        _leftRotation = _rightRotation * Quaternion.Euler(0f, 180f, 0f);
        _direction = _startsFacingRight ? 1 : -1;

        ApplyRotation();
    }

    public void Flip()
    {
        _direction *= -1;
        ApplyRotation();
    }

    public void Face(float directionX)
    {
        if (directionX > 0f && _direction < 0)
        {
            Flip();
        }
        else if (directionX < 0f && _direction > 0)
        {
            Flip();
        }
    }

    private void ApplyRotation()
    {
        transform.localRotation = _direction > 0 ? _rightRotation : _leftRotation;
    }
}
