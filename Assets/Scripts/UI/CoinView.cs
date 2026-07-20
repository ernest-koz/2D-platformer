using TMPro;
using System;
using UnityEngine;

public class CoinView : MonoBehaviour
{
    [SerializeField] private TMP_Text _coinText;
    [SerializeField] private string _coinFormat = "Монеты: {0}";

    public void Render(int totalCoins)
    {
        if (_coinText == null)
        {
            return;
        }

        try
        {
            _coinText.text = string.Format(_coinFormat, totalCoins);
        }
        catch (FormatException exception)
        {
            Debug.LogError($"Invalid coin format on {gameObject.name}: {exception.Message}", gameObject);
        }
    }
}
