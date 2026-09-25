using TMPro;
using UnityEngine;

public class CoinView : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private string _format = "Монеты: {0}";

    private void OnValidate()
    {
        if (_text == null)
        {
            Debug.LogError($"{nameof(CoinView)} text not assigned on {gameObject.name}.", gameObject);
        }

        if (TextFormatUtility.IsValid(_format, 0) == false)
        {
            Debug.LogError($"{nameof(CoinView)} format is invalid on {gameObject.name}.", gameObject);
        }
    }

    public void Render(int totalCoins)
    {
        _text.text = string.Format(_format, totalCoins);
    }
}
