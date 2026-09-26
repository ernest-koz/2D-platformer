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
    private const string CanvasName = "Canvas";
    private const string SliderName = "Slider";
    private const float HeadMargin = 0.25f;
    private const float FallbackHeadOffset = 1.2f;
    private const float Half = 0.5f;
    private const int SortingOrder = 10;
    private const float CanvasWidth = 160f;
    private const float CanvasHeight = 22f;
    private const float CanvasScale = 0.01f;
    private const float BarWorldWidth = 0.9f;
    private const float MinimalCharacterScale = 0.0001f;

    private static readonly Color BarFillColor = new Color(0.85f, 0.25f, 0.25f);

    [MenuItem("Tools/Health Display/Install World Bars Into SampleScene")]
    public static void InstallFromMenu()
    {
        Install();
    }

    private static void Install()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() == false)
        {
            return;
        }

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

        if (prefab == null)
        {
            prefab = CreatePrefab();
        }

        int installed = 0;
        DestroyExistingBars();

        foreach (Transform character in CollectCharacters())
        {
            InstallBar(character, prefab);
            installed++;
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"[HealthBarWorldInstaller] Installed {installed} health bars into {ScenePath}.");
    }

    private static void DestroyExistingBars()
    {
        Transform[] transforms = Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (Transform bar in transforms)
        {
            if (bar == null)
            {
                continue;
            }

            if (bar.name == BarName)
            {
                Object.DestroyImmediate(bar.gameObject);
            }
        }
    }

    private static void InstallBar(Transform character, GameObject prefab)
    {
        GameObject bar = (GameObject)PrefabUtility.InstantiatePrefab(prefab, character);
        bar.name = BarName;

        Slider slider = bar.GetComponentInChildren<Slider>();

        if (slider == null)
        {
            Debug.LogError($"[HealthBarWorldInstaller] Prefab {PrefabPath} has no Slider.", bar);
            return;
        }

        SmoothHealthBar view = slider.gameObject.AddComponent<SmoothHealthBar>();
        SerializedPropertyUtility.SetObjectReference(view, "_slider", slider);
        SerializedPropertyUtility.SetObjectReference(view, "_health", character.GetComponent<Health>());

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
    }

    private static IEnumerable<Transform> CollectCharacters()
    {
        foreach (EnemyBrain enemy in Object.FindObjectsByType<EnemyBrain>(FindObjectsSortMode.None))
        {
            yield return enemy.transform;
        }

        Player player = Object.FindFirstObjectByType<Player>();

        if (player == null)
        {
            yield break;
        }

        yield return player.transform;
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

        float colliderTop = (collider.offset.y + collider.size.y * Half) * Mathf.Abs(character.transform.lossyScale.y);

        return colliderTop + HeadMargin;
    }

    private static GameObject CreatePrefab()
    {
        GameObject root = new GameObject(BarName);
        root.AddComponent<WorldBillboard>();

        GameObject canvasObject = new GameObject(CanvasName, typeof(RectTransform), typeof(Canvas));
        canvasObject.transform.SetParent(root.transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = SortingOrder;

        RectTransform canvasRect = (RectTransform)canvasObject.transform;
        canvasRect.sizeDelta = new Vector2(CanvasWidth, CanvasHeight);
        canvasRect.localScale = Vector3.one * CanvasScale;

        DefaultControls.Resources resources = EditorUiSceneUtility.CreateResources();
        Slider slider = EditorUiSceneUtility.CreateSlider(
            canvasRect,
            SliderName,
            resources,
            BarFillColor,
            HealthDemoConstants.SliderBackgroundColor);
        EditorUiSceneUtility.Stretch((RectTransform)slider.transform);
        SerializedPropertyUtility.DestroyChildIfExists(
            slider.transform,
            EditorUiSceneUtility.SliderHandleAreaName);

        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        return prefabAsset;
    }
}
