using UnityEngine;
using UnityEngine.UI;

public class HealthBar : HealthView
{
    [SerializeField] private Slider _slider;

    protected Slider Slider => _slider;

    protected override void Awake()
    {
        base.Awake();

        if (_slider == null)
        {
            Debug.LogError($"{nameof(HealthBar)} slider not assigned on {gameObject.name}.", gameObject);
            enabled = false;
        }
    }

    protected override void Render(int current, int maximum)
    {
        Slider.maxValue = maximum;
        Slider.value = current;
    }
}
