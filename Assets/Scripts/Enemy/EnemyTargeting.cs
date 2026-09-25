using UnityEngine;

public class EnemyTargeting : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField, Min(0f)] private float _detectRange = 5f;
    [SerializeField, Min(0f)] private float _chaseRange = 7f;
    [SerializeField] private LayerMask _targetLayer;

    private Collider2D[] _targetBuffer = TargetSearch.CreateBuffer();

    public float DetectRange => _detectRange;
    public float ChaseRange => _chaseRange;

    public ITargetable FindNearestTarget(float range)
    {
        int count = TargetSearch.Collect(transform.position, range, _targetLayer, ref _targetBuffer);

        return TargetSearch.FindNearest(_targetBuffer, count, transform.position, gameObject);
    }
}
