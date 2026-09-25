using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuSceneBuilder
{
    private const string ScenePath = "Assets/MainMenu.unity";

    private const string TitleText = "CAVE RUNNER";
    private const string AuthorsTitleText = "Авторы";
    private const string AuthorsBodyText = "Разработчик:\nErnest Kozyrev\n\nУчебная 2D-платформера\nна Unity";
    private const string BackButtonText = "Назад";

    private const float WidthHeightMatchBalance = 0.5f;
    private const float MenuButtonFontSize = 40f;
    private const float TitleFontSize = 110f;
    private const float AuthorsTitleFontSize = 72f;
    private const float AuthorsBodyFontSize = 36f;

    private static readonly Color MenuBackgroundColor = new Color(0.08f, 0.10f, 0.16f);
    private static readonly Color MenuPanelColor = new Color(0f, 0f, 0f, 0.45f);
    private static readonly Color AuthorsPanelColor = new Color(0.06f, 0.07f, 0.12f, 1f);
    private static readonly Color TitleColor = new Color(0.85f, 0.20f, 0.32f);
    private static readonly Color BodyColor = new Color(0.85f, 0.85f, 0.85f);
    private static readonly Color ButtonColor = new Color(0.13f, 0.13f, 0.16f, 0.98f);

    private static readonly string[] MenuLabels = { "Играть", "Об авторах", "Выход" };

    [MenuItem("Tools/Main Menu/Build Scene")]
    public static void BuildFromMenu()
    {
        Build();
    }

    public static void Build()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() == false)
        {
            return;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        EditorUiSceneUtility.CreateCamera(MenuBackgroundColor);

        GameObject root = new GameObject("MainMenu");
        MainMenu menu = root.AddComponent<MainMenu>();

        Canvas canvas = EditorUiSceneUtility.CreateCanvas(root.transform, WidthHeightMatchBalance);
        EditorUiSceneUtility.CreateEventSystem();

        DefaultControls.Resources resources = EditorUiSceneUtility.CreateResources();

        CreateBackground(canvas.transform);
        CreateTitle(canvas.transform, TitleText, TitleFontSize, new Vector2(0f, 330f));
        CreateMenuPanel(canvas.transform);

        WireMenuButton(menu, "_playButton", CreateMenuButton(canvas.transform, MenuLabels[0], new Vector2(0f, -60f), resources));
        WireMenuButton(menu, "_authorsButton", CreateMenuButton(canvas.transform, MenuLabels[1], new Vector2(0f, -200f), resources));
        WireMenuButton(menu, "_quitButton", CreateMenuButton(canvas.transform, MenuLabels[2], new Vector2(0f, -340f), resources));

        GameObject authorsPanel = CreateAuthorsPanel(canvas.transform, menu);
        authorsPanel.SetActive(false);

        SerializedPropertyUtility.SetObjectReference(menu, "_authorsPanel", authorsPanel);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        UpdateBuildSettings();

        Debug.Log($"[MainMenuSceneBuilder] Scene saved to {ScenePath} and placed first in Build Settings.");
    }

    private static void UpdateBuildSettings()
    {
        System.Collections.Generic.List<EditorBuildSettingsScene> scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();

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
        RectTransform rect = EditorUiSceneUtility.CreateElement("Background", parent);
        EditorUiSceneUtility.Stretch(rect);

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = MenuBackgroundColor;
    }

    private static void CreateMenuPanel(Transform parent)
    {
        RectTransform rect = EditorUiSceneUtility.CreateElement("MenuPanel", parent);
        AnchorCenter(rect, new Vector2(0f, -130f), new Vector2(640f, 740f));

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = MenuPanelColor;
    }

    private static GameObject CreateAuthorsPanel(Transform parent, MainMenu menu)
    {
        GameObject panel = EditorUiSceneUtility.CreateElement("AuthorsPanel", parent).gameObject;
        EditorUiSceneUtility.Stretch((RectTransform)panel.transform);

        Image image = panel.AddComponent<Image>();
        image.color = AuthorsPanelColor;

        CreateTitle(panel.transform, AuthorsTitleText, AuthorsTitleFontSize, new Vector2(0f, 260f));

        RectTransform body = EditorUiSceneUtility.CreateElement("Body", panel.transform);
        AnchorCenter(body, Vector2.zero, new Vector2(900f, 400f));

        TextMeshProUGUI bodyLabel = body.gameObject.AddComponent<TextMeshProUGUI>();
        bodyLabel.font = TMP_Settings.defaultFontAsset;
        bodyLabel.text = AuthorsBodyText;
        bodyLabel.fontSize = AuthorsBodyFontSize;
        bodyLabel.color = BodyColor;
        bodyLabel.alignment = TextAlignmentOptions.Center;

        DefaultControls.Resources resources = EditorUiSceneUtility.CreateResources();
        WireMenuButton(menu, "_backButton", CreateMenuButton(panel.transform, BackButtonText, new Vector2(0f, -320f), resources));

        return panel;
    }

    private static GameObject CreateMenuButton(Transform parent, string label, Vector2 position,
        DefaultControls.Resources resources)
    {
        GameObject buttonObject = DefaultControls.CreateButton(resources);
        buttonObject.name = $"{label}Button";
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = (RectTransform)buttonObject.transform;
        AnchorCenter(rect, position, new Vector2(460f, 90f));

        Image image = buttonObject.GetComponent<Image>();
        image.color = ButtonColor;

        Button button = buttonObject.GetComponent<Button>();
        button.transition = Selectable.Transition.None;

        Text buttonText = buttonObject.GetComponentInChildren<Text>();
        buttonText.text = label;
        buttonText.fontSize = (int)MenuButtonFontSize;
        buttonText.color = Color.white;

        MenuButton menuButton = buttonObject.AddComponent<MenuButton>();
        SerializedPropertyUtility.SetObjectReference(menuButton, "_image", image);

        return buttonObject;
    }

    private static void WireMenuButton(MainMenu menu, string propertyName, GameObject buttonObject)
    {
        Button button = buttonObject.GetComponent<Button>();
        SerializedPropertyUtility.SetObjectReference(menu, propertyName, button);
    }

    private static void CreateTitle(Transform parent, string text, float fontSize, Vector2 position)
    {
        RectTransform rect = EditorUiSceneUtility.CreateElement($"{text}Title", parent);
        AnchorCenter(rect, position, new Vector2(900f, 120f));

        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = FontStyles.Bold;
        label.color = TitleColor;
        label.alignment = TextAlignmentOptions.Center;
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
