using UnityEngine;

public class PatrolRoute : MonoBehaviour
{
    private const float LeftDirection = -1f;
    private const float RightDirection = 1f;

    private static readonly Color RouteGizmoColor = Color.yellow;

    [SerializeField] private float _leftBoundary = -3f;
    [SerializeField] private float _rightBoundary = 3f;

    private void OnValidate()
    {
        if (_leftBoundary >= _rightBoundary)
        {
            Debug.LogError(
                $"{nameof(PatrolRoute)} left boundary must be less than right boundary on {gameObject.name}.",
                gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = RouteGizmoColor;
        Vector3 left = new Vector3(_leftBoundary, transform.position.y, 0f);
        Vector3 right = new Vector3(_rightBoundary, transform.position.y, 0f);
        Gizmos.DrawLine(left, right);
    }

    public float GetDirectionToward(float currentX, int facingDirection)
    {
        if (facingDirection > 0)
        {
            if (currentX >= _rightBoundary)
            {
                return LeftDirection;
            }
        }

        if (facingDirection < 0)
        {
            if (currentX <= _leftBoundary)
            {
                return RightDirection;
            }
        }

        return facingDirection;
    }
}
