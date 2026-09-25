using UnityEngine;
using UnityEngine.EventSystems;

public class HoverCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Texture2D _handCursor;
    [SerializeField] private Vector2 _hotspot = new Vector2(11f, 3f);

    private bool _isHandShown;

    private void OnDisable()
    {
        if (_isHandShown)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            _isHandShown = false;
        }
    }

    private void OnValidate()
    {
        if (_handCursor == null)
        {
            Debug.LogError($"{nameof(HoverCursor)} hand cursor not assigned on {gameObject.name}.", gameObject);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowHand();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideHand();
    }

    private void ShowHand()
    {
        Cursor.SetCursor(_handCursor, _hotspot, CursorMode.Auto);
        _isHandShown = true;
    }

    private void HideHand()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        _isHandShown = false;
    }
}
