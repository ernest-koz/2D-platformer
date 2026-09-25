using UnityEngine;
using UnityEngine.UI;

public abstract class HealthChangerButton : MonoBehaviour
{
    [SerializeField] private Health _health;
    [SerializeField] private Button _button;

    protected Health Health => _health;

    private void OnEnable()
    {
        _button.onClick.AddListener(ApplyOnPress);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(ApplyOnPress);
    }

    private void OnValidate()
    {
        if (_health == null)
        {
            Debug.LogError($"{nameof(HealthChangerButton)} health not assigned on {gameObject.name}.", gameObject);
        }

        if (_button == null)
        {
            Debug.LogError($"{nameof(HealthChangerButton)} button not assigned on {gameObject.name}.", gameObject);
        }
    }

    private void ApplyOnPress()
    {
        Apply();
    }

    protected abstract void Apply();
}
