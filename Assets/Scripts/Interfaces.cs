using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using UnityEngine.Events;

public interface UnitInterface
{
    Vector3d val3d {get;}
    double val {get;}
    string stringVal {get;}
}

public interface Dynamic_Obj_Common
{
    GameObject _gameObject {get;}
    Rigidbody _rigidbody {get;}

    OrbitalParams orbitalParams {get; set;}
    Orbit orbit {get; set;}

    // Each of the hereunder properties are defined with the Universe Scaling System
    /// <summary>
    /// Acceleration of the body relative to its parent, defined with the UniverseScalingSystem
    /// </summary>
    Vector3d relativeAcc {get; set;} // Orbited body relative acceleration, m/s2
    /// <summary>
    /// Velocity of the body relative to its parent, defined with the UniverseScalingSystem
    /// </summary>
    Vector3d relativeVel {get; set;} // Orbited body relative velocity, m/s
    /// <summary>
    /// Position of the body in the offset universe, defined with the UniverseScalingSystem
    /// </summary>
    Vector3d realPosition {get; set;} // Position with respect to the center of the universe
}

public interface Dynamic_Obj_Settings
{
    
}

// Interface handling the Simulation settings classes
public interface SimSettingInterface<T1>
{
    T1 value {get; set;}
    T1 default_value {get; set;}
    string type {get; set;}
    string displayName {get; set;}
    SimSettingCategory category {get; set;}
    SimSettings_Info simSettings_Info {get; set;}
}

[System.Serializable]
// Sends 2 int when one panel of startLocation (either InitPlanetary or inOrbit panel) fire this event.
// =======FIRST INT IS THE PANEL IDENTIFIER:
// The 'inOrbitInitialization' panel will send '0' as its identifier
// The 'planetaryInitialization' panel will send '1' as its identifier
// ======SECOND INT IS THE BOOLEAN INDICATING IF THE PANEL IS SET-UP (0: not set-up; 1: set-up)
public class StartLocPanelIsSetUp : UnityEvent<int, int>{}

[System.Serializable]
// Sends 2 int when one panel of the mainMenu ('startLocation', 'Spacecraft', 'Matlab' or 'simSettings') fire this event.
// =======FIRST INT IS THE PANEL IDENTIFIER:
// The 'startLocation' panel will send '0' as its identifier
// The 'spacecraft' panel will send '1' as its identifier
// The 'matlab' panel will send '2' as its identifier
// The 'simSettings' panel will send '3' as its identifier
// ======SECOND INT IS THE BOOLEAN INDICATING IF THE PANEL IS SET-UP (0: not set-up; 1: set-up)
public class MainPanelIsSetUp : UnityEvent<int, int>{}

// Event sending a bool of the value of the parameter 'use keyboard to control the spaceship'
public class UseKeyboardSimSettingEvent : UnityEvent<bool>{}
// Panel sending its channelIdx when it is deleted from the UI Main Menu Group Layout
public class ComPanelIsRemoved : UnityEvent<int>{}