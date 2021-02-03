using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(Resources_Manifest_Helper),true)]
public class Resources_Manifest_Helper_Editor : Editor
{
    Resources_Manifest_Helper targetScript;

    void OnEnable()
    {
        targetScript = (Resources_Manifest_Helper)target;
    }

    public override void OnInspectorGUI()
    {
        if (!EditorGUIUtility.wideMode)
            EditorGUIUtility.wideMode = true;

        using(var check = new EditorGUI.ChangeCheckScope()) {
            base.OnInspectorGUI();
        }

        if(GUILayout.Button("Generate Manifest File"))
        {
            targetScript.On_Generate_File_Click();
        }
    }
}