using UnityEngine;
using UnityEngine.UI;

public class VampirismView : MonoBehaviour
{
    [SerializeField] private Vampirism _vampirism;
    [SerializeField] private Slider _slider;

    private void Awake()
    {
        if (_vampirism == null)
        {
            Debug.LogError($"{nameof(VampirismView)} vampirism not assigned on {gameObject.name}.", gameObject);
            enabled = false;
        }

        if (_slider == null)
        {
            Debug.LogError($"{nameof(VampirismView)} slider not assigned on {gameObject.name}.", gameObject);
            enabled = false;
        }
    }

    private void Update()
    {
        _slider.value = _vampirism.Fill;
    }
}
