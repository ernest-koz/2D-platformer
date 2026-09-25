using System;
using UnityEngine;

public class Vampirism : MonoBehaviour
{
    private const float MaximumPendingDamage = 3f;

    [Header("Vampirism")]
    [SerializeField, Min(0.1f)] private float _duration = 6f;
    [SerializeField, Min(0.1f)] private float _cooldownDuration = 4f;
    [SerializeField, Min(0.1f)] private float _radius = 3f;
    [SerializeField, Min(0.1f)] private float _drainPerSecond = 5f;
    [SerializeField] private LayerMask _targetLayer;

    private Collider2D[] _targetBuffer = TargetSearch.CreateBuffer();
    private float _remainingTime;
    private float _remainingCooldownTime;
    private float _pendingDamage;

    public event Action<int> Drained;
    public event Action FillChanged;

    public bool IsActive => _remainingTime > 0f;

    public bool IsReady
    {
        get
        {
            if (IsActive)
            {
                return false;
            }

            return _remainingCooldownTime <= 0f;
        }
    }

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
        float fillBefore = GetFill();

        if (IsActive)
        {
            TickActive(deltaTime);
        }
        else
        {
            TickCooldown(deltaTime);
        }

        if (GetFill() == fillBefore)
        {
            return;
        }

        FillChanged?.Invoke();
    }

    public void Interrupt()
    {
        if (IsActive == false)
        {
            return;
        }

        float fillBefore = GetFill();
        _remainingTime = 0f;
        _pendingDamage = 0f;

        if (GetFill() == fillBefore)
        {
            return;
        }

        FillChanged?.Invoke();
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
        if (_remainingCooldownTime <= 0f)
        {
            return;
        }

        _remainingCooldownTime = Mathf.Max(_remainingCooldownTime - deltaTime, 0f);
    }

    private void Stop()
    {
        _remainingTime = 0f;
        _pendingDamage = 0f;
        _remainingCooldownTime = _cooldownDuration;
    }

    private void DrainNearestTarget(float deltaTime)
    {
        ITargetable target = FindNearestTarget();

        if (target == null)
        {
            return;
        }

        _pendingDamage = Mathf.Min(_pendingDamage + _drainPerSecond * deltaTime, MaximumPendingDamage);

        int damage = target.TakeDamage((int)_pendingDamage, transform.position);

        if (damage < 1)
        {
            return;
        }

        _pendingDamage -= damage;
        Drained?.Invoke(damage);
    }

    private ITargetable FindNearestTarget()
    {
        int count = TargetSearch.Collect(transform.position, _radius, _targetLayer, ref _targetBuffer);

        return TargetSearch.FindNearest(_targetBuffer, count, transform.position, gameObject);
    }

    private float GetFill()
    {
        if (IsActive)
        {
            return _remainingTime / _duration;
        }

        if (_remainingCooldownTime > 0f)
        {
            return 1f - _remainingCooldownTime / _cooldownDuration;
        }

        return 1f;
    }
}
