using UnityEngine;

public class SpriteFacing : MonoBehaviour
{
    [SerializeField] private bool _startsFacingRight = true;

    private int _direction;

    public int FacingDirection => _direction;

    public Vector2 FacingVector => _direction > 0 ? Vector2.right : Vector2.left;

    private void Awake()
    {
        _direction = _startsFacingRight ? 1 : -1;
        ApplyScale();
    }

    public void Flip()
    {
        _direction *= -1;
        ApplyScale();
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

    private void ApplyScale()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * _direction;
        transform.localScale = scale;
    }
}
