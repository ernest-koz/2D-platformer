using UnityEngine;

public abstract class HealthView : MonoBehaviour
{
    [SerializeField] private Health _health;

    protected Health Health => _health;

    protected virtual void OnEnable()
    {
        Health.Changed += Render;
    }

    protected virtual void Start()
    {
        Render(Health.Current, Health.Maximum);
    }

    protected virtual void OnDisable()
    {
        Health.Changed -= Render;
    }

    protected virtual void OnValidate()
    {
        if (_health == null)
        {
            Debug.LogError($"{nameof(HealthView)} health not assigned on {gameObject.name}.", gameObject);
        }
    }

    protected abstract void Render(int current, int maximum);
}
