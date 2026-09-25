using TMPro;
using UnityEngine;

public class HealthText : HealthView
{
    [SerializeField] private TMP_Text _text;

    protected override void OnValidate()
    {
        base.OnValidate();

        if (_text == null)
        {
            Debug.LogError($"{nameof(HealthText)} text not assigned on {gameObject.name}.", gameObject);
        }
    }

    protected override void Render(int current, int maximum)
    {
        _text.text = $"{current}/{maximum}";
    }
}
