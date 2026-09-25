using System.Collections;
using UnityEngine;

public class SmoothHealthBar : HealthBar
{
    [SerializeField, Min(0.01f)] private float _fillDuration = 0.5f;

    private Coroutine _fillRoutine;

    protected override void Render(int current, int maximum)
    {
        RestartFill(CalculateRatio(current, maximum));
    }

    private void RestartFill(float target)
    {
        if (_fillRoutine != null)
        {
            StopCoroutine(_fillRoutine);
        }

        _fillRoutine = StartCoroutine(FillRoutine(target));
    }

    private IEnumerator FillRoutine(float target)
    {
        float from = Slider.value;

        for (float time = 0f; time < _fillDuration; time += Time.deltaTime)
        {
            Slider.value = Mathf.Lerp(from, target, time / _fillDuration);

            yield return null;
        }

        Slider.value = target;
        _fillRoutine = null;
    }
}
