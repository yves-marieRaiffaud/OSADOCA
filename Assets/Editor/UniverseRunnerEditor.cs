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

        simEnvFoldoutBool = simEnvSerializedObjs.FindProperty("simulationEnvFoldout").boolValue;
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
            universe.simEnv = SimulationEnvSaveData.LoadObjectFromJSON(simSettingsFilePath);
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
        param.simulateGravity = EditorGUILayout.Toggle("Simulate gravity", param.simulateGravity);
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("FPS Targetting", EditorStyles.boldLabel);
        param.useTargetFrameRate = EditorGUILayout.Toggle("Enable FPS targetting", param.useTargetFrameRate);
        EditorGUI.BeginDisabledGroup(param.useTargetFrameRate);
        param.targetFrameRate = EditorGUILayout.IntField("Target FPS", param.targetFrameRate);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.Separator();

        param.physicsUpdateRate = EditorGUILayout.IntSlider("Physics update rate", param.physicsUpdateRate, 1, 100);
        param.timeScale = EditorGUILayout.IntField("Time scale", param.timeScale);

        EditorGUILayout.Separator();

        param.NBODYSIM_NB_BODY = EditorGUILayout.IntSlider("NBody sim NB Body", param.NBODYSIM_NB_BODY, 1, 5);

        if(GUI.changed)
        {
            SimulationEnvSaveData dataToWrite = GatherSimSettingsDataToWriteToFile();
            string filepath = UsefulFunctions.WriteToFileSimulationSettingsSaveData(dataToWrite);
            Debug.Log("SimulationSettingsSaveData successfully saved at: '" + simSettingsFilePath + "'.");
        }
    }

    private SimulationEnvSaveData GatherSimSettingsDataToWriteToFile()
    {
        /*
        public bool simulateGravity=false;

        public bool useTargetFrameRate=false;
        public int targetFrameRate = 60; //fps
        public int physicsUpdateRate = 50; //Hz, default value at 50 Hz
        public int timeScale = 1;

        // Number of celestialBodies to compute the gravitational acc for each celestialBody
        public int NBODYSIM_NB_BODY;
        */
        SimulationEnv env = universe.simEnv;

        string[] simSettinsToSave = new string[SimulationEnvSaveData.NB_PARAMS];
        simSettinsToSave[0] = env.simulateGravity ? "1" : "0";
        simSettinsToSave[1] = env.useTargetFrameRate ? "1" : "0";
        simSettinsToSave[2] = env.targetFrameRate.ToString();
        simSettinsToSave[3] = env.physicsUpdateRate.ToString();
        simSettinsToSave[4] = env.timeScale.ToString();
        simSettinsToSave[5] = env.NBODYSIM_NB_BODY.ToString();

        return new SimulationEnvSaveData(simSettinsToSave);
    }
}