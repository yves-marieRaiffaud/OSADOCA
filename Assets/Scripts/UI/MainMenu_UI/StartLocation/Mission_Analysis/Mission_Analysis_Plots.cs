using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mathd_Lib;
using MathOps = CommonMethods.MathsOps;
using ObjHand = CommonMethods.ObjectsHandling;
using UI_Fncs = CommonMethods.UI_Methods;
using UniCsts = UniverseConstants;
using UnityEngine.Events;
using System.Linq;

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
        if(_initOrbitScript.previewedOrbit == null || _initOrbitScript.previewedOrbit.param == null)
            return;

        float nuIncr = (float)(1f*UniCsts.deg2rad);
        float aop = (float) _initOrbitScript.previewedOrbit.param.omega;
        float e = (float) _initOrbitScript.previewedOrbit.param.e;
        float T = (float) _initOrbitScript.previewedOrbit.param.period;
        float i = (float) _initOrbitScript.previewedOrbit.param.i;
        float lAscN = (float) _initOrbitScript.previewedOrbit.param.lAscN;
        float cI = Mathf.Cos(i);
        float sI = Mathf.Sin(i);

        double orbitedBodyPeriod = _initOrbitScript.previewedOrbit.orbitedBody.settings.planetaryParams.siderealRotPeriod.val;
        float earthEquatorSpeed = (float) (2d*Mathd.PI/orbitedBodyPeriod); // rad/s

        float deltaLat = 0f;
        float deltaLong = 0f;

        List<Vector3> latLongTime = new List<Vector3>();

        float lAscN_M = (float) Kepler.OrbitalConversions.OrbitPosition.nu_2_M(-aop, e).Item1;
        // Time since periapsis to reach the ascending node
        float t_lAscN = lAscN_M/(float)_initOrbitScript.previewedOrbit.param.n;
        Debug.Log("lAscN_M = " + lAscN_M + "; t_lAscN = " + t_lAscN);

        for(float deltaTheta=0f; deltaTheta<2f*Mathf.PI; deltaTheta+=nuIncr)
        {
            deltaLat = Mathf.Asin(sI*Mathf.Sin(deltaTheta));
            deltaLong = Mathf.Asin(Mathf.Sin(deltaTheta)*cI/Mathf.Cos(deltaLat));
            if(MathOps.isInRange(deltaTheta, Mathf.PI/2f, 3f*Mathf.PI/2f, MathOps.isInRangeFlags.both_excluded))
                deltaLong = Mathf.PI - deltaLong;

            // Mean anomaly of the current satellite pos with respect to the apside line/perigee
            float nuToPeriapsis_M = (float) Kepler.OrbitalConversions.OrbitPosition.nu_2_M(deltaTheta - aop, e).Item1;
            nuToPeriapsis_M -= lAscN_M;
            nuToPeriapsis_M = ClampAngle(nuToPeriapsis_M, -Mathf.PI, Mathf.PI);

            float t = nuToPeriapsis_M/(float)_initOrbitScript.previewedOrbit.param.n; // time since the ascending node, in s
            if(t+0.00001f < Mathf.Epsilon)
                t = T - Mathf.Abs(t);

            deltaLong += lAscN; // Computing the longitude with respect to the 0° longitude, not with respect to the ascending node longitude
            deltaLong = deltaLong - earthEquatorSpeed*t;

            Vector2 latLong_xy = LaunchPad.RawLatLong_2_XY(new Vector2(913.4f, 443.4f), deltaLat*UniCsts.rad2deg, deltaLong*UniCsts.rad2deg);
            latLongTime.Add(new Vector3(latLong_xy.x, latLong_xy.y, t));
        }
        _missionAnalysisScript._lr.gameObject.SetActive(true);

        latLongTime = latLongTime.OrderBy(v => v.z).ToList<Vector3>(); // Sorting by time
        // Nedd to offset by x pos so that their is no weird line
        _missionAnalysisScript._lr.Points = latLongTime.ConvertAll<Vector2>(Retrieve_XY_From_LatLongTime_List).ToArray();
    }

    Vector2 Retrieve_XY_From_LatLongTime_List(Vector3 _inVec)
    {
        return new Vector2(_inVec.x, _inVec.y);
    }

    public float ClampAngle(float value, float lowerBound, float upperBound, float incr=2f*Mathf.PI)
    {
        while(value - upperBound > Mathf.Epsilon)
            value -= incr;
        while(value - lowerBound < Mathf.Epsilon)
            value += incr;
        return value;
    }
}