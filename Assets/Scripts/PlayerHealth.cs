using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour, ITargetable
{
    [SerializeField] private int _maxHealth = 3;
    [SerializeField] private float _invincibilityTime = 1.0f;
    [SerializeField] private float _deathY = -20f;
    [SerializeField] private GameSession _gameSession;
    [SerializeField] private PlayerMovement _playerMovement;

    private int _currentHealth;
    private float _invincibilityTimer;
    private Rigidbody2D _rigidbody;
    private bool _isDead;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsAlive => _currentHealth > 0 && _isDead == false;
    public bool IsInvincible => _invincibilityTimer > 0f;
    public Vector3 Position => transform.position;
    public bool IsTargetable => IsAlive;

    public event Action<int, int> HealthChanged;
    public event Action<Vector2> Damaged;
    public event Action Died;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _currentHealth = _maxHealth;

        if (_playerMovement == null)
        {
            _playerMovement = GetComponent<PlayerMovement>();
        }

        if (_gameSession == null)
        {
            Debug.LogError($"PlayerHealth: GameSession not assigned on {gameObject.name}. GameOver will not trigger.", gameObject);
        }
    }

    private void Start()
    {
        HealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    private void Update()
    {
        if (_gameSession == null)
        {
            return;
        }

        if (_gameSession.State == GameState.Playing)
        {
            CheckFallingDeath();
        }

        if (_invincibilityTimer > 0f)
        {

        if (_invincibilityTimer > 0f)
        {
            _invincibilityTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(Vector2 damageSourcePosition) =>
        TakeDamage(1, damageSourcePosition);

    public void TakeDamage(int amount, Vector2 damageSourcePosition)
    {
        if (_invincibilityTimer > 0f || IsAlive == false)
        {
            return;
        }

        _currentHealth -= amount;
        _invincibilityTimer = _invincibilityTime;

        HealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Damaged?.Invoke(damageSourcePosition);
        }
    }

    public void Heal(int amount)
    {
        if (IsAlive == false)
        {
            return;
        }

        amount = Mathf.Max(0, amount);
        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
        HealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    private void Die()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;

        StopMovement();
        NotifyDied();
    }

    private void StopMovement()
    {
        if (_playerMovement == null)
        {
            return;
        }

        _playerMovement.SetDead(true);
        _rigidbody.velocity = Vector2.zero;
    }

    private void NotifyDied()
    {
        Died?.Invoke();

        if (_gameSession == null)
        {
            return;
        }

        _gameSession.GameOver();
    }

    private void CheckFallingDeath()
    {
        if (_isDead)
        {
            return;
        }

        if (transform.position.y < _deathY)
        {
            Die();
        }
    }
}
