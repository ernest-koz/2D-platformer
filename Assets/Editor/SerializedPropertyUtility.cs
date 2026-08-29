using UnityEditor;
using UnityEngine;

public static class SerializedPropertyUtility
{
    public static void SetObjectReference(Object target, string propertyName, Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogError($"Serialized property '{propertyName}' not found on {target.GetType().Name}.", target);
            return;
        }

        property.objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetInteger(Object target, string propertyName, int value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogError($"Serialized property '{propertyName}' not found on {target.GetType().Name}.", target);
            return;
        }

        property.intValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetFloat(Object target, string propertyName, float value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogError($"Serialized property '{propertyName}' not found on {target.GetType().Name}.", target);
            return;
        }

        property.floatValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetLayerMask(Object target, string propertyName, int layerIndex)
    {
        if (layerIndex < 0)
        {
            Debug.LogError($"Layer index {layerIndex} is invalid, cannot set '{propertyName}'.", target);
            return;
        }

        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogError($"Serialized property '{propertyName}' not found on {target.GetType().Name}.", target);
            return;
        }

        property.intValue = 1 << layerIndex;
        serialized.ApplyModifiedPropertiesWithoutUndo();
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
}
