using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class VampirismInstaller
{
    private const string ScenePath = "Assets/SampleScene.unity";
    private const string ZoneName = "VampirismZone";
    private const string BarName = "VampirismBar";
    private const string CaptionName = "VampirismCaption";
    private const string CaptionText = "Вампиризм (E)";
    private const string HandleAreaName = "Handle Slide Area";
    private const string CoinTextName = "CoinText";
    private const float ZoneHeightOffset = 0.6f;
    private const float KnobWorldSize = 0.26f;
    private const float DiameterScale = 2f;
    private const int ZoneSortingOrder = -1;

    [MenuItem("Tools/Health Display/Install Vampirism Into SampleScene")]
    public static void InstallFromMenu()
    {
        Install();
    }

    public static void Install()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        Player player = Object.FindFirstObjectByType<Player>();

        if (player == null)
        {
            Debug.LogError($"[VampirismInstaller] Player not found in {ScenePath}.");
            return;
        }

        Vampirism vampirism = player.GetComponent<Vampirism>();

        if (vampirism == null)
        {
            vampirism = player.gameObject.AddComponent<Vampirism>();
        }

        SerializedPropertyUtility.SetLayerMask(vampirism, "_targetLayer", LayerMask.NameToLayer("Enemy"));

        VampirismZone zone = CreateZone(player, vampirism);
        VampirismView view = CreateHudBar();

        if (zone == null)
        {
            Debug.LogError($"[VampirismInstaller] Zone was not created in {ScenePath}.");
            return;
        }

        SerializedPropertyUtility.SetObjectReference(player, "_vampirismZone", zone);

        if (view == null)
        {
            Debug.LogError($"[VampirismInstaller] View was not created in {ScenePath}.");
            return;
        }

        GameSession session = Object.FindFirstObjectByType<GameSession>();

        if (session == null)
        {
            Debug.LogError($"[VampirismInstaller] GameSession not found in {ScenePath}.");
            return;
        }

        SerializedPropertyUtility.SetObjectReference(session, "_vampirismView", view);

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"[VampirismInstaller] Vampirism installed into {ScenePath}.");
    }

    private static VampirismZone CreateZone(Player player, Vampirism vampirism)
    {
        Transform character = player.transform;
        Transform zoneTransform = character.Find(ZoneName);

        if (zoneTransform == null)
        {
            GameObject zone = new GameObject(ZoneName, typeof(SpriteRenderer), typeof(VampirismZone));
            zone.transform.SetParent(character, false);
            zone.transform.localPosition = new Vector3(0f, ZoneHeightOffset, 0f);

            SpriteRenderer renderer = zone.GetComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            renderer.color = new Color(0.65f, 0.20f, 0.85f, 0.25f);
            renderer.sortingOrder = ZoneSortingOrder;

            float spriteSize = GetSpriteWorldSize(renderer.sprite);
            zone.transform.localScale = Vector3.one * (vampirism.Radius * DiameterScale / spriteSize);

            zoneTransform = zone.transform;
        }

        return zoneTransform.GetComponent<VampirismZone>();
    }

    private static float GetSpriteWorldSize(Sprite sprite)
    {
        if (sprite == null)
        {
            return KnobWorldSize;
        }

        if (sprite.bounds.size.x <= 0f)
        {
            return KnobWorldSize;
        }

        return sprite.bounds.size.x;
    }

    private static VampirismView CreateHudBar()
    {
        Canvas hud = FindHudCanvas();

        if (hud == null)
        {
            Debug.LogError($"[VampirismInstaller] HUD canvas not found in {ScenePath}.");
            return null;
        }

        Transform existingBar = hud.transform.Find(BarName);

        if (existingBar == null)
        {
            return CreateSliderView(hud.transform);
        }

        return existingBar.GetComponent<VampirismView>();
    }

    private static VampirismView CreateSliderView(Transform hud)
    {
        DefaultControls.Resources resources = HealthDisplayDemoBuilder.CreateResources();

        GameObject sliderObject = DefaultControls.CreateSlider(resources);
        sliderObject.name = BarName;
        sliderObject.transform.SetParent(hud, false);

        RectTransform rect = (RectTransform)sliderObject.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = new Vector2(50f, 60f);
        rect.sizeDelta = new Vector2(420f, 36f);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.maxValue = 1f;
        slider.value = 1f;

        SerializedPropertyUtility.DestroyChildIfExists(sliderObject.transform, HandleAreaName);

        RectTransform fillArea = (RectTransform)sliderObject.transform.Find("Fill Area");
        fillArea.anchorMin = Vector2.zero;
        fillArea.anchorMax = Vector2.one;
        fillArea.offsetMin = new Vector2(4f, fillArea.offsetMin.y);
        fillArea.offsetMax = new Vector2(-4f, fillArea.offsetMax.y);

        Image background = sliderObject.transform.Find("Background").GetComponent<Image>();
        background.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

        Image fill = sliderObject.transform.Find("Fill Area/Fill").GetComponent<Image>();
        fill.color = new Color(0.78f, 0.16f, 0.30f);

        CreateCaption(hud, new Vector2(50f, 100f));

        VampirismView view = sliderObject.AddComponent<VampirismView>();
        SerializedPropertyUtility.SetObjectReference(view, "_slider", slider);

        return view;
    }

    private static Canvas FindHudCanvas()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);

        foreach (Canvas canvas in canvases)
        {
            Transform coinText = canvas.transform.Find(CoinTextName);

            if (coinText == null)
            {
                continue;
            }

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return canvas;
            }
        }

        return null;
    }

    private static void CreateCaption(Transform parent, Vector2 position)
    {
        RectTransform rect = new GameObject(CaptionName, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(420f, 34f);

        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = CaptionText;
        label.fontSize = 26f;
        label.color = new Color(0.9f, 0.85f, 0.9f);
        label.alignment = TextAlignmentOptions.Left;
    }
}
