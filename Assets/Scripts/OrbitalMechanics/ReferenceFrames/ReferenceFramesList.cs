using UnityEngine;
using Mathd_Lib;
using System;

public class ReferenceFramesList
{
    ReferenceFrame _ECEF;
    public ReferenceFrame ECEF
    {
        get {
            return _ECEF;
        }
    }


    public ReferenceFramesList()
    {
        GameObject earthGO = GameObject.Find("Earth");
        // 'Earth-Centered-Earth-Fixed' frame == repère terrestre vrai : 'https://en.wikipedia.org/wiki/ECEF'
        _ECEF = ReferenceFrame.New_ReferenceFrame(Vector3.zero, Vector3.right, Vector3.up, ReferenceFrame.SpecifiedVectors.XZ, earthGO, "ECEF");
    }
}