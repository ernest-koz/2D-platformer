using UnityEngine;

public class DamageFlicker : MonoBehaviour
{
    private const int FlickerParityDivisor = 2;

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _frequency = 18f;

    private bool _isFlickering;

    private void OnDisable()
    {
        StopFlickering();
    }

    private void OnValidate()
    {
        if (_spriteRenderer == null)
        {
            Debug.LogError($"SpriteRenderer not assigned on {gameObject.name}.", gameObject);
        }
    }

    public void StartFlickering() =>
        SetFlickeringCore(true);

    public void StopFlickering() =>
        SetFlickeringCore(false);

    public void Tick(float elapsedTime)
    {
        if (_isFlickering == false)
        {
            return;
        }

        _spriteRenderer.enabled = Mathf.FloorToInt(elapsedTime * _frequency) % FlickerParityDivisor == 0;
    }

    private void SetFlickeringCore(bool isFlickering)
    {
        _isFlickering = isFlickering;

        if (isFlickering == false)
        {
            _spriteRenderer.enabled = true;
        }
    }
}
