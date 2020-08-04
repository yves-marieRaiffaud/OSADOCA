using UnityEngine;
using System;

public enum SimSettingCategory { UI, NBodyEngine, Physics };

[CreateAssetMenu(), Serializable]
public class SimulationEnv : ScriptableObject
{
    public static string[] simSettingCategoryLabels = new string[] {"UI", "NBody Simulator Engine", "Physics"};
    //========================
    [NonSerialized] public bool simulationEnvFoldout=true; // For custom editor, won't be saved as not included in the 'SimulationEnvSaveData' struct
    //=============================================
    //=============================================
    //=============================================
    // Class containing every variables needed to define the simulation environment
    public SimSettingBool simulateGravity = new SimSettingBool(true, true, "Simulate gravity", SimSettingCategory.NBodyEngine);
    // Number of celestialBodies to compute the gravitational acc for each celestialBody
    public SimSettingInt NBODYSIM_NB_BODY = new SimSettingInt(3, 3, "Number of body to consider for the N-Body sim computations", SimSettingCategory.NBodyEngine, 1, 10);
    //=============================================
    //=============================================
    //=============================================
    public SimSettingBool useTargetFrameRate = new SimSettingBool(false, false, "Enable FPS targetting", SimSettingCategory.Physics);
    public SimSettingInt targetFrameRate = new SimSettingInt(60, 60, "Target FPS value", SimSettingCategory.Physics, 1, 100);
    public SimSettingInt physicsUpdateRate = new SimSettingInt(50, 50, "Physics update frequency", SimSettingCategory.Physics, 1, 100);
    public SimSettingFloat timeScale = new SimSettingFloat(1f, 1f, "Time scale", SimSettingCategory.Physics, 0.1f, 10f);
    //=============================================
    //=============================================
    //=============================================
    //public UniverseClock masterClock;
    //=============================================
    //=============================================
    //=============================================
    //================================================
    public static bool ObjectIsSimSetting(object obj)
    {
        if(obj is SimSettingBool || obj is SimSettingInt || obj is SimSettingFloat) { return true; }
        else { return false; }
    }
}

public class SimSettingGeneric<T1>
{
    public T1 value;
    public T1 default_value;
    public string type; // only for writing in the JSON file
    public string displayName;
    public SimSettingCategory category;

    public SimSettingGeneric(T1 variableValue, T1 default_value, string nameToDisplay, SimSettingCategory settingCategory)
    {
        this.displayName = nameToDisplay;
        this.value = variableValue;
        this.category = settingCategory;
        this.type = variableValue.GetType().ToString();
    }
}

[Serializable]
public class SimSettingInt : SimSettingGeneric<int>
{
    public int minValue;
    public int maxValue;
    public SimSettingInt(int variableValue, int defaultValue, string nameToDisplay, SimSettingCategory settingCategory, int minVal, int maxVal) 
    : base(variableValue, defaultValue, nameToDisplay, settingCategory)
    {
        this.minValue = minVal;
        this.maxValue = maxVal;
    }
}

[Serializable]
public class SimSettingFloat : SimSettingGeneric<float>
{
    public float minValue;
    public float maxValue;
    public SimSettingFloat(float variableValue, float defaultValue, string nameToDisplay, SimSettingCategory settingCategory, float minVal, float maxVal) 
    : base(variableValue, defaultValue, nameToDisplay, settingCategory)
    {
        this.minValue = minVal;
        this.maxValue = maxVal;
    }
}

[Serializable]
public class SimSettingBool : SimSettingGeneric<bool>
{
    public SimSettingBool(bool variableValue, bool defaultValue, string nameToDisplay, SimSettingCategory settingCategory)
    : base(variableValue, defaultValue, nameToDisplay, settingCategory) { }
}

