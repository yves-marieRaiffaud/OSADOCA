using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(UniverseRunner),true)]
public class UniverseRunnerEditor : Editor
{
    UniverseRunner universe;
    SerializedObject simEnvSerializedObjs;
    string simSettingsFilePath;

    bool simEnvFoldoutBool;

    private void OnEnable()
    {
        universe = (UniverseRunner)target;
        simSettingsFilePath = Application.persistentDataPath + Filepaths.simulation_settings;
        CheckCreateSimEnvSettingsFile();
        simEnvSerializedObjs = new SerializedObject(serializedObject.FindProperty("simEnv").objectReferenceValue);

        simEnvFoldoutBool = universe.simulationEnvFoldout;
    }

    private void CheckCreateSimEnvSettingsFile()
    {
        if(!File.Exists(simSettingsFilePath))
        {
            Debug.Log("Creating a new instance of SimulationEnv at path: '" + simSettingsFilePath + "'");
            SimulationEnv newInstance = ScriptableObject.CreateInstance<SimulationEnv>();
            universe.simEnv = newInstance;
        }
        else {
            JsonUtility.FromJsonOverwrite(File.ReadAllText(simSettingsFilePath), universe.simEnv);
        }
    }

    public void OnInspectorUpdate()
    {
        this.Repaint();
    }

    public override void OnInspectorGUI()
    {
        simEnvSerializedObjs.Update();
        //==================================
        if (!EditorGUIUtility.wideMode)
        {
            EditorGUIUtility.wideMode = true;
        }
        using(var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
        }
        
        DrawUniverseRunnerSettingsEditor(universe.simEnv);
        //==================================
        simEnvSerializedObjs.ApplyModifiedProperties();
    }

    private void DrawUniverseRunnerSettingsEditor(SimulationEnv settings)
    {
        if(settings != null)
        {
            simEnvFoldoutBool = EditorGUILayout.InspectorTitlebar(simEnvFoldoutBool, settings);
            using(var check = new EditorGUI.ChangeCheckScope())
            {   
                if(simEnvFoldoutBool)
                {
                    CreateUniverseRunnerSettingsEditor(settings);
                }
            }
        } 
    }

    private void CreateUniverseRunnerSettingsEditor(SimulationEnv param)
    {
        param.simulateGravity.value = EditorGUILayout.Toggle("Simulate gravity", param.simulateGravity.value);
        EditorGUILayout.Separator();

        if(!param.simulateGravity.value)
        {
            EditorGUI.BeginDisabledGroup(true);
            param.useNBodySimulation.value = false;
            param.useNBodySimulation.value = EditorGUILayout.Toggle("Use N-Body Simulation", param.useNBodySimulation.value);
            param.NBODYSIM_NB_BODY.value = EditorGUILayout.IntSlider("NBody sim NB Body", param.NBODYSIM_NB_BODY.value, 1, 5);
            EditorGUI.EndDisabledGroup();
        }
        else {
            param.useNBodySimulation.value = EditorGUILayout.Toggle("Use N-Body Simulation", param.useNBodySimulation.value);
            EditorGUI.BeginDisabledGroup(!param.useNBodySimulation.value);
            param.NBODYSIM_NB_BODY.value = EditorGUILayout.IntSlider("NBody sim NB Body", param.NBODYSIM_NB_BODY.value, 1, 5);
            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("FPS Targetting", EditorStyles.boldLabel);
        param.useTargetFrameRate.value = EditorGUILayout.Toggle("Enable FPS targetting", param.useTargetFrameRate.value);
        EditorGUI.BeginDisabledGroup(!param.useTargetFrameRate.value);
        param.targetFrameRate.value = EditorGUILayout.IntField("Target FPS", param.targetFrameRate.value);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.Separator();

        param.physicsUpdateRate.value = EditorGUILayout.IntSlider("Physics update rate", param.physicsUpdateRate.value, 1, 100);
        param.timeScale.value = EditorGUILayout.FloatField("Time scale", param.timeScale.value);
        
        if(GUI.changed)
        {
            string filepath = UsefulFunctions.WriteToFileSimuSettingsSaveData(param);
            Debug.Log("SimulationSettingsSaveData successfully saved at: '" + simSettingsFilePath + "'.");
        }
    }
}