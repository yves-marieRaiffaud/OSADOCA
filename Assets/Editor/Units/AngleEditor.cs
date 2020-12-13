using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Angle))]
public class AngleEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var valueRect = new Rect(position.x, position.y, position.width/2f-2.5f, position.height);
        var unitRect = new Rect(position.x + position.width/2f, position.y, position.width/2f-2.5f, position.height);

        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("_val"), GUIContent.none);
        EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("_unit"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}