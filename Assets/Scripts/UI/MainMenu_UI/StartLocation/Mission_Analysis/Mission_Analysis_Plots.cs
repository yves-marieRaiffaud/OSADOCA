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
using Vectrosity;

public class Mission_Analysis_Plots
{
    UIStartLoc_InitOrbit _initOrbitScript;
    UI_MissionAnalysis_Panel _missionAnalysisScript;
    Vector2 planetMapSize; // x = width; y = height

    public Mission_Analysis_Plots(UI_MissionAnalysis_Panel __missionAnalysisScript, UIStartLoc_InitOrbit __initOrbitScript)
    {
        _initOrbitScript = __initOrbitScript;
        _missionAnalysisScript = __missionAnalysisScript;

        float planetMapWidth = _missionAnalysisScript.planetMap.rectTransform.rect.width;
        float planetMapHeight = _missionAnalysisScript.planetMap.rectTransform.rect.height;
        planetMapSize = new Vector2(planetMapWidth, planetMapHeight);
    }

    public List<Vector2> Create_GroundTracks_Data(bool plotRotatingPlanet_GT)
    {
        // if 'plotRotatingPlanet_GT' == true: plotting the rotating planet ground tracks
        // if 'plotRotatingPlanet_GT' == false: plotting the non-rotating ground tracks
        if(_initOrbitScript.previewedOrbit == null || _initOrbitScript.previewedOrbit.param == null)
            Debug.LogError("Error while plotting ground track. '_initOrbitScript.previewedOrbit' or '_initOrbitScript.previewedOrbit.param' is null");

        float nuIncr = (float)(0.5f*UniCsts.deg2rad);
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
        int perigeeIdxStart = (int) Mathf.Ceil(MathOps.ClampAngle(aop, 0f, 2f*Mathf.PI)/nuIncr) - 1;
        //int perigeeIdxStart = (int) Mathf.Ceil(3f*Mathf.PI/(2f*nuIncr)) - 1;
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

            Vector2 latLong_xy = LaunchPad.RawLatLong_2_XY(planetMapSize, deltaLat*UniCsts.rad2deg, deltaLong*UniCsts.rad2deg);
            latLongTime.Add(new Vector3(latLong_xy.x-13.33f, latLong_xy.y, t));
        }

        // Sorting List by time since ascending node ==> output list is from the ascending node to the next ascending node one orbit afterwards
        latLongTime = latLongTime.OrderBy(v => v.z).ToList<Vector3>();
        
        // Patching from the apogee to the ascending node to the beginning of the List, so that it becomes from the perigee to the ascending node
        if(perigeeIdxStart != 0 && perigeeIdxStart != latLongTime.Count-1) {
            int nbItems = latLongTime.Count - perigeeIdxStart;
            float ascendingNode_Pt_XVal = latLongTime[0].x;
            float lastMovedPt_XVal = latLongTime[latLongTime.Count-1].x;

            List<Vector3> range2Move = latLongTime.GetRange(perigeeIdxStart, nbItems);
            latLongTime.RemoveRange(perigeeIdxStart, nbItems);
            latLongTime.InsertRange(0, range2Move);
            // Offseting the moved values so that they are patched correctly
            Vector3 loopVectorVal;
            for(int rngIdx = 0; rngIdx<nbItems; rngIdx++)
            {
                loopVectorVal = latLongTime[rngIdx];
                loopVectorVal.x += ascendingNode_Pt_XVal - lastMovedPt_XVal;
                if(loopVectorVal.x > planetMapSize.x)
                    loopVectorVal.x -= planetMapSize.x;
                latLongTime[rngIdx] = loopVectorVal;
            }
        }

        return  latLongTime.ConvertAll<Vector2>(Retrieve_XY_From_LatLongTime_List);
    }
    Vector2 Retrieve_XY_From_LatLongTime_List(Vector3 _latLongTimeVec3)
    {
        // From the vector3 of the 'latLongTime' List<Vector3>, returning only the X & Y position of the corresponding longitude & latitude
        // Not returning the time that was only used to sort the List
        return new Vector2(_latLongTimeVec3.x, _latLongTimeVec3.y);
    }

    internal List<Vector2> OffsetOrbit_fromLastOrbit_Point(Vector2 prevOrbitLastPt_XY, List<Vector2> currOrbitXY)
    {
        Vector2 firstPt = currOrbitXY[0];
        Vector2 loopVectorVal;
        for(int idx=0; idx<currOrbitXY.Count; idx++) {
            loopVectorVal = currOrbitXY[idx];
            loopVectorVal += prevOrbitLastPt_XY - firstPt;
            if(loopVectorVal.x > planetMapSize.x)
                loopVectorVal.x -= planetMapSize.x;
            currOrbitXY[idx] = loopVectorVal;
        }
        return currOrbitXY;
    }
}