using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mathd_Lib;
using Vectrosity;
using Fncs = UsefulFunctions;

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
        // Creating array for positions, 2 values per degree
        int nbValuesPerObit = 720;
        double T = previewedOrbit.param.period; // in s
        double e = previewedOrbit.param.e;
        double a = previewedOrbit.param.a * 1_000d; // m
        double i = previewedOrbit.param.i * UniCsts.deg2rad; // rad
        double nu = previewedOrbit.param.nu * UniCsts.deg2rad; // rad
        double lAscN = previewedOrbit.param.lAscN * UniCsts.deg2rad; // rad
        double omega = previewedOrbit.param.omega * UniCsts.deg2rad; // rad
        double n = previewedOrbit.n; // rad/s

        double tIncr = T/nbValuesPerObit; // in s
        List<Vector2> pos = new List<Vector2>();
        List<Vector2> pos2 = new List<Vector2>();

        // Compute evolution of the longitude of ascending node
        double mu = orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.mu.ToString()].value;
        double J2 = orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.jnParams.j2.ToString()].value;
        double equaRad = orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()].value; // km
        double revPeriod = orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.revolutionPeriod.ToString()].value; // days
        double rotPeriod = orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()].value; // s

        Debug.LogFormat("mu = {0}, J2 = {1}, equaRad = {2}, revPeriod = {3}, rotPeriod = {4}", mu, J2, equaRad, revPeriod, rotPeriod);
        Debug.LogFormat("T = {0} s, e = {1}, a = {2} km, i = {3} °, nu = {4} °, lAscN = {5} °, omega = {6} °", T, e, a/1000d, i*UniCsts.rad2deg, nu*UniCsts.rad2deg, lAscN*UniCsts.rad2deg, omega*UniCsts.rad2deg);

        double h = Mathd.Sqrt(mu*UniCsts.µExponent*previewedOrbit.param.rp*1_000d*(1+e)) * Mathd.Pow(10,-3);
        float bodyAngularVel = (float)CelestialBody.Get_Body_Angular_Rotation_Velocity(revPeriod, rotPeriod);
        Debug.Log("h = " + h + " ; bodyAngularVel = " + (bodyAngularVel*UniCsts.rad2deg) + "°/s");

        double lAscNPoint = Kepler.Compute_lAscN_Derivative(n, J2*Mathd.Pow(10,-6), equaRad*1000d, e, a, i); // rad/s
        double omegaPoint = Kepler.Compute_Omega_Derivative(lAscNPoint, i); // rad/s

        Debug.Log("lAscNPoint = " + lAscNPoint + "°/s ; omegaPoint = " + omegaPoint + "°/s");

        double t0 = Kepler.OrbitalParamsConversion.nu2t(nu, e, T);
        Debug.Log("t0 = " + t0);
        double updatedLAscN = lAscN; // rad
        double updatedOmega = omega; // rad

        for(double t=t0; t<T; t+=tIncr)
        {
            Debug.Log("============================");
            Debug.Log("t = " + t);
            (double, double, double) nuEM = Kepler.OrbitalParamsConversion.t2nuEM(t, T, e);
            Debug.Log("nu = " + (nuEM.Item1*UniCsts.rad2deg) + " ; E = " + (nuEM.Item2*UniCsts.rad2deg) + " ; M = " + (nuEM.Item3*UniCsts.rad2deg));
            double currNu = nuEM.Item1; // in rad

            updatedLAscN += lAscNPoint*t;
            updatedOmega += omegaPoint*t;
            Debug.Log("updatedLAscN = " + (updatedLAscN*UniCsts.rad2deg) + " ; updatedOmega = " + (updatedOmega*UniCsts.rad2deg));

            Vector3d[] rv_perifocal = Kepler.Get_R_V_Perifocal_Frame(h, mu, e, currNu);
            Debug.Log("rv_perifocal = " + rv_perifocal[0] + "\n" + rv_perifocal[1]);
            Matrix4x4 perifocal2Geo_mat = Kepler.Get_Perifocal_2_Geocentric_Frame_Matrix(i, updatedOmega, updatedLAscN);
            Debug.Log("mat = " + perifocal2Geo_mat);
            Vector3 rGeocentric_noRot = perifocal2Geo_mat.MultiplyVector((Vector3) rv_perifocal[0]);
            Debug.Log("rx step c = " + (rGeocentric_noRot/1000f));

            float earthRotAngle = bodyAngularVel * (float)tIncr;
            Debug.Log("earthRotAngle = " + (earthRotAngle*UniCsts.rad2deg));
            float cOE = Mathf.Cos(earthRotAngle);
            float sOE = Mathf.Sin(earthRotAngle);
            Matrix4x4 earthRotMat = new Matrix4x4(new Vector4(cOE,-sOE,0), new Vector4(sOE,cOE,0), new Vector4(0,0,1), Vector4.zero);
            Vector3 rGeocentric = earthRotMat.MultiplyVector(rGeocentric_noRot);
            Debug.Log("rx step d = " + (rGeocentric/1000f));

            Vector2d latLong = Kepler.Geocentric_2_Latitude_Longitude(new Vector3d(rGeocentric)); // deg
            Vector2 xyCoord = LaunchPad.RawLatLong_2_XY(new Vector2(1160, 560), latLong.x, latLong.y);
            xyCoord = xyCoord - new Vector2(580, 280);
            Debug.Log("xycoord = " + xyCoord + "; latlOng = " + latLong);

            pos.Add(xyCoord);
        }
        maVectorLine.points2 = pos;
        maVectorLine.Draw();

        //maVectorLine2.points2 = pos2;
        //maVectorLine2.Draw();
    }
}