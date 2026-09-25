using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class EditorUiSceneUtility
{
    private const string CanvasName = "Canvas";
    private const string CameraName = "Main Camera";
    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;

    public static Canvas CreateCanvas(Transform parent, float matchWidthOrHeight)
    {
        GameObject canvasObject = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = backgroundColor;
    }

    public static void CreateEventSystem()
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    public static RectTransform CreateElement(string name, Transform parent)
    {
        GameObject element = new GameObject(name, typeof(RectTransform));
        element.transform.SetParent(parent, false);

        return (RectTransform)element.transform;
    }

    public static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public static DefaultControls.Resources CreateResources()
    {
        return new DefaultControls.Resources
        {
            standard = AssetDatabaseResource<Sprite>(HealthDemoConstants.UiSpritePath),
            background = AssetDatabaseResource<Sprite>("UI/Skin/Background.psd"),
            inputField = AssetDatabaseResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
            knob = AssetDatabaseResource<Sprite>(HealthDemoConstants.KnobSpritePath),
            checkmark = AssetDatabaseResource<Sprite>("UI/Skin/Checkmark.psd"),
            dropdown = AssetDatabaseResource<Sprite>("UI/Skin/DropdownArrow.psd"),
            mask = AssetDatabaseResource<Sprite>("UI/Skin/UIMask.psd")
        };
    }

    private static T AssetDatabaseResource<T>(string path) where T : Object
    {
        return UnityEditor.AssetDatabase.GetBuiltinExtraResource<T>(path);
    }
}
