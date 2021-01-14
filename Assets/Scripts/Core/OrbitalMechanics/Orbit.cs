using UnityEngine;
using Mathd_Lib;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
//using Vectrosity;

using Universe;
using MathOps = CommonMethods.MathsOps;
using ObjHand = CommonMethods.ObjectsHandling;
using UniCsts = UniverseConstants;
using OrbShape = Kepler.OrbitalConversions.OrbitShape;
using OrbTime = Kepler.OrbitalConversions.OrbitTime;
using OrbPos = Kepler.OrbitalConversions.OrbitPosition;

[Serializable]
public class Orbit
{
    [SerializeField] private const double CIRCULAR_ORBIT_TOLERANCE = 10e-5d; // Tolerance for the excentricity value with respect to zero

    public OrbitalParams param;

    List<Vector3> lineRendererPts;
    LineRenderer _lineRenderer;
    public LineRenderer lineRenderer {
        get {
            return _lineRenderer;
        }
    }

    public CelestialBody orbitedBody;
    public Dynamic_Obj_Common orbitingInterface;
    public Quaterniond orbitRot; // Rotation from XZ ellipse to XYZ ellipse by applying i, Omega, omega

    public Orbit(OrbitalParams orbitalParams, CelestialBody celestialBody, GameObject orbitingBody, LineRenderer __lineRenderer=null)
    {
        param = orbitalParams;
        orbitingInterface = orbitingBody.GetComponent<Dynamic_Obj_Common>();
        orbitedBody = celestialBody;

        switch(param.orbitDefType)
        {
            case OrbitalTypes.orbitDefinitionType.rarp:
                param.e = OrbShape.rarp_2_e(param.ra, param.rp);
                param.p = OrbShape.rarp_2_p(param.ra, param.rp);
                break;
            case OrbitalTypes.orbitDefinitionType.rpe:
                param.ra = OrbShape.erp_2_ra(param.e, param.rp);
                param.p = OrbShape.erp_2_p(param.e, param.rp);
                break;
            case OrbitalTypes.orbitDefinitionType.pe:
                param.rp = OrbShape.ep_2_rp(param.e, param.p);
                param.ra = OrbShape.erp_2_ra(param.e, param.rp);
                break;
        }

        param.a = OrbShape.rarp_2_a(param.ra, param.rp);
        param.b = OrbShape.pa_2_b(param.p, param.a);
        param.c = OrbShape.ea_2_c(param.e, param.a);

        double a_km = param.a;
        if(param.orbParamsUnits.Equals(OrbitalTypes.orbitalParamsUnits.AU))
            a_km *= Units.AU2KM;

        if(orbitedBody != null) {
            param.period = OrbTime.amu_2_T(a_km, orbitedBody.settings.planetaryParams.mu.val*UniCsts.µExponent);
            param.n = OrbTime.T_2_n(param.period);
        }
        else {
            param.period = param.n = double.NaN;
        }

        (double,double) nu_E;
        switch(param.bodyPosType)
        {
            case OrbitalTypes.bodyPositionType.nu:
                (double,double) M_E = OrbPos.nu_2_M(param.nu, param.e);
                param.M = M_E.Item1;
                param.E = M_E.Item2;
                param.L = OrbPos.lAscN_omegaM_2_L(param.lAscN, param.omega, M_E.Item1);
                param.t = OrbPos.MT_2_t(param.M, param.period);
                break;

            case OrbitalTypes.bodyPositionType.M:
                nu_E = OrbPos.M_2_nu(param.M, param.e);
                param.nu = nu_E.Item1;
                param.E = nu_E.Item2;
                param.L = OrbPos.lAscN_omegaM_2_L(param.lAscN, param.omega, param.M);
                param.t = OrbPos.MT_2_t(param.M, param.period);
                break;

            case OrbitalTypes.bodyPositionType.E:
                param.nu = OrbPos.E_2_nu(param.E, param.e);
                param.M = OrbPos.E_2_M(param.E, param.e);
                param.L = OrbPos.lAscN_omegaM_2_L(param.lAscN, param.omega, param.M);
                param.t = OrbPos.MT_2_t(param.M, param.period);
                break;

            case OrbitalTypes.bodyPositionType.L:
                param.M = OrbPos.L_2_M(param.L, param.lAscN, param.omega);
                nu_E = OrbPos.M_2_nu(param.M, param.e);
                param.nu = nu_E.Item1;
                param.E = nu_E.Item2;
                param.t = OrbPos.MT_2_t(param.M, param.period);
                break;

            case OrbitalTypes.bodyPositionType.t:
                (double,double,double) nu_E_M = OrbPos.t_2_nuEM(param.t, param.period, param.e);
                param.nu = nu_E_M.Item1;
                param.E = nu_E_M.Item2;
                param.M = nu_E_M.Item3;
                param.L = OrbPos.lAscN_omegaM_2_L(param.lAscN, param.omega, param.M);
                break;
        }

        param.vp = ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.vernalPoint);
        param.vpAxisRight = ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.vpAxisRight);
        param.vpAxisUp = ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.vpAxisUp);

        lineRendererPts = new List<Vector3>();
        if(__lineRenderer != null)
            _lineRenderer = __lineRenderer;
        else
            Init_LineRenderer();
        Draw_Orbit();
        //DebugOrbitalParams();
    }

    public Vector3d ComputeDirectionVector(OrbitalTypes.typeOfVectorDir vecType)
    {
        if(orbitedBody == null)
            return new Vector3d(double.NaN, double.NaN, double.NaN);
        else {
            switch(vecType)
            {
                case OrbitalTypes.typeOfVectorDir.vernalPoint:
                    return (UniCsts.pv_j2000 - orbitingInterface.realPosition).normalized;

                case OrbitalTypes.typeOfVectorDir.vpAxisRight:
                    if(!Vector3d.IsValid(param.vp))
                        ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.vernalPoint);
                    return new Vector3d(param.vp.y, param.vp.z, param.vp.x).normalized;

                case OrbitalTypes.typeOfVectorDir.vpAxisUp:
                    if(!Vector3d.IsValid(param.vp))
                        ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.vernalPoint);
                    return new Vector3d(param.vp.z, param.vp.x, param.vp.y).normalized;

                case OrbitalTypes.typeOfVectorDir.apogeeLine:
                    if(lineRenderer != null)
                        return new Vector3d(lineRendererPts[lineRendererPts.Count/2]).normalized;
                    return new Vector3d(double.NaN, double.NaN, double.NaN);

                case OrbitalTypes.typeOfVectorDir.ascendingNodeLine:
                    if(MathOps.DoublesAreEqual(param.i, 0d, 10e-4))
                    {
                        // There is an infinity of Ascending nodes
                        // Returning the ascending node line as the perpendicular vector with respect to the apogee line, and in the orbit plane
                        Vector3d tmp = ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.apogeeLine);
                        return Vector3d.Cross(tmp, param.orbitPlane.normal);
                    }
                    else {
                        Vector3d intersectPoint = new Vector3d();
                        Vector3d intersectLine = new Vector3d();
                        OrbitPlane.PlaneIntersectPlane(param.orbitPlane, orbitedBody.equatorialPlane, out intersectPoint, out intersectLine);
                        return intersectLine;
                    }

                case OrbitalTypes.typeOfVectorDir.radialVec:
                    return (orbitedBody.realPosition - orbitingInterface.realPosition).normalized;

                case OrbitalTypes.typeOfVectorDir.tangentialVec:
                    Vector3d radial = ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.radialVec);
                    return Vector3d.Cross(radial, param.orbitPlane.normal).normalized;

                case OrbitalTypes.typeOfVectorDir.velocityVec:
                    return orbitingInterface.relativeVel.normalized;

                case OrbitalTypes.typeOfVectorDir.accelerationVec:
                    return orbitingInterface.relativeAcc.normalized;
            }
        }
        return new Vector3d(double.NaN, double.NaN, double.NaN);
    }

    void Init_LineRenderer()
    {
        string lineName = "Orbit_" + orbitingInterface._gameObject.name;
        _lineRenderer = ObjHand.CreateAssignGameObject(lineName, typeof(LineRenderer)).GetComponent<LineRenderer>();
        _lineRenderer.gameObject.layer = Filepaths.engineLayersToInt[Filepaths.EngineLayersName.Orbit];
        _lineRenderer.gameObject.tag = UniverseRunner.goTags.Orbit.ToString();
        _lineRenderer.transform.parent = orbitedBody.orbitFolderGO.transform;
        _lineRenderer.transform.localPosition = Vector3.zero;
        _lineRenderer.transform.localScale = Vector3.one;

        _lineRenderer.useWorldSpace = false;
        _lineRenderer.loop = true;
        _lineRenderer.receiveShadows = false;
        _lineRenderer.generateLightingData = false;
        _lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        _lineRenderer.widthCurve = AnimationCurve.Constant(0f,1f,1f);
        _lineRenderer.sharedMaterial = Resources.Load<Material>(Filepaths.RSC_orbitMaterial);
    }

    void Draw_Orbit()
    {
        //==================================
        // 1|==> Compute Aphelion in plane of the equatorial plane of the orbited body, without any rotation
        //==================================
        // 2|==> Compute apsidesline 'apogeeLineDir' and 'ascendingNodeLineDir' which is perpendicular to to the apsides line
        // because without any 'argument of the periapsis' rotation applied, the two vectors are perpendicular.
        //       Compute inclination rotation 'iRot' to the ellipse
        //==================================
        // 3|==> Compute rotations: first the longitude of the ascending node, then the argument of the perihelion
        //==================================
        // 4|==> Compute normal vector of the rotated orbit plane, and compute rotation quaternions for the 'longitude of the ascending node'
        // and the argument of the perihelion 
        //==================================
        // 5|==> Updating directions are computed: first the new normal of the orbit plane,
        // then the new 'apogeeLineDir', finally the new 'ascendingNodeLineDir'
        // Finnaly, drawing the ellipse in its equatorial plane and rotating it using the following rotation order:
        // inclination, argument of the perihelion, longitude of the ascending node.
        // The equatorial plane has been rotated by the axial tilt angle of the orbited body
        //==================================
        // 1
        double incr = 2*Mathf.PI/param.orbitDrawingResolution;

        double tmp_p = param.p;
        if(param.orbParamsUnits.Equals(OrbitalTypes.orbitalParamsUnits.AU))
            tmp_p *= Units.AU2KM;

        double thetaAphelion = (int)(param.orbitDrawingResolution/2) * incr;
        double xAphelion = Mathd.Cos(thetaAphelion)*tmp_p/(1 + param.e*Mathd.Cos(thetaAphelion));
        double zAphelion = Mathd.Sin(thetaAphelion)*tmp_p/(1 + param.e*Mathd.Cos(thetaAphelion));
        Vector3d aphelionNoRot = new Vector3d(xAphelion, 0d, zAphelion);// * scalingFactor;

        // 2
        // Modifying the y value of the aphelion, so that the 'aphelionNoRot' remains in the (XZ) plane even if the orbited planet has a y value != 0 (because it is on its own orbit)
        aphelionNoRot.y = orbitedBody.transform.position.y;
        param.apogeeLineDir = (aphelionNoRot - orbitedBody.transform.position).normalized;
        param.ascendingNodeLineDir = Vector3d.Cross(param.apogeeLineDir, param.vpAxisUp).normalized;
        Quaterniond iRot = Quaterniond.AngleAxis(param.i*UniCsts.rad2deg, param.ascendingNodeLineDir).GetNormalized();

        param.apogeeLineDir = iRot * param.apogeeLineDir;

        // 3
        Vector3d orbitNormalUp = Vector3d.Cross(param.ascendingNodeLineDir, param.apogeeLineDir).normalized;
        Quaterniond perihelionArgRot = Quaterniond.AngleAxis(90d - param.omega*UniCsts.rad2deg, orbitNormalUp).GetNormalized();
        Quaterniond longAscendingNodeRot = Quaterniond.AngleAxis(-(param.lAscN*UniCsts.rad2deg + 90d), param.vpAxisUp).GetNormalized();
        Quaterniond rot = (longAscendingNodeRot*perihelionArgRot).GetNormalized();

        // 5
        // Taking into account the axial tilt of the planet
        // Will be used to rotate the equatorial plane/equatorial vectors
        Quaterniond equatorialAdjustment = Quaterniond.Identity;
        if(!orbitedBody.tag.Equals(UniverseRunner.goTags.Star.ToString()) && !orbitedBody.spawnAsUIPlanet)
        {
            //Vector3d tangentialVec = orbitedBody.orbit.ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.tangentialVec);
            Vector3d tangentialVec = orbitedBody.orbit.ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.tangentialVec);
            double orbitedBodyAxialTilt = orbitedBody.settings.planetaryParams.axialTilt.val;
            equatorialAdjustment = Quaterniond.AngleAxis(orbitedBodyAxialTilt, -tangentialVec).GetNormalized();
        }
        orbitedBody.equatorialPlane.forwardVec = equatorialAdjustment * orbitedBody.equatorialPlane.forwardVec;
        orbitedBody.equatorialPlane.rightVec = equatorialAdjustment * orbitedBody.equatorialPlane.rightVec;
        orbitedBody.equatorialPlane.normal = equatorialAdjustment * orbitedBody.equatorialPlane.normal;
        
        // Rotation Quaternion of the complete rotation (first inclination, then perihelion argument,
        // then longitude of the ascending node
        orbitRot = rot * iRot;

        // Depending if we are calling the Orbit class from the UI Main Menu or from the Simulation, we must divide every points of the orbit
        // by a factor: either the localScale of the orbited CelestialBody if we are in sim (sim planets have a local scale equal to their diameter in km)
        // , or the diameter of the orbited CelestialBody if we are in the UI Main Menu (UI planets have a diameter of 1).
        double pointScaler = 1d; // we are in sim
        if(orbitedBody.spawnAsUIPlanet)
            pointScaler = 2d*orbitedBody.settings.planetaryParams.radius.val/orbitedBody.transform.localScale.x; // we are in the UI Main Menu

        // Drawing the ellipse
        for(int i = 0; i < param.orbitDrawingResolution; i++)
        {
            double theta = i * incr;
            double x = Mathd.Cos(theta)*tmp_p/(1 + param.e*Mathd.Cos(theta));
            double z = Mathd.Sin(theta)*tmp_p/(1 + param.e*Mathd.Cos(theta));

            Vector3d noRotPoint = orbitedBody.equatorialPlane.forwardVec * x + orbitedBody.equatorialPlane.rightVec * z;
            if(orbitedBody.spawnAsUIPlanet)
                noRotPoint /= pointScaler; // only for orbits drawn in the UI Main Menu
            lineRendererPts.Add((Vector3)(orbitRot * (noRotPoint)));
        }
        lineRendererPts.Add(lineRendererPts[0]); // Adding at the end the first point to close the ellipse
        _lineRenderer.positionCount = param.orbitDrawingResolution;
        _lineRenderer.SetPositions(lineRendererPts.ToArray());

        // 4
        // normalUp vector has changed after the rotation by longAscendingNodeRot.
        orbitNormalUp = equatorialAdjustment * longAscendingNodeRot * orbitNormalUp;
        param.apogeeLineDir = ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.apogeeLine);
        Vector3d orbitPlaneRightVec = Vector3d.Cross(param.apogeeLineDir, orbitNormalUp);

        if(param.orbitPlane == null)
            param.orbitPlane = new OrbitPlane();
        param.orbitPlane.AssignOrbitPlane(param.apogeeLineDir, orbitPlaneRightVec, new Vector3d(lineRendererPts[(int)(lineRendererPts.Count/2)]));
        //Orbit plane.normal will point downward.
        if(!Vector3d.IsValidAndNoneZero(param.ascendingNodeLineDir))
            param.ascendingNodeLineDir = Vector3d.Cross(param.apogeeLineDir, orbitNormalUp).normalized; // Orbit is circular (coincident planes) or does not intersect equatorial plane
    }

    /// <summary>
    /// Returns the world position, in km, of the object relative to its orbitedBody
    /// </summary>
    public static Vector3d GetWorldPositionFromOrbit(Orbit orbit)
    {
        // Return the Vector3d position in the world (X,Y,Z) from the position of the body on its orbit
        Vector3d worldPos = Vector3d.NaN();
        double posValue = orbit.param.nu;
        double cosNu = Mathd.Cos(posValue);

        double tmp_p = orbit.param.p;
        if(orbit.param.orbParamsUnits.Equals(OrbitalTypes.orbitalParamsUnits.AU))
            tmp_p *= Units.AU2KM;

        double r = tmp_p / (1 + orbit.param.e*cosNu);

        Vector3d fVec = orbit.orbitedBody.equatorialPlane.forwardVec;
        Vector3d rVec = orbit.orbitedBody.equatorialPlane.rightVec;

        worldPos = r*cosNu*fVec + r*Mathd.Sin(posValue)*rVec;
        worldPos = orbit.orbitRot * worldPos;

        double pointScaler = 1d; // we are in sim
        if(orbit.orbitedBody.spawnAsUIPlanet)
            pointScaler = 2d*orbit.orbitedBody.settings.planetaryParams.radius.val; // we are in the UI Main Menu
        return worldPos/pointScaler;
    }

    public Velocity GetRadialSpeed()
    {
        double sF = 1d;
        if(param.orbParamsUnits == OrbitalTypes.orbitalParamsUnits.AU)
            sF = Units.AU2KM;

        double radialVel = Mathd.Pow(10,5) * Math.Sqrt(orbitedBody.settings.planetaryParams.mu.val/(param.p*sF)) * param.e*Mathd.Sin(param.nu);
        return new Velocity(radialVel, Units.velocity.ms);
    }

    public Velocity GetTangentialSpeed()
    {
        double sF = 1d;
        if(param.orbParamsUnits == OrbitalTypes.orbitalParamsUnits.AU)
            sF = Units.AU2KM;

        double tangentialVel = Mathd.Pow(10,5) * Math.Sqrt(orbitedBody.settings.planetaryParams.mu.val/(param.p*sF)) * (1 + param.e*Mathd.Cos(param.nu));
        return new Velocity(tangentialVel, Units.velocity.ms);
    }

}