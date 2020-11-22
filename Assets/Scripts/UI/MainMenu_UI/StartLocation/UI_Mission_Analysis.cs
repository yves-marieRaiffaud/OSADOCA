using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mathd_Lib;
using Vectrosity;
using Fncs = UsefulFunctions;
using System.Linq;

public class UI_Mission_Analysis : MonoBehaviour
{
    public GameObject sectionMissionAnalysis;
    public GameObject sectionInitOrbit;
    public RectTransform mapGO;

    private UIStartLoc_InitOrbit initOrbitInst;
    private Orbit previewedOrbit;
    private CelestialBody orbitedBody;
    private VectorLine maVectorLine;

    private VectorLine maVectorLine2;
    

    void Start()
    {
        if(initOrbitInst == null)
            initOrbitInst = sectionInitOrbit.GetComponent<UIStartLoc_InitOrbit>();

        maVectorLine = new VectorLine("missionAnalysis_FlatMapLine", new List<Vector2>(), 2f);
        maVectorLine.lineType = LineType.Points;

        maVectorLine2 = new VectorLine("missionAnalysis_FlatMapLine2", new List<Vector2>(), 2f);
        maVectorLine2.lineType = LineType.Points;
    }

    public void CloseMissionAnalysisSection()
    {
        sectionMissionAnalysis.SetActive(false);
        maVectorLine.active = false;
        maVectorLine2.active = false;
    }

    public void OpenMissionAnalysisSection()
    {
        sectionMissionAnalysis.SetActive(true);
        maVectorLine.active = true;
        maVectorLine2.active = true;

        GetPreviewedOrbit();

        maVectorLine.drawTransform = mapGO.transform;
        maVectorLine2.drawTransform = mapGO.transform;
        GenerateGroundTracks();
    }

    private void GetPreviewedOrbit()
    {
        if(initOrbitInst.previewedOrbit != null)
            previewedOrbit = initOrbitInst.previewedOrbit;
        else
            previewedOrbit = null;

        if(initOrbitInst.orbitedBody != null)
            orbitedBody = initOrbitInst.orbitedBody;
        else
            orbitedBody = null;
    }

