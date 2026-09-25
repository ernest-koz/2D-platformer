using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class HealthDemoSceneValidator
{
    public static void Validate(string scenePath)
    {
        GameObject root = GameObject.Find("HealthDemo");
        ThrowIfNull(root, "HealthDemo root not found");

        Health health = root.GetComponent<Health>();
        ThrowIfNull(health, "Health component not found");
        AssertEqual(SerializedPropertyUtility.GetInteger(health, "_maximum"), HealthDemoConstants.MaximumHealth, "Health._maximum");

        Canvas canvas = root.GetComponentInChildren<Canvas>();
        ThrowIfNull(canvas, "Canvas not found");
        ThrowIfNull(UnityEngine.Object.FindObjectOfType<EventSystem>(), "EventSystem not found");
        ThrowIfNull(UnityEngine.Object.FindObjectOfType<Camera>(), "Main Camera not found");

        ValidateHealthText(canvas, health);
        ValidateBar(canvas, "InstantHealthBar", health, false);
        ValidateBar(canvas, "SmoothHealthBar", health, true);
        ValidateButton(canvas, "DamageButton", typeof(DamageButton), health, HealthDemoConstants.SimulatedDamage);
        ValidateButton(canvas, "HealButton", typeof(HealButton), health, HealthDemoConstants.SimulatedHeal);

        Debug.Log($"[HealthDemoSceneValidator] {scenePath} is valid: health, three indicators and two buttons are wired.");
    }

    private static void ValidateHealthText(Canvas canvas, Health health)
    {
        Transform host = canvas.transform.Find("Panel/HealthText");
        ThrowIfNull(host, "HealthText label not found");

        TextMeshProUGUI label = host.GetComponent<TextMeshProUGUI>();
        ThrowIfNull(label, "HealthText has no TextMeshProUGUI");

        HealthText view = host.GetComponent<HealthText>();
        ThrowIfNull(view, "HealthText view not found");
        AssertReference(SerializedPropertyUtility.GetReference(view, "_health"), health, "HealthText._health");
        AssertReference(SerializedPropertyUtility.GetReference(view, "_text"), label, "HealthText._text");
    }

    private static void ValidateBar(Canvas canvas, string name, Health health, bool isSmooth)
    {
        Transform host = canvas.transform.Find($"Panel/{name}");
        ThrowIfNull(host, $"{name} slider not found");

        Slider slider = host.GetComponent<Slider>();
        ThrowIfNull(slider, $"{name} has no Slider");
        AssertEqual(slider.minValue, 0f, $"{name}.minValue");
        AssertEqual(slider.maxValue, 1f, $"{name}.maxValue");
        AssertEqual(slider.value, 1f, $"{name}.value");
        AssertEqual(slider.interactable, false, $"{name} must not be interactable");
        AssertEqual(slider.transition, Selectable.Transition.None, $"{name} must use no transition");

        HealthBar view = host.GetComponent<HealthBar>();
        ThrowIfNull(view, $"{name} view not found");

        if (isSmooth)
        {
            ThrowIfNull(host.GetComponent<SmoothHealthBar>(), $"{name} must use SmoothHealthBar");
            AssertEqual(SerializedPropertyUtility.GetFloat(view, "_fillDuration"), HealthDemoConstants.SmoothFillDuration, $"{name}._fillDuration");
        }
        else
        {
            ThrowIfPlainBarUsesSmooth(view, name);
        }

        AssertReference(SerializedPropertyUtility.GetReference(view, "_health"), health, $"{name}._health");
        AssertReference(SerializedPropertyUtility.GetReference(view, "_slider"), slider, $"{name}._slider");
    }

    private static void ValidateButton(Canvas canvas, string name, Type actionType, Health health, int amount)
    {
        Transform host = canvas.transform.Find($"Panel/{name}");
        ThrowIfNull(host, $"{name} not found");

        Button button = host.GetComponent<Button>();
        ThrowIfNull(button, $"{name} has no Button");

        Image image = host.GetComponent<Image>();
        ThrowIfNull(image, $"{name} has no Image");
        ThrowIfNull(image.sprite, $"{name} has no sprite");

        HoverCursor hoverCursor = host.GetComponent<HoverCursor>();
        ThrowIfNull(hoverCursor, $"{name} has no HoverCursor");
        ThrowIfNull(SerializedPropertyUtility.GetReference(hoverCursor, "_handCursor"), $"{name} has no hand cursor");

        HealthChangerButton action = host.GetComponent(actionType) as HealthChangerButton;
        ThrowIfNull(action, $"{name} has no {actionType.Name}");

        AssertEqual(button.onClick.GetPersistentEventCount(), 0, $"{name}.onClick persistent calls");
        AssertReference(SerializedPropertyUtility.GetReference(action, "_health"), health, $"{actionType.Name}._health");
        AssertEqual(SerializedPropertyUtility.GetInteger(action, "_amount"), amount, $"{actionType.Name}._amount");
    }

    private static void ThrowIfPlainBarUsesSmooth(HealthBar view, string name)
    {
        SmoothHealthBar smoothOnPlainBar = view.GetComponent<SmoothHealthBar>();

        if (smoothOnPlainBar == null)
        {
            return;
        }

        throw new InvalidOperationException($"[HealthDemoSceneValidator] {name} must use plain HealthBar.");
    }

    private static void ThrowIfNull(object value, string message)
    {
        if (value == null)
        {
            throw new InvalidOperationException($"[HealthDemoSceneValidator] {message}.");
        }
    }

    private static void ThrowIfNull(UnityEngine.Object value, string message)
    {
        if (value == null)
        {
            throw new InvalidOperationException($"[HealthDemoSceneValidator] {message}.");
        }
    }

    private static void AssertReference(object actual, object expected, string field)
    {
        if (ReferenceEquals(actual, expected) == false)
        {
            throw new InvalidOperationException($"[HealthDemoSceneValidator] {field} is not wired.");
        }
    }

    private static void AssertEqual(object actual, object expected, string field)
    {
        if (Equals(actual, expected) == false)
        {
            throw new InvalidOperationException($"[HealthDemoSceneValidator] {field} is {actual}, expected {expected}.");
        }
    }
}
