using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;

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
    Vector3d GetRelativeVelocity();
    double GetRelativeVelocityMagnitude();
    //================
    GameObject _gameObject {get; set;}
}

public interface FlyingObjSettings
{
    // Either 'SpaceshipSettings' or 'CelestialBodySettings'
}