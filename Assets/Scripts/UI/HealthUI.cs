using System;
using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private string _healthFormat = "HP: {0}/{1}";

    public void Render(int current, int maximum)
    {
        if (_healthText == null)
        {
            Debug.LogError($"{nameof(HealthUI)} text not assigned on {gameObject.name}.", gameObject);
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
