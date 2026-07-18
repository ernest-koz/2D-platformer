using UnityEngine;

public class PatrolRoute : MonoBehaviour
{
    [SerializeField] private float _leftBoundary = -3f;
    [SerializeField] private float _rightBoundary = 3f;

    public float LeftBoundary => _leftBoundary;
    public float RightBoundary => _rightBoundary;

    public float GetDirectionToward(float currentX, int facingDirection)
    {
        if (facingDirection > 0 && currentX >= _rightBoundary)
        {
            return -1f;
        }

        if (facingDirection < 0 && currentX <= _leftBoundary)
        {
            return 1f;
        }

        return facingDirection;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 left = new Vector3(_leftBoundary, transform.position.y, 0f);
        Vector3 right = new Vector3(_rightBoundary, transform.position.y, 0f);
        Gizmos.DrawLine(left, right);
    }
}
