using UnityEngine;
using UnityEngine.UI;

public class VampirismView : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    private void Awake()
    {
        _slider.minValue = 0f;
        _slider.maxValue = 1f;
    }

    private void OnValidate()
    {
        if (_slider == null)
        {
            Debug.LogError($"{nameof(VampirismView)} slider not assigned on {gameObject.name}.", gameObject);
        }
    }

    public void Render(float fill)
    {
        _slider.value = fill;
    }
}
