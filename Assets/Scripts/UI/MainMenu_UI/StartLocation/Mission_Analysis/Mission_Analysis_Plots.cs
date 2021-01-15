using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mathd_Lib;
using ObjHand = CommonMethods.ObjectsHandling;
using UI_Fncs = CommonMethods.UI_Methods;
using UniCsts = UniverseConstants;
using UnityEngine.Events;

public class Mission_Analysis_Plots
{
    UIStartLoc_InitOrbit _initOrbitScript;
    UI_MissionAnalysis_Panel _missionAnalysisScript;

    public Mission_Analysis_Plots(UI_MissionAnalysis_Panel __missionAnalysisScript, UIStartLoc_InitOrbit __initOrbitScript)
    {
        _initOrbitScript = __initOrbitScript;
        _missionAnalysisScript = __missionAnalysisScript;
    }

    public void Create_GroundTracks_Data()
    {
        // Loop across delta_Theta = omega + true anomaly
        float nuIncr = (float)(1f*UniCsts.deg2rad);
        float aop = (float) _initOrbitScript.previewedOrbit.param.omega;
        float i = (float) _initOrbitScript.previewedOrbit.param.i;
        float lAscN = (float) _initOrbitScript.previewedOrbit.param.lAscN;
        float cI = Mathf.Cos(i);
        float sI = Mathf.Sin(i);

        float deltaLat = 0f;
        float deltaLong = 0f;
        float deltaTheta; // true anomaly

        List<Vector2> latLong = new List<Vector2>();

        // Starting at the ascending node ==> deltaLat = lat = 0°
        for(float nu=0f; nu<2f*Mathf.PI; nu+=nuIncr)
        {
            deltaTheta = nu + aop;
            deltaLat = Mathf.Asin(sI*Mathf.Sin(deltaTheta));
            deltaLong = Mathf.Asin(Mathf.Sin(deltaTheta)*cI/Mathf.Cos(deltaLat));

            Debug.LogFormat("Lat = {0}; Long = {1}", deltaLat*UniCsts.rad2deg, deltaLong*UniCsts.rad2deg);

            latLong.Add(LaunchPad.RawLatLong_2_XY(new Vector2(940f, 470f), deltaLat*UniCsts.rad2deg, deltaLong*UniCsts.rad2deg));
            Debug.Log(latLong[latLong.Count-1]);
        }

        _missionAnalysisScript._lr.Points = latLong.ToArray();
    }
}