    private void GenerateGroundTracks()
    {
        // Retrieve orbital parameters
        int nbValuesPerObit = 720;
        double e = previewedOrbit.param.e;
        double a = previewedOrbit.param.a * 1_000d; // m
        double i = previewedOrbit.param.i * UniCsts.deg2rad; // rad
        double nu = previewedOrbit.param.nu * UniCsts.deg2rad; // rad
        double lAscN = previewedOrbit.param.lAscN * UniCsts.deg2rad; // rad
        double omega = previewedOrbit.param.omega * UniCsts.deg2rad; // rad
        double T = previewedOrbit.param.period; // in s
        double n = previewedOrbit.n; // rad/s

        double tIncr = 60; //T/nbValuesPerObit; // in s
        List<Vector2> pos = new List<Vector2>();
        //List<Vector2> pos2 = new List<Vector2>();

        // Retrieve parameters of the orbited body
        double mu = orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.mu.ToString()].value;
        double J2 = orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.jnParams.j2.ToString()].value;
        double equaRad = orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()].value; // km
        double revPeriod = orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.revolutionPeriod.ToString()].value; // days
        double rotPeriod = orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()].value; // s

        // Angular momentum in km^2/s
        double h = Mathd.Sqrt(mu*UniCsts.µExponent*previewedOrbit.param.rp*1_000d*(1+e)) * Mathd.Pow(10,-3);

        // Angular velocity of the orbited body in rad/s
        float bodyAngularVel = (float)CelestialBody.Get_Body_Angular_Rotation_Velocity(revPeriod, rotPeriod);
        // Rotation of the Earth in rad during tIncr
        float earthRotAngle = bodyAngularVel * (float)tIncr;
        float cOE = Mathf.Cos(earthRotAngle);
        float sOE = Mathf.Sin(earthRotAngle);
        // Rotation matrix to describe the rotation of the Earth during tIncr
        Matrix4x4 earthRotMat = new Matrix4x4(new Vector4(cOE,-sOE,0), new Vector4(sOE,cOE,0), new Vector4(0,0,1), Vector4.zero);

        // Rate of the longitude of the ascending node in rad/s 
        double lAscNPoint = Kepler.Compute_lAscN_Derivative(n, J2*Mathd.Pow(10,-6), equaRad*1000d, e, a, i);
        // Rate of the argument of the perigee in rad/s
        double omegaPoint = Kepler.Compute_Omega_Derivative(lAscNPoint, i);

        // Time on orbit before/after perigee (depending on the sign of t0), in s
        double t0 = Kepler.OrbitalParamsConversion.nu2t(nu, e, T);
        double updatedLAscN = lAscN; // updated lAscN in the loop, in rad
        double updatedOmega = omega; // updated omega in the loop, in rad

        Debug.LogFormat("mu = {0}, J2 = {1}, equaRad = {2}, revPeriod = {3}, rotPeriod = {4}", mu, J2, equaRad, revPeriod, rotPeriod);
        Debug.LogFormat("T = {0} s, e = {1}, a = {2} km, i = {3} °, nu = {4} °, lAscN = {5} °, omega = {6} °", T, e, a/1000d, i*UniCsts.rad2deg, nu*UniCsts.rad2deg, lAscN*UniCsts.rad2deg, omega*UniCsts.rad2deg);
        Debug.Log("h = " + h + " ; bodyAngularVel = " + (bodyAngularVel*UniCsts.rad2deg) + "°/s");
        Debug.Log("lAscNPoint = " + lAscNPoint + "°/s ; omegaPoint = " + omegaPoint + "°/s");
        Debug.Log("t0 = " + t0);
        Debug.Log("earthRotmat = \n" + earthRotMat);

        for(double t=t0; t<T; t+=tIncr)
        {
            // Computes the true anomaly, eccentric anomaly and mean anomaly, all in rad, for the current time on the orbit
            (double, double, double) nuEM = Kepler.OrbitalParamsConversion.t2nuEM(t, T, e);
            // Get current true anomaly in rad
            double currNu = nuEM.Item1;

            updatedLAscN += lAscNPoint*t;
            updatedOmega += omegaPoint*t;

            Vector3d[] rv_perifocal = Kepler.Get_R_V_Perifocal_Frame(h, mu, e, currNu);
            Matrix4x4 perifocal2Geo_mat = Kepler.Get_Perifocal_2_Geocentric_Frame_Matrix(i, updatedOmega, updatedLAscN);
            // Position vector in earth fixed (no rotation of the earth) frame, in meters
            Vector3 rGeocentric_noRot = perifocal2Geo_mat.MultiplyVector((Vector3) rv_perifocal[0]);
            // Position vector at t, taking into account the rotation of the Earth during tIncr, in meters
            Vector3 rGeocentric = earthRotMat.MultiplyVector(rGeocentric_noRot);
            // Converting the geocentric position in longitude and latitude (or azimuth & declination), all in degrees
            Vector2d latLong = Kepler.Geocentric_2_Latitude_Longitude(new Vector3d(rGeocentric));
            Vector2 xyCoord = LaunchPad.RawLatLong_2_XY(new Vector2(1160, 560), latLong.x, latLong.y);
            // Offset the coordinates from the center of the map to its bottom left corner for proper positionning
            xyCoord = xyCoord - new Vector2(580, 280);

            // Adding the point to the ground track to draw
            pos.Add(xyCoord);

            Debug.Log("============================");
            Debug.Log("t = " + t + "s before/after perigee");
            Debug.Log("nu = " + (nuEM.Item1*UniCsts.rad2deg) + "° ; E = " + (nuEM.Item2*UniCsts.rad2deg) + "° ; M = " + (nuEM.Item3*UniCsts.rad2deg) + "°");
            Debug.Log("updatedLAscN = " + (updatedLAscN*UniCsts.rad2deg) + "°/s ; updatedOmega = " + (updatedOmega*UniCsts.rad2deg) + "°/s");
            Debug.Log("rv_perifocal = " + rv_perifocal[0] + "m\n" + rv_perifocal[1] + "m/s");
            Debug.Log("perifocal2Geo_mat = \n" + perifocal2Geo_mat);
            Debug.Log("rGeocentric_noRot = " + (rGeocentric_noRot/1000f) + " km");
            Debug.Log("earthRotAngle = " + (earthRotAngle*UniCsts.rad2deg) + " °");
            Debug.Log("rGeocentric = " + (rGeocentric/1000f) + " km");
            Debug.Log("xycoord = " + xyCoord + "° ; latlOng = " + latLong + " °");
        }
        maVectorLine.points2 = pos;
        maVectorLine.Draw();

        //maVectorLine2.points2 = pos2;
        //maVectorLine2.Draw();
    }

    public void Generate_Visibility_Track()
    {

    }
}