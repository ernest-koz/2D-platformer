using System;
using UnityEngine;

public class Health : MonoBehaviour, ITargetable
{
    [Header("Health")]
    [SerializeField, Min(1)] private int _maximumHealth = 3;
    [SerializeField, Min(0f)] private float _invincibilityTime = 1f;

    private int _currentHealth;
    private float _invincibilityTimer;
    private bool _isDead;
    private bool _wasInvincible;

    public event Action<int, int> HealthChanged;
    public event Action<Vector2> Damaged;
    public event Action Died;
    public event Action<bool> InvincibilityChanged;

    public int CurrentHealth => _currentHealth;
    public int MaximumHealth => _maximumHealth;
    public bool IsAlive => _isDead == false;
    public bool IsInvincible => _invincibilityTimer > 0f;
    public Vector3 Position => transform.position;
    public bool IsTargetable => IsAlive;

    private void Awake()
    {
        _currentHealth = _maximumHealth;
    }

    private void Start()
    {
        HealthChanged?.Invoke(_currentHealth, _maximumHealth);
    }

    private void Update()
    {
        bool wasInvincible = _wasInvincible;

        if (_invincibilityTimer > 0f)
        {
            _invincibilityTimer -= Time.deltaTime;
        }

        bool isInvincible = _invincibilityTimer > 0f;

        if (wasInvincible != isInvincible)
        {
            _wasInvincible = isInvincible;
            InvincibilityChanged?.Invoke(isInvincible);
        }
    }

    public void TakeDamage(int amount, Vector2 damageSourcePosition)
    {
        if (amount <= 0 || IsInvincible || IsAlive == false)
        {
            return;
        }

        _currentHealth = Mathf.Max(_currentHealth - amount, 0);
        _invincibilityTimer = _invincibilityTime;

        HealthChanged?.Invoke(_currentHealth, _maximumHealth);

        if (_currentHealth == 0)
        {
            Die();
            return;
        }

        Damaged?.Invoke(damageSourcePosition);
    }

    public bool Heal(int amount)
    {
        if (amount <= 0 || IsAlive == false)
        {
            return false;
        }

        int healedHealth = Mathf.Min(_currentHealth + amount, _maximumHealth);

        if (healedHealth == _currentHealth)
        {
            return false;
        }

        _currentHealth = healedHealth;
        HealthChanged?.Invoke(_currentHealth, _maximumHealth);
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
