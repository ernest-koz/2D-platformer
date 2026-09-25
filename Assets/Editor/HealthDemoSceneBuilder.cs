using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class HealthDemoSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/HealthDemo.unity";
    private const string NormalSpritePath = "Assets/My Assets/Fantasy Wooden GUI/TextBTN_Big.png";
    private const string PressedSpritePath = "Assets/My Assets/Fantasy Wooden GUI/TextBTN_Big_Pressed.png";
    private const string HandCursorPath = "Assets/My Assets/UI/hand_cursor.png";
    private const float CaptionLift = 16f;

    private static readonly Color WoodenTint = new Color(0.7547f, 0.6372f, 0.6372f);
    private static readonly Color SceneBackgroundColor = new Color(0.08f, 0.08f, 0.10f);
    private static readonly Color PanelColor = new Color(0f, 0f, 0f, 0.55f);
    private static readonly Color InstantBarFillColor = new Color(0.30f, 0.80f, 0.35f);
    private static readonly Color SmoothBarFillColor = new Color(0.95f, 0.62f, 0.20f);
    private static readonly Color CaptionColor = new Color(0.82f, 0.82f, 0.82f);

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

        EditorUiSceneUtility.CreateCamera(SceneBackgroundColor);

        GameObject root = new GameObject(HealthDemoConstants.DemoRootName);
        root.SetActive(false);

        Health health = root.AddComponent<Health>();
        SerializedPropertyUtility.SetInteger(health, "_maximum", HealthDemoConstants.MaximumHealth);

        Canvas canvas = EditorUiSceneUtility.CreateCanvas(root.transform, 0f);
        DefaultControls.Resources resources = EditorUiSceneUtility.CreateResources();

        RectTransform panel = CreatePanel(canvas.transform);

        CreateHealthText(panel, health);
        CreateInstantBar(panel, HealthDemoConstants.InstantBarName, "Бар здоровья", new Vector2(0f, -240f), InstantBarFillColor, resources, health);
        CreateSmoothBar(panel, HealthDemoConstants.SmoothBarName, "Плавный бар здоровья", new Vector2(0f, -380f), SmoothBarFillColor, resources, health);

        Button damageButton = CreateButton(panel, HealthDemoConstants.DamageButtonName, "Урон -10", new Vector2(-230f, -560f));
        DamageButton damageAction = damageButton.gameObject.AddComponent<DamageButton>();
        SerializedPropertyUtility.SetObjectReference(damageAction, "_health", health);
        SerializedPropertyUtility.SetObjectReference(damageAction, "_button", damageButton);
        SerializedPropertyUtility.SetInteger(damageAction, "_amount", HealthDemoConstants.SimulatedDamage);

        Button healButton = CreateButton(panel, HealthDemoConstants.HealButtonName, "Лечение +10", new Vector2(230f, -560f));
        HealButton healAction = healButton.gameObject.AddComponent<HealButton>();
        SerializedPropertyUtility.SetObjectReference(healAction, "_health", health);
        SerializedPropertyUtility.SetObjectReference(healAction, "_button", healButton);
        SerializedPropertyUtility.SetInteger(healAction, "_amount", HealthDemoConstants.SimulatedHeal);

        EditorUiSceneUtility.CreateEventSystem();

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

    private static RectTransform CreatePanel(Transform parent)
    {
        RectTransform rectTransform = EditorUiSceneUtility.CreateElement(HealthDemoConstants.PanelName, parent);
        AnchorTop(rectTransform, new Vector2(0f, -110f), new Vector2(1240f, 860f));

        Image image = rectTransform.gameObject.AddComponent<Image>();
        image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(HealthDemoConstants.UiSpritePath);
        image.type = Image.Type.Sliced;
        image.color = PanelColor;

        return rectTransform;
    }

    private static void CreateHealthText(Transform parent, Health health)
    {
        RectTransform rectTransform = EditorUiSceneUtility.CreateElement(HealthDemoConstants.HealthTextName, parent);
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

    private static void CreateInstantBar(Transform parent, string name, string caption, Vector2 position,
        Color fillColor, DefaultControls.Resources resources, Health health)
    {
        Slider slider = CreateSliderCore(parent, name, caption, position, fillColor, resources);

        HealthBar view = slider.gameObject.AddComponent<HealthBar>();
        SerializedPropertyUtility.SetObjectReference(view, "_health", health);
        SerializedPropertyUtility.SetObjectReference(view, "_slider", slider);
    }

    private static void CreateSmoothBar(Transform parent, string name, string caption, Vector2 position,
        Color fillColor, DefaultControls.Resources resources, Health health)
    {
        Slider slider = CreateSliderCore(parent, name, caption, position, fillColor, resources);

        SmoothHealthBar view = slider.gameObject.AddComponent<SmoothHealthBar>();
        SerializedPropertyUtility.SetFloat(view, "_fillDuration", HealthDemoConstants.SmoothFillDuration);
        SerializedPropertyUtility.SetObjectReference(view, "_health", health);
        SerializedPropertyUtility.SetObjectReference(view, "_slider", slider);
    }

    private static Slider CreateSliderCore(Transform parent, string name, string caption, Vector2 position,
        Color fillColor, DefaultControls.Resources resources)
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

        Image background = sliderObject.transform.Find(HealthDemoConstants.SliderBackgroundName).GetComponent<Image>();
        background.color = HealthDemoConstants.SliderBackgroundColor;

        Image fill = sliderObject.transform.Find(HealthDemoConstants.SliderFillAreaFillName).GetComponent<Image>();
        fill.color = fillColor;

        return slider;
    }

    private static void CreateCaption(Transform parent, string name, string text, float fontSize, Color color, float bottomOffset)
    {
        RectTransform rectTransform = EditorUiSceneUtility.CreateElement(name, parent);
        EditorUiSceneUtility.Stretch(rectTransform);
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
        RectTransform rectTransform = EditorUiSceneUtility.CreateElement($"{hostName}Caption", parent);
        AnchorTop(rectTransform, position, new Vector2(400f, 30f));
        CreateCaption(rectTransform, "Label", text, 26f, CaptionColor, 0f);
    }

    private static Button CreateButton(Transform parent, string name, string caption, Vector2 position)
    {
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

    private static void AnchorTop(RectTransform rectTransform, Vector2 position, Vector2 size)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
    }
}
