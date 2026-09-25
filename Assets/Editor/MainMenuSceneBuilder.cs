using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuSceneBuilder
{
    private const string ScenePath = "Assets/MainMenu.unity";
    private const string GameScenePath = "Assets/SampleScene.unity";
    private const string DemoScenePath = "Assets/HealthDisplayDemo.unity";

    private const string TitleText = "CAVE RUNNER";
    private const string AuthorsTitleText = "Авторы";
    private const string AuthorsBodyText = "Разработчик:\nErnest Kozyrev\n\nУчебная 2D-платформера\nна Unity";
    private const string BackButtonText = "Назад";

    private const float WidthHeightMatchBalance = 0.5f;
    private const float MenuButtonFontSize = 40f;

    private static readonly string[] MenuLabels = { "Играть", "Об авторах", "Выход" };

    [MenuItem("Tools/Main Menu/Build Scene")]
    public static void BuildFromMenu()
    {
        Build();
    }

    public static void Build()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();

        GameObject root = new GameObject("MainMenu");
        MainMenu menu = root.AddComponent<MainMenu>();

        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(root.transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = WidthHeightMatchBalance;

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        DefaultControls.Resources resources = HealthDemoSceneBuilder.CreateResources();

        CreateBackground(canvas.transform);
        CreateTitle(canvas.transform, TitleText, 110f, new Vector2(0f, 330f));
        CreateMenuPanel(canvas.transform);

        CreateMenuButton(canvas.transform, MenuLabels[0], new Vector2(0f, -60f), resources, menu.PlayGame);
        CreateMenuButton(canvas.transform, MenuLabels[1], new Vector2(0f, -200f), resources, menu.ShowAuthors);
        CreateMenuButton(canvas.transform, MenuLabels[2], new Vector2(0f, -340f), resources, menu.QuitGame);

        GameObject authorsPanel = CreateAuthorsPanel(canvas.transform, menu);
        authorsPanel.SetActive(false);

        SerializedPropertyUtility.SetObjectReference(menu, "_authorsPanel", authorsPanel);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        UpdateBuildSettings();

        Debug.Log($"[MainMenuSceneBuilder] Scene saved to {ScenePath} and placed first in Build Settings.");
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.10f, 0.16f);
        camera.cullingMask = ~0;
    }

    private static void UpdateBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

        foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
        {
            if (existing.path == ScenePath)
            {
                continue;
            }

            scenes.Add(existing);
        }

        scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void CreateBackground(Transform parent)
    {
        RectTransform rect = CreateElement("Background", parent);
        Stretch(rect);

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.08f, 0.10f, 0.16f);
    }

    private static void CreateMenuPanel(Transform parent)
    {
        RectTransform rect = CreateElement("MenuPanel", parent);
        AnchorCenter(rect, new Vector2(0f, -130f), new Vector2(640f, 740f));

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.45f);
    }

    private static GameObject CreateAuthorsPanel(Transform parent, MainMenu menu)
    {
        GameObject panel = CreateElement("AuthorsPanel", parent).gameObject;
        Stretch((RectTransform)panel.transform);

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.06f, 0.07f, 0.12f, 1f);

        CreateTitle(panel.transform, AuthorsTitleText, 72f, new Vector2(0f, 260f));

        RectTransform body = CreateElement("Body", panel.transform);
        AnchorCenter(body, Vector2.zero, new Vector2(900f, 400f));

        TextMeshProUGUI bodyLabel = body.gameObject.AddComponent<TextMeshProUGUI>();
        bodyLabel.font = TMP_Settings.defaultFontAsset;
        bodyLabel.text = AuthorsBodyText;
        bodyLabel.fontSize = 36f;
        bodyLabel.color = new Color(0.85f, 0.85f, 0.85f);
        bodyLabel.alignment = TextAlignmentOptions.Center;

        DefaultControls.Resources resources = HealthDemoSceneBuilder.CreateResources();
        CreateMenuButton(panel.transform, BackButtonText, new Vector2(0f, -320f), resources, menu.HideAuthors);

        return panel;
    }

    private static GameObject CreateMenuButton(Transform parent, string label, Vector2 position,
        DefaultControls.Resources resources, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = DefaultControls.CreateButton(resources);
        buttonObject.name = $"{label}Button";
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = (RectTransform)buttonObject.transform;
        AnchorCenter(rect, position, new Vector2(460f, 90f));

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.13f, 0.13f, 0.16f, 0.98f);

        Button button = buttonObject.GetComponent<Button>();
        button.transition = Selectable.Transition.None;

        Text buttonText = buttonObject.GetComponentInChildren<Text>();
        buttonText.text = label;
        buttonText.fontSize = (int)MenuButtonFontSize;
        buttonText.color = Color.white;

        MenuButton menuButton = buttonObject.AddComponent<MenuButton>();
        SerializedPropertyUtility.SetObjectReference(menuButton, "_targetImage", image);

        UnityEventTools.AddPersistentListener(button.onClick, onClick);

        return buttonObject;
    }

    private static void CreateTitle(Transform parent, string text, float fontSize, Vector2 position)
    {
        RectTransform rect = CreateElement($"{text}Title", parent);
        AnchorCenter(rect, position, new Vector2(900f, 120f));

        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = FontStyles.Bold;
        label.color = new Color(0.85f, 0.20f, 0.32f);
        label.alignment = TextAlignmentOptions.Center;
    }

    private static RectTransform CreateElement(string name, Transform parent)
    {
        GameObject element = new GameObject(name, typeof(RectTransform));
        element.transform.SetParent(parent, false);

        return (RectTransform)element.transform;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void AnchorCenter(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
