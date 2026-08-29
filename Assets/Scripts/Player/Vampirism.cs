using System;
using UnityEngine;

public class Vampirism : MonoBehaviour
{
    private const int InitialTargetBufferSize = 8;
    private const int MaximumTargetBufferSize = 64;
    private const float MaximumPendingDamage = 3f;

    [Header("Vampirism")]
    [SerializeField, Min(0.1f)] private float _duration = 6f;
    [SerializeField, Min(0.1f)] private float _cooldownTime = 4f;
    [SerializeField, Min(0.1f)] private float _radius = 3f;
    [SerializeField, Min(0.1f)] private float _drainPerSecond = 5f;
    [SerializeField] private LayerMask _targetLayer;

    private Collider2D[] _targetBuffer = new Collider2D[InitialTargetBufferSize];
    private float _remainingTime;
    private float _cooldownTimer;
    private float _pendingDamage;

    public event Action<int> Drained;

    public bool IsActive => _remainingTime > 0f;
    public bool IsReady => IsActive == false && _cooldownTimer <= 0f;
    public float Radius => _radius;
    public float Fill => GetFill();

    public void Activate()
    {
        if (IsReady == false)
        {
            return;
        }

        _remainingTime = _duration;
    }

    public void Tick(float deltaTime)
    {
        if (IsActive)
        {
            TickActive(deltaTime);
            return;
        }

        TickCooldown(deltaTime);
    }

    public void Interrupt()
    {
        if (IsActive == false)
        {
            return;
        }

        _remainingTime = 0f;
        _pendingDamage = 0f;
    }

    private void TickActive(float deltaTime)
    {
        _remainingTime = Mathf.Max(_remainingTime - deltaTime, 0f);

        if (_remainingTime == 0f)
        {
            Stop();
            return;
        }

        DrainNearestTarget(deltaTime);
    }

    private void TickCooldown(float deltaTime)
    {
        if (_cooldownTimer <= 0f)
        {
            return;
        }

        _cooldownTimer = Mathf.Max(_cooldownTimer - deltaTime, 0f);
    }

    private void Stop()
    {
        _remainingTime = 0f;
        _pendingDamage = 0f;
        _cooldownTimer = _cooldownTime;
    }

    private void DrainNearestTarget(float deltaTime)
    {
        ITargetable target = FindNearestTarget();

        if (target == null)
        {
            return;
        }

        _pendingDamage = Mathf.Min(_pendingDamage + _drainPerSecond * deltaTime, MaximumPendingDamage);

        int damage = target.TakeDrain((int)_pendingDamage, transform.position);

        if (damage < 1)
        {
            return;
        }

        _pendingDamage -= damage;
        Drained?.Invoke(damage);
    }

    private ITargetable FindNearestTarget()
    {
        int count = FindTargets();
        ITargetable nearest = null;
        float nearestSqrDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (_targetBuffer[i].gameObject == gameObject)
            {
                continue;
            }

            if (_targetBuffer[i].TryGetComponent(out ITargetable candidate) == false)
            {
                continue;
            }

            if (candidate.IsTargetable == false)
            {
                continue;
            }

            float sqrDistance = ((Vector2)(candidate.Position - transform.position)).sqrMagnitude;

            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = candidate;
            }
        }

        return nearest;
    }

    private int FindTargets()
    {
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, _radius, _targetBuffer, _targetLayer);

        while (count == _targetBuffer.Length)
        {
            if (_targetBuffer.Length >= MaximumTargetBufferSize)
            {
                break;
            }

            int newSize = Mathf.Min(_targetBuffer.Length * 2, MaximumTargetBufferSize);
            _targetBuffer = new Collider2D[newSize];
            count = Physics2D.OverlapCircleNonAlloc(transform.position, _radius, _targetBuffer, _targetLayer);
        }

        return count;
    }

    private float GetFill()
    {
        if (IsActive)
        {
            return _remainingTime / _duration;
        }

        if (_cooldownTimer > 0f)
        {
            return 1f - _cooldownTimer / _cooldownTime;
        }

        return 1f;
    }
}
