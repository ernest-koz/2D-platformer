using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class HealthDisplayDemoBuilder
{
    private const string ScenePath = "Assets/HealthDisplayDemo.unity";
    private const string PackagePath = "HealthDisplay.unitypackage";
    private const int MaximumHealth = 100;
    private const int SimulatedDamage = 10;
    private const int SimulatedHeal = 10;

    private static readonly string[] PackageAssetPaths =
    {
        "Assets/Scripts/UI/HealthDisplay",
        "Assets/Prefabs/HealthBarWorld.prefab",
        ScenePath
    };

    [MenuItem("Tools/Health Display/Build Demo Scene")]
    public static void BuildFromMenu()
    {
        Build();
    }

    [MenuItem("Tools/Health Display/Validate Demo Scene")]
    public static void ValidateFromMenu()
    {
        ValidateSavedScene();
    }

    public static void ValidateSavedScene()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        HealthDemoSceneValidator.Validate(ScenePath);
    }

    public static void Build()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject root = new GameObject("HealthDemo");
        root.SetActive(false);

        Health health = root.AddComponent<Health>();
        SetInt(health, "_maximum", MaximumHealth);
        SetFloat(health, "_invincibilityTime", 0f);

        HealthSimulator simulator = root.AddComponent<HealthSimulator>();
        SetReference(simulator, "_health", health);
        SetInt(simulator, "_damageAmount", SimulatedDamage);
        SetInt(simulator, "_healAmount", SimulatedHeal);

        Canvas canvas = CreateCanvas(root.transform);
        DefaultControls.Resources resources = CreateResources();

        CreatePanel(canvas.transform);
        CreateHealthText(canvas.transform, health);
        CreateBar(canvas.transform, "InstantHealthBar", "Бар здоровья", new Vector2(0f, -215f),
            new Color(0.30f, 0.80f, 0.35f), resources, health, false);
        CreateBar(canvas.transform, "SmoothHealthBar", "Плавный бар здоровья", new Vector2(0f, -320f),
            new Color(0.95f, 0.62f, 0.20f), resources, health, true);
        CreateButton(canvas.transform, "DamageButton", "Урон -10", new Vector2(-170f, -430f),
            resources, simulator.TakeDamage);
        CreateButton(canvas.transform, "HealButton", "Лечение +10", new Vector2(170f, -430f),
            resources, simulator.Heal);

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        root.SetActive(true);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        AddSceneToBuildSettings();
        AssetDatabase.ExportPackage(PackageAssetPaths, PackagePath,
            ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);

        Debug.Log($"[HealthDisplayDemoBuilder] Scene saved to {ScenePath}, package exported to {PackagePath}.");
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

    private static void CreatePanel(Transform parent)
    {
        RectTransform panel = CreateElement("Panel", parent);
        AnchorTop(panel, new Vector2(0f, -40f), new Vector2(760f, 540f));

        Image image = panel.gameObject.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.45f);
    }

    private static void CreateHealthText(Transform parent, Health health)
    {
        RectTransform rect = CreateElement("HealthText", parent);
        AnchorTop(rect, new Vector2(0f, -80f), new Vector2(500f, 70f));

        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = $"{health.Maximum}/{health.Maximum}";
        label.fontSize = 54f;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;

        HealthText view = rect.gameObject.AddComponent<HealthText>();
        SetReference(view, "_health", health);
        SetReference(view, "_text", label);
    }

    private static void CreateBar(Transform parent, string name, string caption, Vector2 position,
        Color fillColor, DefaultControls.Resources resources, Health health, bool smooth)
    {
        CreateCaption(parent, caption, new Vector2(position.x, position.y + 40f));

        GameObject sliderObject = DefaultControls.CreateSlider(resources);
        sliderObject.name = name;
        sliderObject.transform.SetParent(parent, false);

        RectTransform rect = (RectTransform)sliderObject.transform;
        AnchorTop(rect, position, new Vector2(640f, 32f));

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.maxValue = MaximumHealth;
        slider.value = MaximumHealth;

        Image background = sliderObject.transform.Find("Background").GetComponent<Image>();
        background.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

        Image fill = sliderObject.transform.Find("Fill Area/Fill").GetComponent<Image>();
        fill.color = fillColor;

        HealthBar view;
        if (smooth)
        {
            view = sliderObject.AddComponent<SmoothHealthBar>();
        }
        else
        {
            view = sliderObject.AddComponent<HealthBar>();
        }

        SetReference(view, "_health", health);
        SetReference(view, "_slider", slider);
    }

    private static void CreateCaption(Transform parent, string text, Vector2 position)
    {
        RectTransform rect = CreateElement($"{text}Caption", parent);
        AnchorTop(rect, position, new Vector2(400f, 30f));

        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = text;
        label.fontSize = 26f;
        label.color = new Color(0.82f, 0.82f, 0.82f);
        label.alignment = TextAlignmentOptions.Center;
    }

    private static void CreateButton(Transform parent, string name, string label, Vector2 position,
        DefaultControls.Resources resources, UnityAction onClick)
    {
        GameObject buttonObject = DefaultControls.CreateButton(resources);
        buttonObject.name = name;
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = (RectTransform)buttonObject.transform;
        AnchorTop(rect, position, new Vector2(300f, 80f));

        Text buttonText = buttonObject.GetComponentInChildren<Text>();
        buttonText.text = label;
        buttonText.fontSize = 34;

        Button button = buttonObject.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(button.onClick, onClick);
    }

    private static RectTransform CreateElement(string name, Transform parent)
    {
        GameObject element = new GameObject(name, typeof(RectTransform));
        element.transform.SetParent(parent, false);

        return (RectTransform)element.transform;
    }

    private static void AnchorTop(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    internal static DefaultControls.Resources CreateResources()
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

    private static void SetReference(Object target, string propertyName, Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        serialized.FindProperty(propertyName).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetInt(Object target, string propertyName, int value)
    {
        SerializedObject serialized = new SerializedObject(target);
        serialized.FindProperty(propertyName).intValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFloat(Object target, string propertyName, float value)
    {
        SerializedObject serialized = new SerializedObject(target);
        serialized.FindProperty(propertyName).floatValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
