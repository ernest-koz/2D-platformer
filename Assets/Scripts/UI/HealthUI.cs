using TMPro;
using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class HealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private string _healthFormat = "HP: {0}/{1}";

    private Health _health;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        _health.HealthChanged += OnHealthChanged;
        OnHealthChanged(_health.CurrentHealth, _health.MaximumHealth);
    }

    private void OnDisable()
    {
        _health.HealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int current, int maximum)
    {
        if (_healthText == null)
        {
            return;
        }

        try
        {
            _healthText.text = string.Format(_healthFormat, current, maximum);
        }
        catch (FormatException exception)
        {
            Debug.LogError($"Invalid health format on {gameObject.name}: {exception.Message}", gameObject);
        }
    }
}
