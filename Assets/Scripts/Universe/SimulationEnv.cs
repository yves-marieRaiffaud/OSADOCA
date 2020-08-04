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

