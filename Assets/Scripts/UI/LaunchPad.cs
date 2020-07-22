using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Mathd_Lib;

public class LaunchPad
{
    public enum launchPadParams { refPlanet, name, longitude, latitude };

    private Vector2 lp_XYMapPos; // Position on the map of the planet of the launch pad (x & y coordinate)
    public Dictionary<launchPadParams, string> launchPadParamsDict;

    public LaunchPad(Dictionary<launchPadParams, string> ref_launchPadParamsDict)
    {
        launchPadParamsDict = ref_launchPadParamsDict;
        LatitudeLongitude_2_XY();
    }

    private void LatitudeLongitude_2_XY()
    {
        /*
        transform.position = Quaternion.AngleAxis(longitude, -Vector3.up) * Quaternion.AngleAxis(latitude, -Vector3.right) * new Vector3(0,0,1);
        */
        // and from the 3D world position, use the atan2 formlas to project on the XY map ?
    }
}
