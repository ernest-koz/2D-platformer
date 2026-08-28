using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class HealthBarWorldInstaller
{
    private const string ScenePath = "Assets/SampleScene.unity";
    private const string PrefabPath = "Assets/Prefabs/HealthBarWorld.prefab";
    private const string BarName = "HealthBarWorld";
    private const float HeadMargin = 0.25f;
    private const float FallbackHeadOffset = 1.2f;
    private const int SortingOrder = 10;
    private const float CanvasWidth = 160f;
    private const float CanvasHeight = 22f;
    private const float CanvasScale = 0.01f;
    private const float BarWorldWidth = 0.9f;
    private const float MinimalCharacterScale = 0.0001f;

    [MenuItem("Tools/Health Display/Install World Bars Into SampleScene")]
    public static void InstallFromMenu()
    {
        Install();
    }

    public static void Install()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

        if (prefab == null)
        {
            prefab = CreatePrefab();
        }

        int installed = 0;

        foreach (Transform character in CollectCharacters())
        {
            if (character.Find(BarName) != null)
            {
                continue;
            }

            GameObject bar = (GameObject)PrefabUtility.InstantiatePrefab(prefab, character);
            bar.name = BarName;

            Vector3 characterScale = GetSafeScale(character);
            float widthFactor = BarWorldWidth / (CanvasWidth * CanvasScale);
            bar.transform.localScale = new Vector3(
                widthFactor / characterScale.x,
                widthFactor / characterScale.y,
                widthFactor / characterScale.z);
            bar.transform.localPosition = new Vector3(
                0f,
                GetHeadOffset(character.gameObject) / characterScale.y,
                0f);
            installed++;
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"[HealthBarWorldInstaller] Installed {installed} health bars into {ScenePath}.");
    }

    private static IEnumerable<Transform> CollectCharacters()
    {
        Player player = Object.FindFirstObjectByType<Player>();

        if (player != null)
        {
            yield return player.transform;
        }

        foreach (EnemyBrain enemy in Object.FindObjectsByType<EnemyBrain>(FindObjectsSortMode.None))
        {
            yield return enemy.transform;
        }
    }

    private static Vector3 GetSafeScale(Transform character)
    {
        Vector3 scale = character.lossyScale;

        if (Mathf.Abs(scale.x) < MinimalCharacterScale)
        {
            scale.x = MinimalCharacterScale;
        }

        if (Mathf.Abs(scale.y) < MinimalCharacterScale)
        {
            scale.y = MinimalCharacterScale;
        }

        if (Mathf.Abs(scale.z) < MinimalCharacterScale)
        {
            scale.z = MinimalCharacterScale;
        }

        return scale;
    }

    private static float GetHeadOffset(GameObject character)
    {
        BoxCollider2D collider = character.GetComponent<BoxCollider2D>();

        if (collider == null)
        {
            return FallbackHeadOffset;
        }

        float colliderTop = (collider.offset.y + collider.size.y * 0.5f) * Mathf.Abs(character.transform.lossyScale.y);

        return colliderTop + HeadMargin;
    }

    private static GameObject CreatePrefab()
    {
        GameObject root = new GameObject(BarName);
        root.AddComponent<WorldBillboard>();

        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
        canvasObject.transform.SetParent(root.transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = SortingOrder;

        RectTransform canvasRect = (RectTransform)canvasObject.transform;
        canvasRect.sizeDelta = new Vector2(CanvasWidth, CanvasHeight);
        canvasRect.localScale = Vector3.one * CanvasScale;

        DefaultControls.Resources resources = HealthDisplayDemoBuilder.CreateResources();

        GameObject sliderObject = DefaultControls.CreateSlider(resources);
        sliderObject.name = "Slider";
        sliderObject.transform.SetParent(canvasRect, false);

        RectTransform sliderRect = (RectTransform)sliderObject.transform;
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.maxValue = 1f;
        slider.value = 1f;

        Transform handleArea = sliderObject.transform.Find("Handle Slide Area");

        if (handleArea != null)
        {
            Object.DestroyImmediate(handleArea.gameObject);
        }

        Image background = sliderObject.transform.Find("Background").GetComponent<Image>();
        background.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

        Image fill = sliderObject.transform.Find("Fill Area/Fill").GetComponent<Image>();
        fill.color = new Color(0.85f, 0.25f, 0.25f);

        SmoothHealthBar view = sliderObject.AddComponent<SmoothHealthBar>();
        SetReference(view, "_slider", slider);

        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        return prefabAsset;
    }

    private static void SetReference(Object target, string propertyName, Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        serialized.FindProperty(propertyName).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
