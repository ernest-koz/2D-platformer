using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VampirismZone : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private bool _isVisible;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _isVisible = _spriteRenderer.enabled;
    }

    public void Show() =>
        SetVisibleCore(true);

    public void Hide() =>
        SetVisibleCore(false);

    private void SetVisibleCore(bool isVisible)
    {
        if (_isVisible == isVisible)
        {
            return;
        }

        _isVisible = isVisible;
        _spriteRenderer.enabled = isVisible;
    }
}
