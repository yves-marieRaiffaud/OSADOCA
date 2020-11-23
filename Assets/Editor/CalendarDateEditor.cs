using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(CalendarDate))]
public class CalendarDateEditor : PropertyDrawer
{
    const int ControlHeight = 16; // this is the height of each control line
    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        return base.GetPropertyHeight(prop, label) + ControlHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //if(!Application.isPlaying)
        //{
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginProperty(position, label, property);
            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            // Calculate rects
            var calendarStringRect = new Rect(position.x, position.y, 210, 20);
            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(calendarStringRect, property.FindPropertyRelative("dateString_unityEditor"), GUIContent.none);

            var formatStringRect = new Rect(position.x-140, position.y+25, 300, 20);
            EditorGUI.PropertyField(formatStringRect, property.FindPropertyRelative("dateFormat_unityEditor"), new GUIContent("Date Format"));

            // Set indent back to what it was
            EditorGUI.indentLevel = indent; 
            EditorGUI.EndChangeCheck();
            EditorGUI.EndProperty();
        //}
        /*else
        {
            // Application is playing
            EditorGUI.BeginProperty(position, label, property);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            // Calculate rects
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            var calendarStringRect = new Rect(position.x, position.y, 210, 20);
            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.TextField(calendarStringRect, "Calendar Date", property.FindPropertyRelative("dateString_unityEditor").stringValue);

            var formatStringRect = new Rect(position.x-140, position.y+25, 300, 20);
            EditorGUI.TextField(calendarStringRect, "Date Format", property.FindPropertyRelative("dateFormat_unityEditor").stringValue);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent; 
            EditorGUI.EndProperty();
        }*/
    }
}