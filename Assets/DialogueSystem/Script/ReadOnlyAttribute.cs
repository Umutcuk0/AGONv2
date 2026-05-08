using System;
using UnityEditor;
using UnityEngine;

public class ReadOnlyAttribute : PropertyAttribute
{

    public bool playModeOnly { get; private set; }

    public ReadOnlyAttribute(bool playModeOnly = false)
    {
        this.playModeOnly = playModeOnly;
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = (ReadOnlyAttribute)attribute;

        bool shouldLock = attr.playModeOnly ? EditorApplication.isPlaying : true;

        bool prevEnabled = GUI.enabled;
        if (shouldLock) GUI.enabled = false;

        EditorGUI.PropertyField(position, property, label, true);

        GUI.enabled = prevEnabled;
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
#endif
