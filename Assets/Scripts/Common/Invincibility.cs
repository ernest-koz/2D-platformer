using System;
using UnityEngine;

public class Invincibility
{
    private readonly float _duration;
    private float _remainingTime;

    public Invincibility(float duration)
    {
        _duration = duration;
    }

    public event Action<bool> Changed;

    public bool IsActive => _remainingTime > 0f;

    public void Tick(float deltaTime)
    {
        if (deltaTime <= 0f)
        {
            return;
        }

        if (_remainingTime <= 0f)
        {
            return;
        }

        _remainingTime = Mathf.Max(_remainingTime - deltaTime, 0f);

        if (_remainingTime > 0f)
        {
            return;
        }

        Changed?.Invoke(false);
    }

    public int ApplyDamage(IHealthTarget target, int amount, Vector2 sourcePosition)
    {
        if (IsActive)
        {
            return 0;
        }

        int applied = target.TakeDamage(amount, sourcePosition);

        if (applied > 0)
        {
            if (target.IsAlive)
            {
                Begin();
            }
        }

        return applied;
    }

    private void Begin()
    {
        if (_duration <= 0f)
        {
            return;
        }

        _remainingTime = _duration;
        Changed?.Invoke(true);
    }
}
