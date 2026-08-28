using UnityEngine;

public class SmoothHealthBar : HealthBar
{
    [SerializeField, Min(0.1f)] private float _fillSpeed = 50f;

    private float _target;

    protected override void Start()
    {
        base.Start();

        Slider.value = _target;
    }

    private void Update()
    {
        Slider.value = Mathf.MoveTowards(Slider.value, _target, _fillSpeed * Time.deltaTime);
    }

    protected override void Render(int current, int maximum)
    {
        Slider.maxValue = maximum;
        _target = current;
    }
}
