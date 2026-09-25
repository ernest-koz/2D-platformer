using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class SerializedPropertyUtility
{
    public static void SetObjectReference(Object target, string propertyName, Object value)
    {
        SerializedProperty property = FindPropertyForWrite(target, propertyName);

        if (property == null)
        {
            return;
        }

        property.objectReferenceValue = value;
        property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetInteger(Object target, string propertyName, int value)
    {
        SerializedProperty property = FindPropertyForWrite(target, propertyName);

        if (property == null)
        {
            return;
        }

        property.intValue = value;
        property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetFloat(Object target, string propertyName, float value)
    {
        SerializedProperty property = FindPropertyForWrite(target, propertyName);

        if (property == null)
        {
            return;
        }

        property.floatValue = value;
        property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetLayerMask(Object target, string propertyName, int layerIndex)
    {
        if (layerIndex < 0)
        {
            Debug.LogError($"Layer index {layerIndex} is invalid, cannot set '{propertyName}'.", target);
            return;
        }

        SerializedProperty property = FindPropertyForWrite(target, propertyName);

        if (property == null)
        {
            return;
        }

        property.intValue = 1 << layerIndex;
        property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    public static int GetInteger(Object target, string propertyName)
    {
        return FindPropertyOrThrow(target, propertyName).intValue;
    }

    public static float GetFloat(Object target, string propertyName)
    {
        return FindPropertyOrThrow(target, propertyName).floatValue;
    }

    public static Object GetReference(Object target, string propertyName)
    {
        return FindPropertyOrThrow(target, propertyName).objectReferenceValue;
    }

    public static void DestroyChildIfExists(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);

        if (child == null)
        {
            return;
        }

        Object.DestroyImmediate(child.gameObject);
    }

    private static SerializedProperty FindPropertyForWrite(Object target, string propertyName)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogError($"Serialized property '{propertyName}' not found on {target.GetType().Name}.", target);
            return null;
        }

        return property;
    }

    private static SerializedProperty FindPropertyOrThrow(Object target, string propertyName)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);

        if (property == null)
        {
            throw new InvalidOperationException($"Serialized property '{propertyName}' not found on {target.GetType().Name}.");
        }

        return property;
    }
}
