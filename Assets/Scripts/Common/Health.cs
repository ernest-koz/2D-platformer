using System;
using UnityEngine;

public class Health : MonoBehaviour, ITargetable
{
    [Header("Health")]
    [SerializeField, Min(1)] private int _maximum = 3;
    [SerializeField, Min(0f)] private float _invincibilityTime = 1f;

    private int _current;
    private float _invincibilityTimer;
    private bool _isDead;

    public event Action<int, int> Changed;
    public event Action<Vector2> Damaged;
    public event Action Died;
    public event Action<bool> InvincibilityChanged;

    public int Current => _current;
    public int Maximum => _maximum;
    public bool IsAlive => _isDead == false;
    public bool IsInvincible => _invincibilityTimer > 0f;
    public Vector3 Position => transform.position;
    public bool IsTargetable => IsAlive;

    private void Awake()
    {
        _current = _maximum;
    }

    public void Tick(float deltaTime)
    {
        if (deltaTime <= 0f)
        {
            return;
        }

        if (_invincibilityTimer <= 0f)
        {
            return;
        }

        _invincibilityTimer = Mathf.Max(_invincibilityTimer - deltaTime, 0f);

        if (_invincibilityTimer > 0f)
        {
            return;
        }

        InvincibilityChanged?.Invoke(false);
    }

    public void TakeDamage(int amount, Vector2 damageSourcePosition)
    {
        if (amount <= 0)
        {
            return;
        }

        if (IsInvincible)
        {
            return;
        }

        if (IsAlive == false)
        {
            return;
        }

        _current = Mathf.Max(_current - amount, 0);
        Changed?.Invoke(_current, _maximum);

        if (_current == 0)
        {
            Die();
            return;
        }

        _invincibilityTimer = _invincibilityTime;

        if (IsInvincible)
        {
            InvincibilityChanged?.Invoke(true);
        }

        Damaged?.Invoke(damageSourcePosition);
    }

    public int TakeDrain(int amount, Vector2 damageSourcePosition)
    {
        int healthBefore = _current;

        TakeDamage(amount, damageSourcePosition);

        return healthBefore - _current;
    }

    public bool Heal(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (IsAlive == false)
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

    private void Die()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;
        Died?.Invoke();
    }
}
