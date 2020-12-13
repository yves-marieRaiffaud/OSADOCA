using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;

public interface UnitInterface{ }

public interface Dynamic_Obj_Common
{
    GameObject _gameObject {get;}
    Rigidbody _rigidbody {get;}

    Vector3d relativeAcc {get; set;} // Orbited body relative acceleration, m/s2
    Vector3d relativeVel {get; set;} // Orbited body relative velocity, m/s
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