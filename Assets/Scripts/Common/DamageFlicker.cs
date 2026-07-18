using UnityEngine;

[RequireComponent(typeof(Health))]
public class DamageFlicker : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _flickerFrequency = 18f;

    private Health _health;
    private bool _isInvincible;

    private void Awake()
    {
        _health = GetComponent<Health>();

        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void OnEnable()
    {
        _health.InvincibilityChanged += OnInvincibilityChanged;
    }

    private void Update()
    {
        if (_isInvincible == false)
        {
            return;
        }

        if (_spriteRenderer == null)
        {
            return;
        }

        _spriteRenderer.enabled = Mathf.FloorToInt(Time.time * _flickerFrequency) % 2 == 0;
    }

    private void OnDisable()
    {
        _health.InvincibilityChanged -= OnInvincibilityChanged;
    }

    private void OnInvincibilityChanged(bool isInvincible)
    {
        _isInvincible = isInvincible;

        if (_spriteRenderer == null)
        {
            return;
        }

        if (isInvincible == false)
        {
            _spriteRenderer.enabled = true;
        }
    }
}
