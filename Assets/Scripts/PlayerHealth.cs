using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invincibilityTime = 1.0f;
    [SerializeField] private Vector3 spawnPoint;

    [Header("Falling death")]
    [SerializeField] private float deathY = -20f;

    [Header("UI (optional)")]
    [SerializeField] private Text healthText;
    [SerializeField] private string healthFormat = "HP: {0}/{1}";

    private int _currentHealth;
    private float _invincibilityTimer;
    private SpriteRenderer _spriteRenderer;
    private PlayerCombat _combat;
    private Rigidbody2D _rb;
    private Animator _animator;
    private PlayerController _controller;
    private bool _isDead;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsAlive => _currentHealth > 0 && !_isDead;

    private static readonly int HurtTriggerHash = Animator.StringToHash("Hurt");
    private static readonly int DieTriggerHash = Animator.StringToHash("Die");

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _combat = GetComponent<PlayerCombat>();
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _controller = GetComponent<PlayerController>();
        _currentHealth = maxHealth;
        spawnPoint = transform.position;
        UpdateHealthUI();
    }

    private void Update()
    {
        CheckFallingDeath();

        if (_invincibilityTimer > 0f)
        {
            _invincibilityTimer -= Time.deltaTime;
            if (_spriteRenderer != null)
                _spriteRenderer.enabled = Mathf.FloorToInt(Time.time * 18f) % 2 == 0;
        }
        else if (_spriteRenderer != null && !_spriteRenderer.enabled)
        {
            _spriteRenderer.enabled = true;
        }
    }

    public void TakeDamage(Vector2 damageSourcePosition) => TakeDamage(1, damageSourcePosition);

    public void TakeDamage(int amount, Vector2 damageSourcePosition)
    {
        if (_invincibilityTimer > 0f || !IsAlive) return;

        _currentHealth -= amount;
        _invincibilityTimer = invincibilityTime;

        if (_combat != null) _combat.ApplyKnockbackFrom(damageSourcePosition);
        if (_animator != null) _animator.SetTrigger(HurtTriggerHash);
        UpdateHealthUI();

        if (_currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        if (_animator != null) _animator.SetTrigger(DieTriggerHash);
        if (_controller != null) _controller.SetDead(true);
        if (_rb != null) _rb.velocity = Vector2.zero;

        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
    }

    private void CheckFallingDeath()
    {
        if (_isDead) return;
        if (transform.position.y < deathY)
        {
            _isDead = true;
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
        }
    }

    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = string.Format(healthFormat, _currentHealth, maxHealth);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.collider.CompareTag("Enemy")) return;
        var enemy = col.collider.GetComponentInParent<Enemy>();
        if (enemy == null || enemy.IsDead) return;

        bool playerAboveAndFalling =
            transform.position.y > col.transform.position.y + 0.4f && _rb.velocity.y < -0.5f;
        if (playerAboveAndFalling) return;

        TakeDamage(col.transform.position);
    }
}
