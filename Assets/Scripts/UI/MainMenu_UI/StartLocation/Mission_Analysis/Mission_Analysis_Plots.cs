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
    float planetMapWidth;
    float planetMapHeight;

    public Mission_Analysis_Plots(UI_MissionAnalysis_Panel __missionAnalysisScript, UIStartLoc_InitOrbit __initOrbitScript)
    {
        _initOrbitScript = __initOrbitScript;
        _missionAnalysisScript = __missionAnalysisScript;
        planetMapWidth = _missionAnalysisScript.planetMap.rectTransform.rect.width;
        planetMapHeight = _missionAnalysisScript.planetMap.rectTransform.rect.height;
    }

    public Vector2[] Create_GroundTracks_Data(bool plotRotatingPlanet_GT)
    {
        // if 'plotRotatingPlanet_GT' == true: plotting the rotating planet ground tracks
        // if 'plotRotatingPlanet_GT' == false: plotting the non-rotating ground tracks
        if(_initOrbitScript.previewedOrbit == null || _initOrbitScript.previewedOrbit.param == null)
            Debug.LogError("Error while plotting ground track. '_initOrbitScript.previewedOrbit' or '_initOrbitScript.previewedOrbit.param' is null");

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

        for(float deltaTheta=0f; deltaTheta<2f*Mathf.PI; deltaTheta+=nuIncr)
        {
            deltaLat = Mathf.Asin(sI*Mathf.Sin(deltaTheta));
            deltaLong = Mathf.Asin(Mathf.Sin(deltaTheta)*cI/Mathf.Cos(deltaLat));
            if(MathOps.isInRange(deltaTheta, Mathf.PI/2f, 3f*Mathf.PI/2f, MathOps.isInRangeFlags.both_excluded))
                deltaLong = Mathf.PI - deltaLong;

            // Mean anomaly of the current satellite pos with respect to the ascending node
            float nuToPeriapsis_M = (float) Kepler.OrbitalConversions.OrbitPosition.nu_2_M(deltaTheta - aop, e).Item1 - lAscN_M;
            nuToPeriapsis_M = MathOps.ClampAngle(nuToPeriapsis_M, -Mathf.PI, Mathf.PI);
            // Time since the ascending node, in s
            float t = nuToPeriapsis_M/(float)_initOrbitScript.previewedOrbit.param.n;
            if(t+0.00001f < Mathf.Epsilon)
                t = T - Mathf.Abs(t);

            deltaLong += lAscN; // Computing the longitude with respect to the 0° longitude, not with respect to the ascending node longitude
            if(plotRotatingPlanet_GT)
                deltaLong -= earthEquatorSpeed*t;

            Vector2 latLong_xy = LaunchPad.RawLatLong_2_XY(new Vector2(planetMapWidth, planetMapHeight), deltaLat*UniCsts.rad2deg, deltaLong*UniCsts.rad2deg);
            latLongTime.Add(new Vector3(latLong_xy.x, latLong_xy.y, t));
        }

        // Sorting List by time since ascending node ==> output list is from the ascending node to the next ascending node one orbit afterwards
        latLongTime = latLongTime.OrderBy(v => v.z).ToList<Vector3>();
        return latLongTime.ConvertAll<Vector2>(Retrieve_XY_From_LatLongTime_List).ToArray();
    }
    Vector2 Retrieve_XY_From_LatLongTime_List(Vector3 _latLongTimeVec3)
    {
        // From the vector3 of the 'latLongTime' List<Vector3>, returning only the X & Y position of the corresponding longitude & latitude
        // Not returning the time that was only used to sort the List
        return new Vector2(_latLongTimeVec3.x, _latLongTimeVec3.y);
    }

    internal Vector2[] OffsetOrbit_fromLastOrbit_Point(Vector2 prevOrbitLastPt_XY, Vector2[] currOrbitXY)
    {
        Vector2 firstPt = currOrbitXY[0];
        for(int idx=0; idx<currOrbitXY.Length; idx++) {
            currOrbitXY[idx] += prevOrbitLastPt_XY - firstPt;
            if(currOrbitXY[idx].x > planetMapWidth) {
                float extraWidth = currOrbitXY[idx].x - planetMapWidth;
                currOrbitXY[idx].x += - planetMapWidth + extraWidth;
            }
        }
        return currOrbitXY;
    }
}