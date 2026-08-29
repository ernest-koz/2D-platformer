using UnityEngine;

public class DamageFlicker : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _flickerFrequency = 18f;

    private bool _isFlickering;

    private void Awake()
    {
        if (_spriteRenderer == null)
        {
            Debug.LogError($"SpriteRenderer not assigned on {gameObject.name}.", gameObject);
            enabled = false;
        }
    }

    private void OnDisable()
    {
        SetFlickering(false);
    }

    public void SetFlickering(bool isFlickering)
    {
        _isFlickering = isFlickering;

        if (isFlickering == false)
        {
            _spriteRenderer.enabled = true;
        }
    }

    public void Tick(float elapsedTime)
    {
        if (_isFlickering == false)
        {
            return;
        }

        _spriteRenderer.enabled = Mathf.FloorToInt(elapsedTime * _flickerFrequency) % 2 == 0;
    }
}
