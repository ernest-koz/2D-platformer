using UnityEngine;

public class VampirismDamager : MonoBehaviour
{
    private const float MaximumPendingDamage = 3f;

    [Header("Drain")]
    [SerializeField, Min(0.1f)] private float _radius = 3f;
    [SerializeField, Min(0f)] private float _drainPerSecond = 1f;
    [SerializeField] private LayerMask _targetLayer;

    private Collider2D[] _targetBuffer = TargetSearch.CreateBuffer();
    private float _pendingDamage;

    public float Radius => _radius;

    private void OnValidate()
    {
        if (_targetLayer.value == 0)
        {
            Debug.LogError($"{nameof(VampirismDamager)} target layer not assigned on {gameObject.name}.", gameObject);
        }
    }

    public int Tick(Vector2 origin, float deltaTime)
    {
        ITargetable target = FindNearestTarget(origin);

        if (target == null)
        {
            _pendingDamage = 0f;
            return 0;
        }

        _pendingDamage = Mathf.Min(_pendingDamage + _drainPerSecond * deltaTime, MaximumPendingDamage);

        if (_pendingDamage < 1f)
        {
            return 0;
        }

        int damage = (int)_pendingDamage;
        int applied = target.TakeDamage(damage, origin);

        if (applied < 1)
        {
            return 0;
        }

        _pendingDamage -= applied;
        return applied;
    }

    private ITargetable FindNearestTarget(Vector2 origin)
    {
        int count = TargetSearch.Collect(origin, _radius, _targetLayer, ref _targetBuffer);

        return TargetSearch.FindNearest(_targetBuffer, count, origin, gameObject);
    }
}
