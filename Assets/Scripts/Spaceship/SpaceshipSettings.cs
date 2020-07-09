using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;

[CreateAssetMenu(), System.Serializable]
public class SpaceshipSettings : ScriptableObject, FlyingObjSettings
{
    public bool foldoutSpaceshipSettings;
    public bool orbitalParamsEditorFoldout;
    public bool orbitInfoShowPredictedOrbitInfo;

    public double mass; // kg

    [SerializeField] public Material bodyMaterial;
    //==============
    public bool showInfoPanel=false;
}