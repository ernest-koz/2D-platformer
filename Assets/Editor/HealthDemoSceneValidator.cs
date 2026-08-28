using System;
using System.Reflection;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class HealthDemoSceneValidator
{
    private const int ExpectedMaximum = 100;
    private const int ExpectedDamage = 10;
    private const int ExpectedHeal = 10;

    public static void Validate(string scenePath)
    {
        GameObject root = GameObject.Find("HealthDemo");
        ThrowIfNull(root, "HealthDemo root not found");

        Health health = root.GetComponent<Health>();
        ThrowIfNull(health, "Health component not found");
        AssertEqual(GetPrivateInt(health, "_maximum"), ExpectedMaximum, "Health._maximum");
        AssertEqual(GetPrivateFloat(health, "_invincibilityTime"), 0f, "Health._invincibilityTime");

        HealthSimulator simulator = root.GetComponent<HealthSimulator>();
        ThrowIfNull(simulator, "HealthSimulator component not found");
        AssertReference(GetPrivateObject(simulator, "_health"), health, "HealthSimulator._health");
        AssertEqual(GetPrivateInt(simulator, "_damageAmount"), ExpectedDamage, "HealthSimulator._damageAmount");
        AssertEqual(GetPrivateInt(simulator, "_healAmount"), ExpectedHeal, "HealthSimulator._healAmount");

        Canvas canvas = root.GetComponentInChildren<Canvas>();
        ThrowIfNull(canvas, "Canvas not found");
        ThrowIfNull(UnityEngine.Object.FindObjectOfType<EventSystem>(), "EventSystem not found");

        ValidateHealthText(canvas, health);
        ValidateBar(canvas, "InstantHealthBar", health, false);
        ValidateBar(canvas, "SmoothHealthBar", health, true);

        ValidateButton(canvas, "DamageButton", simulator, nameof(HealthSimulator.TakeDamage));
        ValidateButton(canvas, "HealButton", simulator, nameof(HealthSimulator.Heal));

        Debug.Log($"[HealthDemoSceneValidator] {scenePath} is valid: health, three indicators and two buttons are wired.");
    }

    private static void ValidateHealthText(Canvas canvas, Health health)
    {
        Transform host = canvas.transform.Find("HealthText");
        ThrowIfNull(host, "HealthText label not found");

        TextMeshProUGUI label = host.GetComponent<TextMeshProUGUI>();
        ThrowIfNull(label, "HealthText has no TextMeshProUGUI");

        HealthText view = host.GetComponent<HealthText>();
        ThrowIfNull(view, "HealthText view not found");
        AssertReference(GetPrivateObject(view, "_health"), health, "HealthText._health");
        AssertReference(GetPrivateObject(view, "_text"), label, "HealthText._text");
    }

    private static void ValidateBar(Canvas canvas, string name, Health health, bool smooth)
    {
        Transform host = canvas.transform.Find(name);
        ThrowIfNull(host, $"{name} slider not found");

        Slider slider = host.GetComponent<Slider>();
        ThrowIfNull(slider, $"{name} has no Slider");
        AssertEqual((int)slider.maxValue, ExpectedMaximum, $"{name}.maxValue");
        AssertEqual((int)slider.value, ExpectedMaximum, $"{name}.value");

        HealthBar view = host.GetComponent<HealthBar>();
        ThrowIfNull(view, $"{name} view not found");

        if (smooth)
        {
            ThrowIfNull(host.GetComponent<SmoothHealthBar>(), $"{name} must use SmoothHealthBar");
            AssertEqual(GetPrivateFloat(view, "_fillSpeed"), 0.5f, $"{name}._fillSpeed");
        }
        else
        {
            SmoothHealthBar smoothOnPlainBar = view.GetComponent<SmoothHealthBar>();

            if (smoothOnPlainBar != null)
            {
                throw new InvalidOperationException($"[HealthDemoSceneValidator] {name} must use plain HealthBar.");
            }
        }

        AssertReference(GetPrivateObject(view, "_health"), health, $"{name}._health");
        AssertReference(GetPrivateObject(view, "_slider"), slider, $"{name}._slider");
    }

    private static void ValidateButton(Canvas canvas, string name, HealthSimulator simulator, string methodName)
    {
        Transform host = canvas.transform.Find(name);
        ThrowIfNull(host, $"{name} not found");

        Button button = host.GetComponent<Button>();
        ThrowIfNull(button, $"{name} has no Button");

        AssertEqual(button.onClick.GetPersistentEventCount(), 1, $"{name}.onClick persistent calls");
        AssertReference(button.onClick.GetPersistentTarget(0), simulator, $"{name}.onClick target");
        AssertEqual(button.onClick.GetPersistentMethodName(0), methodName, $"{name}.onClick method");
    }

    private static void ThrowIfNull(object value, string message)
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

    private static object GetPrivateObject(object target, string fieldName)
    {
        return GetField(target, fieldName).GetValue(target);
    }

    private static int GetPrivateInt(object target, string fieldName)
    {
        return (int)GetField(target, fieldName).GetValue(target);
    }

    private static float GetPrivateFloat(object target, string fieldName)
    {
        return (float)GetField(target, fieldName).GetValue(target);
    }

    private static FieldInfo GetField(object target, string fieldName)
    {
        for (Type type = target.GetType(); type != null; type = type.BaseType)
        {
            FieldInfo field = type.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            if (field != null)
            {
                return field;
            }
        }

        throw new InvalidOperationException($"[HealthDemoSceneValidator] Field {fieldName} not found on {target.GetType().Name}.");
    }
}
