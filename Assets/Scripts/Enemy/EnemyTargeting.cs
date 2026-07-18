using UnityEngine;

public class EnemyTargeting : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float _detectRange = 5f;
    [SerializeField] private float _chaseRange = 7f;
    [SerializeField] private LayerMask _targetLayer;

    private readonly Collider2D[] _targetBuffer = new Collider2D[8];

    public float DetectRange => _detectRange;
    public float ChaseRange => _chaseRange;
    public LayerMask TargetLayer => _targetLayer;

    public ITargetable FindNearestTarget(float range)
    {
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, range, _targetBuffer, _targetLayer);

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
}
