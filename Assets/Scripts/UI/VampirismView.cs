using UnityEngine;
using UnityEngine.UI;

public class VampirismView : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    private void Awake()
    {
        if (_slider == null)
        {
            Debug.LogError($"{nameof(VampirismView)} slider not assigned on {gameObject.name}.", gameObject);
            enabled = false;
        }
    }

    public void Render(float fill)
    {
        _slider.value = fill;
    }
}
