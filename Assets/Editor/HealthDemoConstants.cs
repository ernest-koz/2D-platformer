using UnityEngine;

internal static class HealthDemoConstants
{
    public const int MaximumHealth = 100;
    public const int SimulatedDamage = 10;
    public const int SimulatedHeal = 10;
    public const float SmoothFillDuration = 1f;

    public const string DemoRootName = "HealthDemo";
    public const string PanelName = "Panel";
    public const string HealthTextName = "HealthText";
    public const string InstantBarName = "InstantHealthBar";
    public const string SmoothBarName = "SmoothHealthBar";
    public const string DamageButtonName = "DamageButton";
    public const string HealButtonName = "HealButton";
    public const string SliderBackgroundName = "Background";
    public const string SliderFillAreaName = "Fill Area";
    public const string SliderFillAreaFillName = "Fill Area/Fill";
    public const string SliderHandleAreaName = "Handle Slide Area";
    public const string KnobSpritePath = "UI/Skin/Knob.psd";
    public const string UiSpritePath = "UI/Skin/UISprite.psd";
    public const string EnemyLayerName = "Enemy";

    public static readonly Color SliderBackgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.9f);
}
