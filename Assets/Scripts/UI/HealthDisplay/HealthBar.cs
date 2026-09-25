using UnityEngine;
using UnityEngine.UI;

public class HealthBar : HealthView
{
    [SerializeField] private Slider _slider;

    protected Slider Slider => _slider;

    private void Awake()
    {
        _slider.minValue = 0f;
        _slider.maxValue = 1f;
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        if (_slider == null)
        {
            Debug.LogError($"{nameof(HealthBar)} slider not assigned on {gameObject.name}.", gameObject);
        }
    }

    protected float CalculateRatio(int current, int maximum)
    {
        return maximum > 0 ? (float)current / maximum : 0f;
    }

    protected override void Render(int current, int maximum)
    {
        _slider.value = CalculateRatio(current, maximum);
    }
}
