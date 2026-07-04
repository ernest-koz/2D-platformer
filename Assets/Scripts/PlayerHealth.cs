using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invincibilityTime = 1.0f;
    [SerializeField] private Vector3 spawnPoint;

    [Header("Stomp threshold (sync with PlayerCombat.IsFalling)")]
    [SerializeField] private float minStompVelocity = -0.5f;

    [Header("UI (optional)")]
    [SerializeField] private Text healthText;
    [SerializeField] private string healthFormat = "HP: {0}/{1}";

    private int _currentHealth;
    private float _invincibilityTimer;
    private SpriteRenderer _spriteRenderer;
    private PlayerCombat _combat;
    private Rigidbody2D _rb;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsAlive => _currentHealth > 0;

    public static event System.Action OnPlayerRespawn;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _combat = GetComponent<PlayerCombat>();
        _rb = GetComponent<Rigidbody2D>();
        _currentHealth = maxHealth;
        spawnPoint = transform.position;
        UpdateUI();
    }

    private void Update()
    {
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

        UpdateUI();

        if (_currentHealth <= 0) Respawn();
    }

    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
        UpdateUI();
    }

    public void Respawn()
    {
        _currentHealth = maxHealth;
        if (_rb != null) _rb.velocity = Vector2.zero;
        transform.position = spawnPoint;
        UpdateUI();
        OnPlayerRespawn?.Invoke();
    }

    private void UpdateUI()
    {
        if (healthText != null)
            healthText.text = string.Format(healthFormat, _currentHealth, maxHealth);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.collider.CompareTag("Enemy")) return;
        var enemy = col.collider.GetComponentInParent<Enemy>();
        if (enemy == null || enemy.IsDead) return;

        // If the player is mostly above the enemy and falling, treat as stomp (handled in PlayerCombat)
        // threshold matches PlayerCombat.IsFalling
        bool playerAboveAndFalling =
            transform.position.y > col.transform.position.y + 0.4f && _rb.velocity.y < minStompVelocity;
        if (playerAboveAndFalling) return;

        TakeDamage(col.transform.position);
    }
}
