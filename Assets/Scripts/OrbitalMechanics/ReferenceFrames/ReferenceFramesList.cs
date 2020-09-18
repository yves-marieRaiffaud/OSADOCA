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

    ReferenceFrame _ECI;
    public ReferenceFrame ECI
    {
        get {
            return _ECI;
        }
    }


    public ReferenceFramesList()
    {
        GameObject earthGO = GameObject.Find("Earth");
        // 'Earth-Centered-Earth-Fixed' frame == repère terrestre vrai : https://en.wikipedia.org/wiki/ECEF
        _ECEF = ReferenceFrame.New_ReferenceFrame(Vector3.zero, Vector3.right, Vector3.up, ReferenceFrame.SpecifiedVectors.XZ, earthGO, "ECEF");

        // 'Eart-centrered Inertial'/'Conventional Celestial Reference System' frame == repère celeste moyen : https://gssc.esa.int/navipedia/index.php/Conventional_Celestial_Reference_System
        Vector3 vernalDir = (Vector3) ((UniCsts.pv_j2000 - earthGO.transform.position).normalized);
        _ECI = ReferenceFrame.New_ReferenceFrame(Vector3.zero, vernalDir, Vector3.up, ReferenceFrame.SpecifiedVectors.XZ, earthGO, "ECI");

        ECEF.PrintFrame(true);
        ECI.PrintFrame(true);
    }
}