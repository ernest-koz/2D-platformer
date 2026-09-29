using UnityEngine;
using UnityEngine.UI;

public class VampirismView : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private SpriteRenderer _zone;

    private Phase _phase = Phase.Idle;

    private void Awake()
    {
        _slider.minValue = 0f;
        _slider.maxValue = 1f;
        _slider.value = 1f;
        _zone.enabled = false;
    }

    private void OnValidate()
    {
        if (_slider == null)
        {
            Debug.LogError($"{nameof(VampirismView)} slider not assigned on {gameObject.name}.", gameObject);
        }

        if (_zone == null)
        {
            Debug.LogError($"{nameof(VampirismView)} zone renderer not assigned on {gameObject.name}.", gameObject);
        }
    }

    public void StartAbility()
    {
        _phase = Phase.Ability;
        _zone.enabled = true;
        RenderFill(1f);
    }

    public void Tick(float elapsedTime, float duration)
    {
        if (_phase == Phase.Ability)
        {
            RenderFill(1f - elapsedTime / duration);
            return;
        }

        if (_phase == Phase.Cooldown)
        {
            RenderFill(elapsedTime / duration);
        }
    }

    public void StartCooldown()
    {
        _phase = Phase.Cooldown;
        _zone.enabled = false;
        RenderFill(0f);
    }

    public void Stop()
    {
        _phase = Phase.Idle;
        _zone.enabled = false;
        RenderFill(1f);
    }

    private void RenderFill(float fill)
    {
        _slider.value = Mathf.Clamp01(fill);
    }

    private enum Phase
    {
        Idle,
        Ability,
        Cooldown
    }
}
