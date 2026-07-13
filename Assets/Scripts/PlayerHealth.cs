using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 3;
    [SerializeField] private float _invincibilityTime = 1.0f;

    [Header("Falling death")]
    [SerializeField] private float _deathY = -20f;

    [Header("UI (optional)")]
    [SerializeField] private Text _healthText;
    [SerializeField] private string _healthFormat = "HP: {0}/{1}";

    [Header("References")]
    [SerializeField] private GameSession _gameSession;

    private const float FlickerFrequency = 18f;

    private int _currentHealth;
    private float _invincibilityTimer;
    private SpriteRenderer _spriteRenderer;
    private PlayerCombat _playerCombat;
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private PlayerMovement _playerMovement;
    private bool _isDead;

    private static readonly int HurtTriggerHash = Animator.StringToHash("Hurt");
    private static readonly int DieTriggerHash = Animator.StringToHash("Die");

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsAlive => _currentHealth > 0 && _isDead == false;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _playerCombat = GetComponent<PlayerCombat>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();
        _currentHealth = _maxHealth;
        UpdateHealthUI();
    }

    private void Update()
    {
        CheckFallingDeath();

        if (_invincibilityTimer > 0f)
        {
            _invincibilityTimer -= Time.deltaTime;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = Mathf.FloorToInt(Time.time * FlickerFrequency) % 2 == 0;
            }
        }
        else if (_spriteRenderer != null && _spriteRenderer.enabled == false)
        {
            _spriteRenderer.enabled = true;
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

        if (_playerCombat != null)
        {
            _playerCombat.ApplyKnockbackFrom(damageSourcePosition);
        }

        if (_animator != null)
        {
            _animator.SetTrigger(HurtTriggerHash);
        }

        UpdateHealthUI();

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
        UpdateHealthUI();
    }

    private void Die()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;

        if (_animator != null)
        {
            _animator.SetTrigger(DieTriggerHash);
        }

        if (_playerMovement != null)
        {
            _playerMovement.SetDead(true);
        }

        if (_rigidbody != null)
        {
            _rigidbody.velocity = Vector2.zero;
        }

        if (_gameSession != null)
        {
            _gameSession.GameOver();
        }
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

    private void UpdateHealthUI()
    {
        if (_healthText != null)
        {
            _healthText.text = string.Format(_healthFormat, _currentHealth, _maxHealth);
        }
    }
}
