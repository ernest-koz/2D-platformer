using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class VampirismInstaller
{
    private const string ScenePath = "Assets/SampleScene.unity";
    private const string ZoneName = "VampirismZone";
    private const string BarName = "VampirismBar";
    private const string CoinTextName = "CoinText";
    private const string EnemyLayerName = "Enemy";
    private const float ZoneHeightOffset = 0.6f;
    private const float FallbackSpriteSize = 0.26f;
    private const float DiameterScale = 2f;
    private const int ZoneSortingOrder = -1;
    private static readonly Vector2 BarPosition = new Vector2(50f, 60f);
    private static readonly Vector2 BarSize = new Vector2(420f, 36f);

    private static readonly Color ZoneColor = new Color(0.65f, 0.20f, 0.85f, 0.25f);
    private static readonly Color BarFillColor = new Color(0.78f, 0.16f, 0.30f);

    [MenuItem("Tools/Health Display/Install Vampirism Into SampleScene")]
    public static void InstallFromMenu()
    {
        Install();
    }

    public static void Install()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() == false)
        {
            return;
        }

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        Player player = Object.FindFirstObjectByType<Player>();

        if (player == null)
        {
            Debug.LogError($"[VampirismInstaller] Player not found in {ScenePath}.");
            return;
        }

        int enemyLayer = LayerMask.NameToLayer(EnemyLayerName);

        if (enemyLayer < 0)
        {
            Debug.LogError($"[VampirismInstaller] Layer '{EnemyLayerName}' does not exist in TagManager.");
            return;
        }

        VampirismDamager damager = EnsureDamager(player, enemyLayer);
        Vampirism vampirism = EnsureVampirism(player);
        SpriteRenderer zone = CreateZone(player, damager);
        VampirismView view = CreateHudBar(zone);

        if (view == null)
        {
            return;
        }

        SerializedPropertyUtility.SetObjectReference(vampirism, "_view", view);

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"[VampirismInstaller] Vampirism installed into {ScenePath}.");
    }

    private static VampirismDamager EnsureDamager(Player player, int enemyLayer)
    {
        VampirismDamager damager = player.GetComponent<VampirismDamager>();

        if (damager == null)
        {
            damager = player.gameObject.AddComponent<VampirismDamager>();
        }

        SerializedPropertyUtility.SetLayerMask(damager, "_targetLayer", enemyLayer);

        return damager;
    }

    private static Vampirism EnsureVampirism(Player player)
    {
        Vampirism vampirism = player.GetComponent<Vampirism>();

        if (vampirism == null)
        {
            vampirism = player.gameObject.AddComponent<Vampirism>();
        }

        return vampirism;
    }

    private static SpriteRenderer CreateZone(Player player, VampirismDamager damager)
    {
        Transform character = player.transform;
        Transform zoneTransform = character.Find(ZoneName);

        if (zoneTransform == null)
        {
            DefaultControls.Resources resources = EditorUiSceneUtility.CreateResources();
            GameObject zone = new GameObject(ZoneName, typeof(SpriteRenderer));
            zone.transform.SetParent(character, false);
            zone.transform.localPosition = new Vector3(0f, ZoneHeightOffset, 0f);

            SpriteRenderer renderer = zone.GetComponent<SpriteRenderer>();
            renderer.sprite = resources.knob;
            renderer.color = ZoneColor;
            renderer.sortingOrder = ZoneSortingOrder;
            zone.transform.localScale = Vector3.one * (damager.Radius * DiameterScale / GetSpriteWorldSize(renderer.sprite));

            zoneTransform = zone.transform;
        }

        return zoneTransform.GetComponent<SpriteRenderer>();
    }

    private static float GetSpriteWorldSize(Sprite sprite)
    {
        if (sprite == null)
        {
            return FallbackSpriteSize;
        }

        if (sprite.bounds.size.x <= 0f)
        {
            return FallbackSpriteSize;
        }

        return sprite.bounds.size.x;
    }

    private static VampirismView CreateHudBar(SpriteRenderer zone)
    {
        Canvas hud = FindHudCanvas();

        if (hud == null)
        {
            Debug.LogError($"[VampirismInstaller] HUD canvas not found in {ScenePath}.");
            return null;
        }

        Transform existingBar = hud.transform.Find(BarName);

        if (existingBar != null)
        {
            return existingBar.GetComponent<VampirismView>();
        }

        DefaultControls.Resources resources = EditorUiSceneUtility.CreateResources();
        Slider slider = EditorUiSceneUtility.CreateSlider(hud.transform, BarName, resources, BarFillColor, HealthDemoConstants.SliderBackgroundColor);
        GameObject barObject = slider.gameObject;

        SerializedPropertyUtility.DestroyChildIfExists(barObject.transform, EditorUiSceneUtility.SliderHandleAreaName);
        EditorUiSceneUtility.Anchor((RectTransform)barObject.transform, Vector2.zero, BarPosition, BarSize);

        VampirismView view = barObject.AddComponent<VampirismView>();
        SerializedPropertyUtility.SetObjectReference(view, "_slider", slider);
        SerializedPropertyUtility.SetObjectReference(view, "_zone", zone);

        return view;
    }

    private static Canvas FindHudCanvas()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);

        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay && canvas.transform.Find(CoinTextName) != null)
            {
                return canvas;
            }
        }

        return null;
    }
}
