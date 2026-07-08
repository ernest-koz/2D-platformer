using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class PlayerHealth : MonoBehaviour
{
    [FormerlySerializedAs("maxHealth")] [SerializeField] private int _maxHealth = 3;
    [FormerlySerializedAs("invincibilityTime")] [SerializeField] private float _invincibilityTime = 1.0f;
    [FormerlySerializedAs("spawnPoint")] [SerializeField] private Vector3 _spawnPoint;

    [Header("Falling death")]
    [FormerlySerializedAs("deathY")] [SerializeField] private float _deathY = -20f;

    [Header("UI (optional)")]
    [FormerlySerializedAs("healthText")] [SerializeField] private Text _healthText;
    [FormerlySerializedAs("healthFormat")] [SerializeField] private string _healthFormat = "HP: {0}/{1}";

    private const float FlickerFrequency = 18f;
    private const float StompHeightThreshold = 0.4f;
    private const float FallSpeedThreshold = 0.5f;

    private int _currentHealth;
    private float _invincibilityTimer;
    private SpriteRenderer _spriteRenderer;
    private PlayerCombat _combat;
    private Rigidbody2D _rb;
    private Animator _animator;
    private PlayerController _controller;
    private bool _isDead;

    private static readonly int HurtTriggerHash = Animator.StringToHash("Hurt");
    private static readonly int DieTriggerHash = Animator.StringToHash("Die");

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsAlive => _currentHealth > 0 && _isDead == false;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _combat = GetComponent<PlayerCombat>();
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _controller = GetComponent<PlayerController>();
        _currentHealth = _maxHealth;
        _spawnPoint = transform.position;
        UpdateHealthUI();
    }

    private void Update()
    {
        CheckFallingDeath();

        if (_invincibilityTimer > 0f)
        {
            _invincibilityTimer -= Time.deltaTime;

            if (_spriteRenderer != null)
                _spriteRenderer.enabled = Mathf.FloorToInt(Time.time * FlickerFrequency) % 2 == 0;
        }
        else if (_spriteRenderer != null && _spriteRenderer.enabled == false)
        {
            _spriteRenderer.enabled = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Enemy") == false) return;

        var enemy = col.collider.GetComponentInParent<Enemy>();

        if (enemy == null || enemy.IsDead) return;

        bool playerAboveAndFalling =
            transform.position.y > col.transform.position.y + StompHeightThreshold && _rb.velocity.y < -FallSpeedThreshold;

        if (playerAboveAndFalling) return;

        TakeDamage(col.transform.position);
    }

    public void TakeDamage(Vector2 damageSourcePosition) =>
        TakeDamage(1, damageSourcePosition);

    public void TakeDamage(int amount, Vector2 damageSourcePosition)
    {
        if (_invincibilityTimer > 0f || IsAlive == false) return;

        _currentHealth -= amount;
        _invincibilityTimer = _invincibilityTime;

        if (_combat != null) _combat.ApplyKnockbackFrom(damageSourcePosition);

        if (_animator != null) _animator.SetTrigger(HurtTriggerHash);

        UpdateHealthUI();

        if (_currentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
        UpdateHealthUI();
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

        if (transform.position.y < _deathY)
        {
            _isDead = true;

            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
        }
    }

    private void UpdateHealthUI()
    {
        if (_healthText != null)
            _healthText.text = string.Format(_healthFormat, _currentHealth, _maxHealth);
    }
}
