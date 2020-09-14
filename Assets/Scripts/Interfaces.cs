using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using UnityEngine.Events;

public interface FlyingObjCommonParams
{
    // Either 'Spaceship' or 'CelestialBody'
    // Interface for the shared variables between 'Spaceship' and 'CelestialBody' objects
    Orbit orbit {get; set;}
    OrbitalPredictor predictor {get; set;}
    OrbitalParams orbitalParams {get; set;}
    //================
    double distanceScaleFactor {get; set;} // Either 'UniCsts.m2km2au2u' if the orbit is defined in AU, or 'UniCsts.m2km2pl2u' if the orbit is defined in km
    Vector3d orbitedBodyRelativeAcc {get; set;}
    Vector3d orbitedBodyRelativeVelIncr {get; set;}
    Vector3d orbitedBodyRelativeVel {get; set;}
    Vector3d realPosition {get; set;}
    //================
    // Ordered List (descending order, from the strongest grav pull force to the weakest) of the gravitational forces 
    CelestialBodyPullForce[] gravPullList {get; set;}
    //================
    double SetDistanceScaleFactor(); // To set the Scaling factor to convert meters to unity units, depending if the orbit is defined in 'km' or in 'AU' 
    Vector3d GetRelativeRealWorldPosition();
    //================
    GameObject _gameObject {get; set;}
}

public interface FlyingObjSettings
{
    // Either 'SpaceshipSettings' or 'CelestialBodySettings'
}

public interface SimSettingInterface<T1>
{
    T1 value {get; set;}
    T1 default_value {get; set;}
    string type {get; set;}
    string displayName {get; set;}
    SimSettingCategory category {get; set;}
    SimSettings_Info simSettings_Info {get; set;}
}

public interface PropulsionInterface
{
    Rigidbody effectiveRB {get; set;}
    Vector3 worldPosition {get;}
    Vector3 localPosition {get;}

    ThrustAxes thrustAxes {get;}
    float RCSThrustPower {get; set;}

    ThrustIsOnEvent thrustIsOn {get; set;}

    bool displayThrustVector {get; set;}
    bool displayThrustAxes {get; set;}

    void DrawThrustVector();
}

[System.Serializable]
public class ThrustIsOnEvent : UnityEvent<PropulsionInterface>{}

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