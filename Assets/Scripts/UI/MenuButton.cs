using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private const float IdleScale = 1f;
    private const float ScaleDuration = 0.2f;

    [SerializeField, Min(1f)] private float _hoverScale = 1.08f;
    [SerializeField] private Color _pressedColor = new Color(0.78f, 0.16f, 0.30f);
    [SerializeField] private Image _image;

    private RectTransform _rectTransform;
    private Color _baseColor;
    private Coroutine _scaleRoutine;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _baseColor = _image.color;
    }

    private void OnDisable()
    {
        StopScaleRoutine();
    }

    private void OnValidate()
    {
        if (_image == null)
        {
            Debug.LogError($"{nameof(MenuButton)} image not assigned on {gameObject.name}.", gameObject);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        RestartScaleRoutine(_hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RestartScaleRoutine(IdleScale);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _image.color = _pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _image.color = _baseColor;
    }

    private void RestartScaleRoutine(float target)
    {
        StopScaleRoutine();
        _scaleRoutine = StartCoroutine(ScaleRoutine(target));
    }

    private void StopScaleRoutine()
    {
        if (_scaleRoutine == null)
        {
            return;
        }

        StopCoroutine(_scaleRoutine);
        _scaleRoutine = null;
    }

    private IEnumerator ScaleRoutine(float target)
    {
        float from = _rectTransform.localScale.x;

        for (float time = 0f; time < ScaleDuration; time += Time.deltaTime)
        {
            float next = Mathf.Lerp(from, target, time / ScaleDuration);
            _rectTransform.localScale = new Vector3(next, next, next);

            yield return null;
        }

        _rectTransform.localScale = new Vector3(target, target, target);
        _scaleRoutine = null;
    }
}
