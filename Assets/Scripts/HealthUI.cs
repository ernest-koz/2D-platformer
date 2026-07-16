using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerHealth))]
public class HealthUI : MonoBehaviour
{
    [SerializeField] private Text _healthText;
    [SerializeField] private string _healthFormat = "HP: {0}/{1}";

    private PlayerHealth _playerHealth;

    private void Awake()
    {
        _playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        _playerHealth.HealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        _playerHealth.HealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int current, int max)
    {
        if (_healthText == null)
        {
            return;
        }

        _healthText.text = string.Format(_healthFormat, current, max);
    }
}
