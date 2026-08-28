using UnityEngine;

public class SmoothHealthBar : HealthBar
{
    [SerializeField, Min(0.01f)] private float _fillSpeed = 0.5f;

    private float _target;

    protected override void Start()
    {
        base.Start();

        Slider.normalizedValue = _target;
    }

    private void Update()
    {
        Slider.normalizedValue = Mathf.MoveTowards(Slider.normalizedValue, _target, _fillSpeed * Time.deltaTime);
    }

    protected override void Render(int current, int maximum)
    {
        Slider.maxValue = maximum;
        _target = maximum > 0 ? (float)current / maximum : 0f;
    }
}
