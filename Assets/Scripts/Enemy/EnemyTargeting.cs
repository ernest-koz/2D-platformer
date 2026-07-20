using UnityEngine;

public class EnemyTargeting : MonoBehaviour
{
    private const int MaximumTargetBufferSize = 64;

    [Header("Detection")]
    [SerializeField] private float _detectRange = 5f;
    [SerializeField] private float _chaseRange = 7f;
    [SerializeField] private LayerMask _targetLayer;

    private Collider2D[] _targetBuffer = new Collider2D[8];

    public float DetectRange => _detectRange;
    public float ChaseRange => _chaseRange;
    public LayerMask TargetLayer => _targetLayer;

    public ITargetable FindNearestTarget(float range)
    {
        int count = FindTargets(range);

        ITargetable nearest = null;
        float nearestSqrDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (_targetBuffer[i].TryGetComponent(out ITargetable target) == false)
            {
                continue;
            }

            if (target.IsTargetable == false)
            {
                continue;
            }

            float sqrDistance = (target.Position - transform.position).sqrMagnitude;

            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = target;
            }
        }

        return nearest;
    }

    private int FindTargets(float range)
    {
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, range, _targetBuffer, _targetLayer);

        while (count == _targetBuffer.Length && _targetBuffer.Length < MaximumTargetBufferSize)
        {
            int newSize = Mathf.Min(_targetBuffer.Length * 2, MaximumTargetBufferSize);
            _targetBuffer = new Collider2D[newSize];
            count = Physics2D.OverlapCircleNonAlloc(transform.position, range, _targetBuffer, _targetLayer);
        }

        return count;
    }
}
