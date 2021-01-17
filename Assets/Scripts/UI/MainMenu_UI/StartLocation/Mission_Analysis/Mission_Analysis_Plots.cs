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

        List<Vector2> latLong = new List<Vector2>();
        List<Vector3> debugArr = new List<Vector3>();

        float lAscN_M = (float) Kepler.OrbitalConversions.OrbitPosition.nu_2_M(-aop, e).Item1;

        for(float deltaTheta=-Mathf.PI; deltaTheta<Mathf.PI; deltaTheta+=nuIncr)
        {
            deltaLat = Mathf.Asin(sI*Mathf.Sin(deltaTheta));
            deltaLong = -Mathf.PI - Mathf.Asin(Mathf.Sin(deltaTheta)*cI/Mathf.Cos(deltaLat));
            if(deltaTheta < -Mathf.PI/2f)
                deltaLong = -Mathf.PI - deltaLong;
            else if(deltaTheta > Mathf.PI/2f)
                deltaLong = Mathf.PI - deltaLong;

            /*float currM = (float) Kepler.OrbitalConversions.OrbitPosition.nu_2_M(deltaTheta-aop, e).Item1 + lAscN_M;
            currM = ClampAngle(currM, 0f, 2f*Mathf.PI);
            float t = currM/(float)_initOrbitScript.previewedOrbit.param.n;
            deltaLong = deltaLong - earthEquatorSpeed*t;*/

            //debugArr.Add(new Vector3((float)(deltaLat*UniCsts.rad2deg), (float)((deltaLong+lAscN-Mathf.PI/2f)*UniCsts.rad2deg), earthEquatorSpeed*t*(float)UniCsts.rad2deg));
            latLong.Add(LaunchPad.RawLatLong_2_XY(new Vector2(913.4f, 443.4f), deltaLat*UniCsts.rad2deg, (deltaLong+lAscN-Mathf.PI/2f)*UniCsts.rad2deg));
        }
        _missionAnalysisScript._lr.gameObject.SetActive(true);
        _missionAnalysisScript._lr.Points = latLong.OrderBy(v => v.x).ToArray<Vector2>();

        debugArr = debugArr.OrderBy(itm => itm.y).ToList<Vector3>();
        foreach(Vector3 itm in debugArr)
            Debug.LogFormat("lat = {0}; long = {1}; t = {2}", itm.x, itm.y, itm.z);
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