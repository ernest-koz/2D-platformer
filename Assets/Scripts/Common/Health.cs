using System;
using UnityEngine;

public class Health : MonoBehaviour, IHealthTarget
{
    [Header("Health")]
    [SerializeField, Min(1)] private int _maximum = 3;

    private int _current;

    public event Action<int, int> Changed;
    public event Action<Vector2> Damaged;
    public event Action Died;

    public int Current => _current;
    public int Maximum => _maximum;
    public bool IsAlive => _current > 0;

    private void Awake()
    {
        _current = _maximum;
    }

    public int TakeDamage(int amount, Vector2 damageSourcePosition)
    {
        if (amount <= 0)
        {
            return 0;
        }

        int applied = Mathf.Min(amount, _current);

        if (applied <= 0)
        {
            return 0;
        }

        _current -= applied;
        Changed?.Invoke(_current, _maximum);

        if (_current == 0)
        {
            Died?.Invoke();
            return applied;
        }

        Damaged?.Invoke(damageSourcePosition);
        return applied;
    }

    public bool ReceiveHealing(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (_current >= _maximum)
        {
            return false;
        }

        int missing = _maximum - _current;
        int restored = Mathf.Min(amount, missing);
        _current += restored;
        Changed?.Invoke(_current, _maximum);
        return true;
    }
}
