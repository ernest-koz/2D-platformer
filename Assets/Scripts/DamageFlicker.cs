using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class DamageFlicker : MonoBehaviour
{
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _flickerFrequency = 18f;

    private void Awake()
    {
        if (_playerHealth == null)
        {
            _playerHealth = GetComponent<PlayerHealth>();
        }

        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Update()
    {
        if (_playerHealth == null || _spriteRenderer == null)
        {
            return;
        }

        if (_playerHealth.IsInvincible)
        {
            _spriteRenderer.enabled = Mathf.FloorToInt(Time.time * _flickerFrequency) % 2 == 0;
        }
        else
        {
            _spriteRenderer.enabled = true;
        }
    }
}
