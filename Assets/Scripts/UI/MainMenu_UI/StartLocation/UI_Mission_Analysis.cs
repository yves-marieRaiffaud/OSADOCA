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
    

    void Start()
    {
        if(initOrbitInst == null)
            initOrbitInst = sectionInitOrbit.GetComponent<UIStartLoc_InitOrbit>();

        maVectorLine = new VectorLine("missionAnalysis_FlatMapLine", new List<Vector2>(), 2f);
        maVectorLine.lineType = LineType.Points;
    }

    public void CloseMissionAnalysisSection()
    {
        sectionMissionAnalysis.SetActive(false);
        maVectorLine.active = false;
    }

    public void OpenMissionAnalysisSection()
    {
        sectionMissionAnalysis.SetActive(true);
        maVectorLine.active = true;

        GetPreviewedOrbit();

        maVectorLine.drawTransform = mapGO.transform;
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
        
        int nbValues = 720;
        double T = previewedOrbit.param.period; // in s
        double e = previewedOrbit.param.e;
        double a = previewedOrbit.param.a * 1_000d;
        double i = previewedOrbit.param.i * UniCsts.deg2rad;
        double nu = previewedOrbit.param.nu * UniCsts.deg2rad;
        double lAscN = previewedOrbit.param.lAscN * UniCsts.deg2rad;
        double omega = previewedOrbit.param.omega * UniCsts.deg2rad;

        double tIncr = T/nbValues; // in s
        List<Vector2> pos = new List<Vector2>();

        // Compute evolution of the longitude of ascending node
        double mu = orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.mu.ToString()].value;
        double J2 = orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.jnParams.j2.ToString()].value;
        double equaRad = orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()].value*1_000d;
        double revPeriod = orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.revolutionPeriod.ToString()].value;

        double h = Mathd.Sqrt(mu*UniCsts.µExponent*previewedOrbit.param.rp*1_000d*(1+e));
        
        double lAscNPoint = -(3/2 * Mathd.Sqrt(mu*UniCsts.µExponent)*J2*equaRad*equaRad / ((1d-e*e)*Mathd.Pow(a, 7d/2d))) * Mathd.Cos(i); // in rad/s
        // Compute evolution of the argument of the perigee
        double omegaPoint = lAscNPoint * (5/2 * Mathd.Pow(Mathd.Sin(i), 2) - 2d) / Mathd.Cos(i); // in rad/s

        double t0 = Kepler.OrbitalParamsConversion.nu2t(nu, e, T);
        Debug.Log("t0 = " + t0);


        // Taking into account Earth's rotation
        float omegaE = 2f*Mathf.PI*(1+1/(float)revPeriod)/86400f * (float)tIncr;
        Debug.Log("omegaE = " + (omegaE*UniCsts.rad2deg));
        float cOE = Mathf.Cos(omegaE);
        float sOE = Mathf.Sin(omegaE);
        Matrix4x4 earthRotMat = new Matrix4x4(new Vector4(cOE,-sOE,0), new Vector4(sOE,cOE,0), new Vector4(0,0,1), Vector4.zero);


        for(double t=0d; t<2f*T; t+=tIncr)
        {
            (double, double, double) nuEM = Kepler.OrbitalParamsConversion.t2nuEM(t0 + t, T, e);
            double currNu = nuEM.Item1; // in rad

            double updatedLAscN = lAscN + lAscNPoint*tIncr;
            double updatedOmega = omega + omegaPoint*tIncr;

            Vector3 Rx = (float) (h*h/(mu*UniCsts.µExponent)*1/(1+e*Mathd.Cos(currNu))) * new Vector3((float)Mathd.Cos(currNu), (float)Mathd.Sin(currNu), 0);
            
            float cO = Mathf.Cos((float)updatedOmega);
            float sO = Mathf.Sin((float)updatedOmega);
            float cI = Mathf.Cos((float)i);
            float sI = Mathf.Sin((float)i);
            float cL = Mathf.Cos((float)updatedLAscN);
            float sL = Mathf.Sin((float)updatedLAscN);

            float a0 = cO;
            float b0 = sO;
            float c0 = -sO;
            float d0 = cO;

            float alpha = cI;
            float beta = sI;
            float gamma = -sI;
            float delta = cI;

            float w = cL;
            float x = sL;
            float y = -sL;
            float z = cL;

            Vector4 c1 = new Vector4(a0 + y*b0*alpha, c0 + y*d0*alpha, y*gamma);
            Vector4 c2 = new Vector4(x*a0 + z*b0*alpha, x*c0 + z*d0*alpha, z*gamma);
            Vector4 c3 = new Vector4(b0*beta, d0*beta, delta);
            Matrix4x4 res = new Matrix4x4(c1, c2, c3, Vector4.zero);
            res = Matrix4x4.Transpose(res);

            Rx = res.MultiplyVector(Rx);


            
            Rx = earthRotMat.MultiplyVector(Rx);



            float magn = Rx.magnitude;
            float latitude = Mathf.Asin(Rx.z/magn);
            float l = Rx.x/magn;
            float tmpLong = Mathf.Acos(l/Mathf.Cos(latitude));
            
            float m = Rx.y/magn;
            float longitude;
            if(m > 0)
                longitude = tmpLong;
            else
                longitude = 2f*Mathf.PI - tmpLong;

            Vector2 xyCoord = LaunchPad.RawLatLong_2_XY(new Vector2(1160, 560), latitude*UniCsts.rad2deg, longitude*UniCsts.rad2deg);
            // Positioning the XY Coords with respect to the bottom left corner of the mzp, not the middle point
            xyCoord = xyCoord - new Vector2(580, 280);

            pos.Add(xyCoord);
        }
        maVectorLine.points2 = pos;
        maVectorLine.Draw();
    }
}