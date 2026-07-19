using TMPro;
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

        _coinText.text = string.Format(_coinFormat, totalCoins);
    }
}
