using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;

[CreateAssetMenu()]
public class SpaceshipSettings : ScriptableObject, FlyingObjSettings
{
    public bool foldoutSpaceshipSettings;
    public bool orbitalParamsEditorFoldout;
    public bool orbitInfoShowPredictedOrbitInfo;

    public double mass; // kg
    public bool startFromGround; // Boolean if the spaceship must spawn on the OrbitedBody surface (no orbit definition then)

    public Material bodyMaterial;
    //==============
    public bool showInfoPanel=false;
}
