using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class HealthDemoSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/HealthDemo.unity";

    private static readonly Color WoodenTint = new Color(0.7547f, 0.6372f, 0.6372f);
    private static readonly Color SceneBackgroundColor = new Color(0.08f, 0.08f, 0.10f);
    private static readonly Color PanelColor = new Color(0f, 0f, 0f, 0.55f);

    [MenuItem("Tools/Health Demo/Build Demo Scene")]
    public static void BuildFromMenu()
    {
        Build();
    }

    [MenuItem("Tools/Health Demo/Validate Demo Scene")]
    public static void ValidateFromMenu()
    {
        ValidateSavedScene();
    }

    private static void ValidateSavedScene()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        HealthDemoSceneValidator.Validate(ScenePath);
    }

    private static void Build()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() == false)
        {
            return;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();

        GameObject root = new GameObject("HealthDemo");
        root.SetActive(false);

        Health health = root.AddComponent<Health>();
        SerializedPropertyUtility.SetInteger(health, "_maximum", HealthDemoConstants.MaximumHealth);

        Canvas canvas = CreateCanvas(root.transform);
        DefaultControls.Resources resources = CreateResources();

        RectTransform panel = CreatePanel(canvas.transform);

        CreateHealthText(panel, health);
        CreateBar(panel, "InstantHealthBar", "Бар здоровья", new Vector2(0f, -240f),
            new Color(0.30f, 0.80f, 0.35f), resources, health, false);
        CreateBar(panel, "SmoothHealthBar", "Плавный бар здоровья", new Vector2(0f, -380f),
            new Color(0.95f, 0.62f, 0.20f), resources, health, true);

        Button damageButton = CreateButton(panel, "DamageButton", "Урон -10", new Vector2(-230f, -560f));
        DamageButton damageAction = damageButton.gameObject.AddComponent<DamageButton>();
        SerializedPropertyUtility.SetObjectReference(damageAction, "_health", health);
        SerializedPropertyUtility.SetObjectReference(damageAction, "_button", damageButton);
        SerializedPropertyUtility.SetInteger(damageAction, "_amount", HealthDemoConstants.SimulatedDamage);

        Button healButton = CreateButton(panel, "HealButton", "Лечение +10", new Vector2(230f, -560f));
        HealButton healAction = healButton.gameObject.AddComponent<HealButton>();
        SerializedPropertyUtility.SetObjectReference(healAction, "_health", health);
        SerializedPropertyUtility.SetObjectReference(healAction, "_button", healButton);
        SerializedPropertyUtility.SetInteger(healAction, "_amount", HealthDemoConstants.SimulatedHeal);

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        root.SetActive(true);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        AddSceneToBuildSettings();

        Debug.Log($"[HealthDemoSceneBuilder] Scene saved to {ScenePath}.");
    }

    private static void AddSceneToBuildSettings()
    {
        if (ContainsScene(EditorBuildSettings.scenes))
        {
            return;
        }

        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static bool ContainsScene(EditorBuildSettingsScene[] scenes)
    {
        foreach (EditorBuildSettingsScene scene in scenes)
        {
            if (scene.path == ScenePath)
            {
                return true;
            }
        }

        return false;
    }

    private static T LoadAssetOrThrow<T>(string path) where T : Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);

        if (asset == null)
        {
            throw new InvalidOperationException($"[HealthDemoSceneBuilder] Asset not found: {path}");
        }

        return asset;
    }

    private static Canvas CreateCanvas(Transform parent)
    {
        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(parent, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        return canvas;
    }

    private static RectTransform CreatePanel(Transform parent)
    {
        RectTransform rectTransform = CreateElement("Panel", parent);
        AnchorTop(rectTransform, new Vector2(0f, -110f), new Vector2(1240f, 860f));

        Image image = rectTransform.gameObject.AddComponent<Image>();
        image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        image.type = Image.Type.Sliced;
        image.color = PanelColor;

        return rectTransform;
    }

    private static void CreateHealthText(Transform parent, Health health)
    {
        RectTransform rectTransform = CreateElement("HealthText", parent);
        AnchorTop(rectTransform, new Vector2(0f, -80f), new Vector2(500f, 70f));

        TextMeshProUGUI label = rectTransform.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = $"{health.Maximum}/{health.Maximum}";
        label.fontSize = 54f;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;

        HealthText view = rectTransform.gameObject.AddComponent<HealthText>();
        SerializedPropertyUtility.SetObjectReference(view, "_health", health);
        SerializedPropertyUtility.SetObjectReference(view, "_text", label);
    }

    private static void CreateBar(Transform parent, string name, string caption, Vector2 position,
        Color fillColor, DefaultControls.Resources resources, Health health, bool isSmooth)
    {
        CreateBarCaption(parent, name, caption, new Vector2(position.x, position.y + 40f));

        GameObject sliderObject = DefaultControls.CreateSlider(resources);
        sliderObject.name = name;
        sliderObject.transform.SetParent(parent, false);

        RectTransform rectTransform = (RectTransform)sliderObject.transform;
        AnchorTop(rectTransform, position, new Vector2(1000f, 50f));

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        Image background = sliderObject.transform.Find("Background").GetComponent<Image>();
        background.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

        Image fill = sliderObject.transform.Find("Fill Area/Fill").GetComponent<Image>();
        fill.color = fillColor;

        HealthBar view;
        if (isSmooth)
        {
            view = sliderObject.AddComponent<SmoothHealthBar>();
            SerializedPropertyUtility.SetFloat(view, "_fillDuration", HealthDemoConstants.SmoothFillDuration);
        }
        else
        {
            view = sliderObject.AddComponent<HealthBar>();
        }

        SerializedPropertyUtility.SetObjectReference(view, "_health", health);
        SerializedPropertyUtility.SetObjectReference(view, "_slider", slider);
    }

    private static void CreateCaption(Transform parent, string name, string text, float fontSize, Color color, float bottomOffset)
    {
        RectTransform rectTransform = CreateElement(name, parent);
        Stretch(rectTransform);
        rectTransform.offsetMin = new Vector2(0f, bottomOffset);

        TextMeshProUGUI label = rectTransform.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
    }

    private static void CreateBarCaption(Transform parent, string hostName, string text, Vector2 position)
    {
        RectTransform rectTransform = CreateElement($"{hostName}Caption", parent);
        AnchorTop(rectTransform, position, new Vector2(400f, 30f));
        CreateCaption(rectTransform, "Label", text, 26f, new Color(0.82f, 0.82f, 0.82f), 0f);
    }

    private static void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = SceneBackgroundColor;
    }

    private static Button CreateButton(Transform parent, string name, string caption, Vector2 position)
    {
        const string NormalSpritePath = "Assets/My Assets/Fantasy Wooden GUI/TextBTN_Big.png";
        const string PressedSpritePath = "Assets/My Assets/Fantasy Wooden GUI/TextBTN_Big_Pressed.png";
        const string HandCursorPath = "Assets/My Assets/UI/hand_cursor.png";
        const float CaptionLift = 16f;

        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(HoverCursor));
        RectTransform rectTransform = (RectTransform)buttonObject.transform;
        rectTransform.SetParent(parent, false);
        AnchorTop(rectTransform, position, new Vector2(336f, 112f));

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = LoadAssetOrThrow<Sprite>(NormalSpritePath);
        image.color = WoodenTint;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.spriteState = new SpriteState
        {
            pressedSprite = LoadAssetOrThrow<Sprite>(PressedSpritePath)
        };

        HoverCursor hoverCursor = buttonObject.GetComponent<HoverCursor>();
        SerializedPropertyUtility.SetObjectReference(hoverCursor, "_handCursor", LoadAssetOrThrow<Texture2D>(HandCursorPath));

        CreateCaption(rectTransform, "Caption", caption, 32f, Color.white, CaptionLift);

        return button;
    }

    private static RectTransform CreateElement(string name, Transform parent)
    {
        GameObject element = new GameObject(name, typeof(RectTransform));
        element.transform.SetParent(parent, false);

        return (RectTransform)element.transform;
    }

    private static void AnchorTop(RectTransform rectTransform, Vector2 position, Vector2 size)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
    }

    public static DefaultControls.Resources CreateResources()
    {
        return new DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
            checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
            dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd"),
            mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd")
        };
    }
}
