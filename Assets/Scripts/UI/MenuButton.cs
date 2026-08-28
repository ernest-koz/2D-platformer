using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField, Min(1f)] private float _hoverScale = 1.08f;
    [SerializeField, Min(0.1f)] private float _scaleSpeed = 4f;
    [SerializeField] private Color _pressedColor = new Color(0.78f, 0.16f, 0.30f);
    [SerializeField] private Image _targetImage;

    private RectTransform _rect;
    private Color _baseColor;
    private float _targetScale;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();

        if (_targetImage == null)
        {
            Debug.LogError($"{nameof(MenuButton)} target image not assigned on {gameObject.name}.", gameObject);
            enabled = false;
            return;
        }

        _baseColor = _targetImage.color;
        _targetScale = 1f;
    }

    private void Update()
    {
        float current = _rect.localScale.x;
        float next = Mathf.MoveTowards(current, _targetScale, _scaleSpeed * Time.deltaTime);
        _rect.localScale = new Vector3(next, next, next);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _targetScale = _hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _targetScale = 1f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _targetImage.color = _pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _targetImage.color = _baseColor;
    }
}
