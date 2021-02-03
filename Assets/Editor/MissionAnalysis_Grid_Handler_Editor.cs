using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(MissionAnalysis_Grid_Handler),true)]
public class MissionAnalysis_Grid_Handler_Editor : Editor
{
    MissionAnalysis_Grid_Handler targetScript;

    void OnEnable()
    {
        targetScript = (MissionAnalysis_Grid_Handler)target;
    }

    public override void OnInspectorGUI()
    {
        if (!EditorGUIUtility.wideMode)
            EditorGUIUtility.wideMode = true;

        using(var check = new EditorGUI.ChangeCheckScope()) {
            base.OnInspectorGUI();
        }

        if(GUILayout.Button("Update Grid Dropdown Options")) {
            targetScript.SetUp_Dropdown_Options();
            Debug.Log("Successfully Updated the Grid Dropdown Options.");
        }
    }
}