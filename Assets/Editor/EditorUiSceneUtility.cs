using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class EditorUiSceneUtility
{
    private const string CanvasName = "Canvas";
    private const string CameraName = "Main Camera";
    private const string MainCameraTagName = "MainCamera";
    private const string EventSystemName = "EventSystem";
    private const string BackgroundSpritePath = "UI/Skin/Background.psd";
    private const string InputFieldSpritePath = "UI/Skin/InputFieldBackground.psd";
    private const string CheckmarkSpritePath = "UI/Skin/Checkmark.psd";
    private const string DropdownArrowSpritePath = "UI/Skin/DropdownArrow.psd";
    private const string UiMaskSpritePath = "UI/Skin/UIMask.psd";
    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;

    public const string UiSpritePath = "UI/Skin/UISprite.psd";
    public const string SliderFillAreaName = "Fill Area";
    public const string SliderHandleAreaName = "Handle Slide Area";
    private const string KnobSpritePath = "UI/Skin/Knob.psd";
    private const string SliderBackgroundName = "Background";
    private const string SliderFillAreaFillName = "Fill Area/Fill";

    private static readonly Vector2 TopAnchor = new Vector2(0.5f, 1f);
    private static readonly Vector2 CenterAnchor = new Vector2(0.5f, 0.5f);

    public static Canvas CreateCanvas(Transform parent, float matchWidthOrHeight)
    {
        GameObject canvasObject = new GameObject(
            CanvasName,
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(parent, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
        scaler.matchWidthOrHeight = matchWidthOrHeight;

        return canvas;
    }

    public static void CreateCamera(Color backgroundColor)
    {
        GameObject cameraObject = new GameObject(CameraName, typeof(Camera), typeof(AudioListener));
        cameraObject.tag = MainCameraTagName;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = backgroundColor;
    }

    public static void CreateEventSystem()
    {
        new GameObject(EventSystemName, typeof(EventSystem), typeof(StandaloneInputModule));
    }

    public static RectTransform CreateElement(string name, Transform parent)
    {
        GameObject element = new GameObject(name, typeof(RectTransform));
        element.transform.SetParent(parent, false);

        return (RectTransform)element.transform;
    }

    public static TextMeshProUGUI CreateLabel(Transform host, string text, float fontSize, Color color)
    {
        TextMeshProUGUI label = host.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;

        return label;
    }

    public static Slider CreateSlider(
        Transform parent,
        string name,
        DefaultControls.Resources resources,
        Color fillColor,
        Color backgroundColor)
    {
        GameObject sliderObject = DefaultControls.CreateSlider(resources);
        sliderObject.name = name;
        sliderObject.transform.SetParent(parent, false);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        Image background = sliderObject.transform.Find(SliderBackgroundName).GetComponent<Image>();
        background.color = backgroundColor;

        Image fill = sliderObject.transform.Find(SliderFillAreaFillName).GetComponent<Image>();
        fill.color = fillColor;

        return slider;
    }

    public static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public static void AnchorTop(RectTransform rect, Vector2 position, Vector2 size) =>
        Anchor(rect, TopAnchor, position, size);

    public static void AnchorCenter(RectTransform rect, Vector2 position, Vector2 size) =>
        Anchor(rect, CenterAnchor, position, size);

    public static void Anchor(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    public static DefaultControls.Resources CreateResources()
    {
        return new DefaultControls.Resources
        {
            standard = AssetDatabaseResource<Sprite>(UiSpritePath),
            background = AssetDatabaseResource<Sprite>(BackgroundSpritePath),
            inputField = AssetDatabaseResource<Sprite>(InputFieldSpritePath),
            knob = AssetDatabaseResource<Sprite>(KnobSpritePath),
            checkmark = AssetDatabaseResource<Sprite>(CheckmarkSpritePath),
            dropdown = AssetDatabaseResource<Sprite>(DropdownArrowSpritePath),
            mask = AssetDatabaseResource<Sprite>(UiMaskSpritePath)
        };
    }

    private static T AssetDatabaseResource<T>(string path) where T : Object
    {
        return UnityEditor.AssetDatabase.GetBuiltinExtraResource<T>(path);
    }
}
