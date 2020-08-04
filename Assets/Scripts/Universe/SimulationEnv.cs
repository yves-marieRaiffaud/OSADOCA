using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[CreateAssetMenu(), Serializable]
public class SimulationEnv : ScriptableObject
{
    public bool simulationEnvFoldout=true; // For custom editor, won't be saved as not included in the 'SimulationEnvSaveData' struct

    // Class containing every variables needed to define the simulation environment
    public SimSettingBool simulateGravity = new SimSettingBool(true, "Simulate gravity");

    public SimSettingBool useTargetFrameRate = new SimSettingBool(false, "Enable FPS targetting");
    public SimSettingInt targetFrameRate = new SimSettingInt(60, "Target FPS value");
    public SimSettingInt physicsUpdateRate = new SimSettingInt(50, "Physics update frequency");
    public SimSettingInt timeScale = new SimSettingInt(1, "Time scale");

    // Number of celestialBodies to compute the gravitational acc for each celestialBody
    public SimSettingInt NBODYSIM_NB_BODY = new SimSettingInt(3, "Number of body to consider for the N-Body sim computations");

    //public UniverseClock masterClock;
}

/*[System.Serializable]
public struct SimulationEnvSaveData
{
    //=========================================
    public const int NB_PARAMS=6; // Won't be serialized and won't be saved. Used only to set the size of the array passed in the constructor
    //=========================================
    [SerializeField] private string simulateGravity; // Bool
    [SerializeField] private string useTargetFrameRate; // Bool
    [SerializeField] private string targetFrameRate; // int
    [SerializeField] private string physicsUpdateRate; // int
    [SerializeField] private string timeScale; // int
    [SerializeField] private string NBODYSIM_NB_BODY; // int

    public SimulationEnvSaveData(params string[] values)
    {
        if(values.Length != NB_PARAMS) {
            Debug.Log("The passed array to save the SimulationSettingsSaveData has an incorrect size. Size should be " + NB_PARAMS + ", but passed array has size " + values.Length);
        }
        this.simulateGravity        = values[0];
        this.useTargetFrameRate     = values[1];
        this.targetFrameRate        = values[2];
        this.physicsUpdateRate      = values[3];
        this.timeScale              = values[4];
        this.NBODYSIM_NB_BODY       = values[5];
    }

    //========================================================================
    public static SimulationEnv LoadObjectFromJSON(string filepath)
    {
        SimulationEnvSaveData loadedData = JsonUtility.FromJson<SimulationEnvSaveData>(File.ReadAllText(filepath));
        SimulationEnv output = (SimulationEnv)ScriptableObject.CreateInstance<SimulationEnv>();
        //==========
        output.simulateGravity = System.Convert.ToBoolean(int.Parse(loadedData.simulateGravity));

        output.useTargetFrameRate = System.Convert.ToBoolean(int.Parse(loadedData.useTargetFrameRate));
        output.targetFrameRate = int.Parse(loadedData.targetFrameRate);
        output.physicsUpdateRate = int.Parse(loadedData.physicsUpdateRate);
        output.timeScale = int.Parse(loadedData.timeScale);

        output.NBODYSIM_NB_BODY = int.Parse(loadedData.NBODYSIM_NB_BODY);
        //==========
        return output;
    }
}*/

public class SimSettingGeneric<T1>
{
    public T1 value;
    public string type; // only for writing in the JSON file
    public string displayName;

    public SimSettingGeneric(T1 variableValue, string nameToDisplay)
    {
        this.displayName = nameToDisplay;
        this.value = variableValue;
        this.type = variableValue.GetType().ToString();
    }
}

[Serializable]
public class SimSettingInt : SimSettingGeneric<int>
{
    public SimSettingInt(int variableValue, string nameToDisplay) : base(variableValue, nameToDisplay) { }
}

[Serializable]
public class SimSettingBool : SimSettingGeneric<bool>
{
    public SimSettingBool(bool variableValue, string nameToDisplay) : base(variableValue, nameToDisplay) { }
}

