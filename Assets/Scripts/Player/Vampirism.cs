using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
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

    private Health _health;
    private Collider2D[] _targetBuffer = new Collider2D[InitialTargetBufferSize];
    private float _remainingTime;
    private float _cooldownTimer;
    private float _pendingDamage;

    public event Action Activated;
    public event Action Deactivated;

    public bool IsActive => _remainingTime > 0f;
    public bool IsReady => IsActive == false && _cooldownTimer <= 0f;
    public float Radius => _radius;
    public float Fill => GetFill();

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    public void Activate()
    {
        if (IsReady == false)
        {
            return;
        }

        _remainingTime = _duration;
        Activated?.Invoke();
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
        Deactivated?.Invoke();
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
        Deactivated?.Invoke();
    }

    private void DrainNearestTarget(float deltaTime)
    {
        Health target = FindNearestTarget();

        if (target == null)
        {
            return;
        }

        _pendingDamage = Mathf.Min(_pendingDamage + _drainPerSecond * deltaTime, MaximumPendingDamage);

        if (target.IsInvincible)
        {
            return;
        }

        int damage = Mathf.Min((int)_pendingDamage, target.Current);

        if (damage < 1)
        {
            return;
        }

        _pendingDamage -= damage;
        target.TakeDamage(damage, transform.position);
        _health.Heal(damage);
    }

    private Health FindNearestTarget()
    {
        int count = FindTargets();
        Health nearest = null;
        float nearestSqrDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (_targetBuffer[i].TryGetComponent(out Health candidate) == false)
            {
                continue;
            }

            if (candidate == _health)
            {
                continue;
            }

            if (candidate.IsAlive == false)
            {
                continue;
            }

            float sqrDistance = ((Vector2)(candidate.transform.position - transform.position)).sqrMagnitude;

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
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, _radius, _targetBuffer);

        while (count == _targetBuffer.Length)
        {
            if (_targetBuffer.Length >= MaximumTargetBufferSize)
            {
                break;
            }

            int newSize = Mathf.Min(_targetBuffer.Length * 2, MaximumTargetBufferSize);
            _targetBuffer = new Collider2D[newSize];
            count = Physics2D.OverlapCircleNonAlloc(transform.position, _radius, _targetBuffer);
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
