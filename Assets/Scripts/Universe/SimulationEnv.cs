using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SimulationEnv : ScriptableObject
{
    public bool simulationEnvFoldout; // For custom editor

    // Class containing every variables needed to define the simulation environment
    public bool simulateGravity=false;

    public bool useTargetFrameRate=false;
    public int targetFrameRate = 60; //fps
    public int physicsUpdateRate = 50; //Hz, default value at 50 Hz
    public int timeScale = 1;

    //public UniverseClock masterClock;
}