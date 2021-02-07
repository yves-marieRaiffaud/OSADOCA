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

    public List<XYLatLongTime> Create_GroundTracks_Data(bool plotRotatingPlanet_GT)
    {
        // if 'plotRotatingPlanet_GT' == true: plotting the rotating planet ground tracks
        // if 'plotRotatingPlanet_GT' == false: plotting the non-rotating ground tracks
        if(_initOrbitScript.previewedOrbit == null || _initOrbitScript.previewedOrbit.param == null)
            return null;

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

        List<XYLatLongTime> latLongTime = new List<XYLatLongTime>();
        int perigeeIdxStart = (int) Mathf.Ceil(MathOps.ClampAngle(aop, 0f, 2f*Mathf.PI)/nuIncr) - 1;
        // Corresponding Mean Anomaly of the ascending node
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

            float lat = (float) (deltaLat*UniCsts.rad2deg);
            float longi = (float) (deltaLong*UniCsts.rad2deg);
            Vector2 latLong_xy = LaunchPad.RawLatLong_2_XY(planetMapSize, lat, longi);
            //latLong_xy.x -= -13.33f;
            latLongTime.Add(new XYLatLongTime(latLong_xy.x, latLong_xy.y, lat, longi, t));
        }

        // Sorting List by time since ascending node ==> output list is from the ascending node to the next ascending node one orbit afterwards
        latLongTime = latLongTime.OrderBy(v => v.time).ToList<XYLatLongTime>();
        
        // Patching from the apogee to the ascending node to the beginning of the List, so that it becomes from the perigee to the ascending node
        if(perigeeIdxStart != 0 && perigeeIdxStart != latLongTime.Count-1) {
            int nbItems = latLongTime.Count - perigeeIdxStart;
            float ascendingNode_Pt_XVal = latLongTime[0].x;
            float lastMovedPt_XVal = latLongTime[latLongTime.Count-1].x;

            List<XYLatLongTime> range2Move = latLongTime.GetRange(perigeeIdxStart, nbItems);
            latLongTime.RemoveRange(perigeeIdxStart, nbItems);
            latLongTime.InsertRange(0, range2Move);

            // Offseting the moved values so that they are patched correctly
            XYLatLongTime loopVectorVal;
            for(int rngIdx = 0; rngIdx<nbItems; rngIdx++)
            {
                loopVectorVal = latLongTime[rngIdx];
                loopVectorVal.x += ascendingNode_Pt_XVal - lastMovedPt_XVal;
                if(loopVectorVal.x > planetMapSize.x)
                    loopVectorVal.x -= planetMapSize.x;
                latLongTime[rngIdx] = loopVectorVal;
            }
        }

        return latLongTime;
    }
    

    internal List<XYLatLongTime> OffsetOrbit_fromLastOrbit_Point(XYLatLongTime prevOrbitLastPt_XY, List<XYLatLongTime> currOrbitXY)
    {
        XYLatLongTime firstPt_XYLatLong = currOrbitXY[0];
        XYLatLongTime loopVectorVal;
        for(int idx=0; idx<currOrbitXY.Count; idx++) {
            loopVectorVal = currOrbitXY[idx];
            loopVectorVal.Add_To_XYLatLong(XYLatLongTime.Sub_XYLatLongTime_Data(prevOrbitLastPt_XY, firstPt_XYLatLong).Get_XYLatLong());
            if(loopVectorVal.x > planetMapSize.x)
                loopVectorVal.x -= planetMapSize.x;
            currOrbitXY[idx] = loopVectorVal;
        }
        return currOrbitXY;
    }

    public struct XYLatLongTime
    {
        // Struct used while generating the Ground Track data.
        // x & y are the X and Y pixel position on the screen with respect to the bottom left corner of the planetMap
        // latitude and longitude are in degrees with respect to the Greenwhich meridian
        // time is the time since the ascending node, in seconds. time can be positive or negative (after/before the ascending node).
        public float x;
        public float y;
        public float latitude;
        public float longitude;
        public float time;
        public XYLatLongTime(float _x, float _y, float _latitude, float _longitude, float _time) {
            x = _x;
            y = _y;
            latitude = _latitude;
            longitude = _longitude;
            time = _time;
        }

        public XYLatLongTime Add_To_XYLatLong(Vector4 xyLatLong_ToAdd)
        {
            x += xyLatLong_ToAdd.x;
            y += xyLatLong_ToAdd.y;
            latitude += xyLatLong_ToAdd.z;
            longitude += xyLatLong_ToAdd.w;
            return this;
        }
        public Vector4 Get_XYLatLong()
        {
            return new Vector4(x, y, latitude, longitude);
        }

        /// <summary>
        /// Will return data1 + data2 element wise
        /// </summary>
        public static XYLatLongTime Add_XYLatLongTime_Data(XYLatLongTime data1, XYLatLongTime data2)
        {
            float _x = data1.x + data2.x;
            float _y = data1.y + data2.y;
            float _lat = data1.latitude + data2.latitude;
            float _longi = data1.longitude + data2.longitude;
            float _time = data1.time + data2.time;
            return new XYLatLongTime(_x, _y, _lat, _longi, _time);
        }
        /// <summary>
        /// Will return data1 - data2 element wise
        /// </summary>
        public static XYLatLongTime Sub_XYLatLongTime_Data(XYLatLongTime data1, XYLatLongTime data2)
        {
            float _x = data1.x - data2.x;
            float _y = data1.y - data2.y;
            float _lat = data1.latitude - data2.latitude;
            float _longi = data1.longitude - data2.longitude;
            float _time = data1.time - data2.time;
            return new XYLatLongTime(_x, _y, _lat, _longi, _time);
        }
        public static Vector4 Retrieve_XYLatLong_From_Struct(XYLatLongTime _XYlatLongTimeData)
        {
            return new Vector4(_XYlatLongTimeData.x, _XYlatLongTimeData.y, _XYlatLongTimeData.latitude, _XYlatLongTimeData.longitude);
        }
        public static Vector2 Retrieve_XY_From_Struct(XYLatLongTime _XYlatLongTimeData)
        {
            return new Vector2(_XYlatLongTimeData.x, _XYlatLongTimeData.y);
        }
        public static Vector2 Retrieve_LatLong_From_Struct(XYLatLongTime _XYlatLongTimeData)
        {
            return new Vector2(_XYlatLongTimeData.latitude, _XYlatLongTimeData.longitude);
        }
        public override string ToString()
        {
            return string.Format("X = {0}; Y = {1}; Lat = {2}; Long = {3}; t = {4}", x, y, latitude, longitude, time);
        }
    }
}