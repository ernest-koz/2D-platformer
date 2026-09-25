using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private string _format = "HP: {0}/{1}";

    private void OnValidate()
    {
        if (_text == null)
        {
            Debug.LogError($"{nameof(HealthUI)} text not assigned on {gameObject.name}.", gameObject);
        }

        if (TextFormatUtility.IsValid(_format, 0, 0) == false)
        {
            Debug.LogError($"{nameof(HealthUI)} format is invalid on {gameObject.name}.", gameObject);
        }
    }

    public void Render(int current, int maximum)
    {
        _text.text = string.Format(_format, current, maximum);
    }
}
