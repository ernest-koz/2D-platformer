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
    private const string InstantBarCaption = "Бар здоровья";
    private const string SmoothBarCaption = "Плавный бар здоровья";
    private const string DamageButtonCaption = "Урон -10";
    private const string HealButtonCaption = "Лечение +10";
    private const string LabelName = "Label";
    private const string CaptionName = "Caption";
    private const float CaptionLift = 16f;
    private const float BarCaptionLift = 40f;
    private const float HealthTextFontSize = 54f;
    private const float BarCaptionFontSize = 26f;
    private const float ButtonCaptionFontSize = 32f;
    private const float MatchWidth = 0f;

    private static readonly Color WoodenTint = new Color(0.7547f, 0.6372f, 0.6372f);
    private static readonly Color SceneBackgroundColor = new Color(0.08f, 0.08f, 0.10f);
    private static readonly Color PanelColor = new Color(0f, 0f, 0f, 0.55f);
    private static readonly Color InstantBarFillColor = new Color(0.30f, 0.80f, 0.35f);
    private static readonly Color SmoothBarFillColor = new Color(0.95f, 0.62f, 0.20f);
    private static readonly Color CaptionColor = new Color(0.82f, 0.82f, 0.82f);
    private static readonly Vector2 PanelPosition = new Vector2(0f, -110f);
    private static readonly Vector2 PanelSize = new Vector2(1240f, 860f);
    private static readonly Vector2 HealthTextPosition = new Vector2(0f, -80f);
    private static readonly Vector2 HealthTextSize = new Vector2(500f, 70f);
    private static readonly Vector2 BarSliderSize = new Vector2(1000f, 50f);
    private static readonly Vector2 BarCaptionSize = new Vector2(400f, 30f);
    private static readonly Vector2 ButtonSize = new Vector2(336f, 112f);
    private static readonly Vector2 DamageButtonPosition = new Vector2(-230f, -560f);
    private static readonly Vector2 HealButtonPosition = new Vector2(230f, -560f);
    private static readonly BarLayout InstantBarLayout = new BarLayout(
        HealthDemoConstants.InstantBarName,
        InstantBarCaption,
        new Vector2(0f, -240f),
        InstantBarFillColor);
    private static readonly BarLayout SmoothBarLayout = new BarLayout(
        HealthDemoConstants.SmoothBarName,
        SmoothBarCaption,
        new Vector2(0f, -380f),
        SmoothBarFillColor);

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
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() == false)
        {
            return;
        }

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

        Canvas canvas = EditorUiSceneUtility.CreateCanvas(root.transform, MatchWidth);
        DefaultControls.Resources resources = EditorUiSceneUtility.CreateResources();

        RectTransform panel = CreatePanel(canvas.transform);

        CreateHealthText(panel, health);
        CreateInstantBar(panel, health, InstantBarLayout, resources);
        CreateSmoothBar(panel, health, SmoothBarLayout, resources);

        Button damageButton = CreateButton(
            panel, HealthDemoConstants.DamageButtonName, DamageButtonCaption, DamageButtonPosition);
        DamageButton damageAction = damageButton.gameObject.AddComponent<DamageButton>();
        SerializedPropertyUtility.SetObjectReference(damageAction, "_health", health);
        SerializedPropertyUtility.SetObjectReference(damageAction, "_button", damageButton);
        SerializedPropertyUtility.SetInteger(damageAction, "_amount", HealthDemoConstants.SimulatedDamage);

        Button healButton = CreateButton(
            panel, HealthDemoConstants.HealButtonName, HealButtonCaption, HealButtonPosition);
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
        EditorUiSceneUtility.AnchorTop(rectTransform, PanelPosition, PanelSize);

        Image image = rectTransform.gameObject.AddComponent<Image>();
        image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(EditorUiSceneUtility.UiSpritePath);
        image.type = Image.Type.Sliced;
        image.color = PanelColor;

        return rectTransform;
    }

    private static void CreateHealthText(Transform parent, Health health)
    {
        RectTransform rectTransform = EditorUiSceneUtility.CreateElement(HealthDemoConstants.HealthTextName, parent);
        EditorUiSceneUtility.AnchorTop(rectTransform, HealthTextPosition, HealthTextSize);

        TextMeshProUGUI label = EditorUiSceneUtility.CreateLabel(
            rectTransform,
            $"{health.Maximum}/{health.Maximum}",
            HealthTextFontSize,
            Color.white);

        HealthText view = rectTransform.gameObject.AddComponent<HealthText>();
        SerializedPropertyUtility.SetObjectReference(view, "_health", health);
        SerializedPropertyUtility.SetObjectReference(view, "_text", label);
    }

    private static void CreateInstantBar(
        Transform parent,
        Health health,
        BarLayout layout,
        DefaultControls.Resources resources)
    {
        Slider slider = CreateBarSlider(parent, layout, resources);

        HealthBar view = slider.gameObject.AddComponent<HealthBar>();
        WireBarView(view, health, slider);
    }

    private static void CreateSmoothBar(
        Transform parent,
        Health health,
        BarLayout layout,
        DefaultControls.Resources resources)
    {
        Slider slider = CreateBarSlider(parent, layout, resources);

        SmoothHealthBar view = slider.gameObject.AddComponent<SmoothHealthBar>();
        SerializedPropertyUtility.SetFloat(view, "_fillDuration", HealthDemoConstants.SmoothFillDuration);
        WireBarView(view, health, slider);
    }

    private static void WireBarView(HealthBar view, Health health, Slider slider)
    {
        SerializedPropertyUtility.SetObjectReference(view, "_health", health);
        SerializedPropertyUtility.SetObjectReference(view, "_slider", slider);
    }

    private static Slider CreateBarSlider(Transform parent, BarLayout layout, DefaultControls.Resources resources)
    {
        CreateBarCaption(parent, layout.Name, layout.Caption, layout.Position + Vector2.up * BarCaptionLift);

        Slider slider = EditorUiSceneUtility.CreateSlider(
            parent,
            layout.Name,
            resources,
            layout.FillColor,
            EditorUiSceneUtility.SliderBackgroundColor);
        EditorUiSceneUtility.AnchorTop((RectTransform)slider.transform, layout.Position, BarSliderSize);

        return slider;
    }

    private static void CreateBarCaption(Transform parent, string hostName, string text, Vector2 position)
    {
        RectTransform rectTransform = EditorUiSceneUtility.CreateElement($"{hostName}Caption", parent);
        EditorUiSceneUtility.AnchorTop(rectTransform, position, BarCaptionSize);

        RectTransform labelHost = EditorUiSceneUtility.CreateElement(LabelName, rectTransform);
        EditorUiSceneUtility.Stretch(labelHost);
        EditorUiSceneUtility.CreateLabel(labelHost, text, BarCaptionFontSize, CaptionColor);
    }

    private static Button CreateButton(Transform parent, string name, string caption, Vector2 position)
    {
        GameObject buttonObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button),
            typeof(HoverCursor));
        RectTransform rectTransform = (RectTransform)buttonObject.transform;
        rectTransform.SetParent(parent, false);
        EditorUiSceneUtility.AnchorTop(rectTransform, position, ButtonSize);

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
        SerializedPropertyUtility.SetObjectReference(
            hoverCursor, "_handCursor", LoadAssetOrThrow<Texture2D>(HandCursorPath));

        RectTransform captionHost = EditorUiSceneUtility.CreateElement(CaptionName, rectTransform);
        EditorUiSceneUtility.Stretch(captionHost);
        captionHost.offsetMin = new Vector2(0f, CaptionLift);
        EditorUiSceneUtility.CreateLabel(captionHost, caption, ButtonCaptionFontSize, Color.white);

        return button;
    }

    private readonly struct BarLayout
    {
        public BarLayout(string name, string caption, Vector2 position, Color fillColor)
        {
            Name = name;
            Caption = caption;
            Position = position;
            FillColor = fillColor;
        }

        public string Name { get; }
        public string Caption { get; }
        public Vector2 Position { get; }
        public Color FillColor { get; }
    }
}
