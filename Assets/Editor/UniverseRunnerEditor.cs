using UnityEngine;
using UnityEditor;
using Mathd_Lib;

[CustomEditor(typeof(UniverseRunner),true)]
public class UniverseRunnerEditor : Editor
{
    UniverseRunner universe;

    private void onEnable()
    {
        GetSimulationEnv();
    }

    private void GetSimulationEnv()
    {
        universe = GameObject.Find("UniverseRunner").GetComponent<UniverseRunner>();
        if(universe.simEnv == null) {
            universe.simEnv = (SimulationEnv) SimulationEnv.CreateInstance("SimulationEnv");
        }
    }

    public void OnInspectorUpdate()
    {
        this.Repaint();
    }

    public override void OnInspectorGUI()
    {
        GetSimulationEnv();
        if (!EditorGUIUtility.wideMode)
        {
            EditorGUIUtility.wideMode = true;
        }
        using(var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
        }
        DrawUniverseRunnerSettingsEditor(universe.simEnv, ref universe.simEnv.simulationEnvFoldout);
    }

    private void DrawUniverseRunnerSettingsEditor(SimulationEnv settings, ref bool foldout)
    {
        if(settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            using(var check = new EditorGUI.ChangeCheckScope())
            {   
                if(foldout)
                {
                    CreateUniverseRunnerSettingsEditor(settings);
                }
            }
        } 
    }

    private void CreateUniverseRunnerSettingsEditor(SimulationEnv param)
    {
        param.simulateGravity = EditorGUILayout.Toggle("Simulate gravity", param.simulateGravity);
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Physics parameters", EditorStyles.boldLabel);
        param.useTargetFrameRate = EditorGUILayout.Toggle("Enable FPS targetting", param.useTargetFrameRate);
        EditorGUI.BeginDisabledGroup(param.useTargetFrameRate);
        param.targetFrameRate = EditorGUILayout.IntField("Target FPS", param.targetFrameRate);
        EditorGUI.EndDisabledGroup();
        param.physicsUpdateRate = EditorGUILayout.IntSlider("Physics update rate", param.physicsUpdateRate, 1, 100);
        param.timeScale = EditorGUILayout.IntField("Time scale", param.timeScale);
    }
}