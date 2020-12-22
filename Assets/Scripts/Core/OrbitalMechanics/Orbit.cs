using UnityEngine;
using Mathd_Lib;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
using Vectrosity;

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
    VectorLine _lineRenderer;
    public VectorLine lineRenderer {
        get {
            return _lineRenderer;
        }
    }

    public CelestialBody orbitedBody;
    public GameObject orbitingGO;
    public Quaterniond orbitRot; // Rotation from XZ ellipse to XYZ ellipse by applying i, Omega, omega

    //public string suffixGO; // Suffix used for every gameObject that is created and parented to the spaceship

    public Orbit(OrbitalParams orbitalParams, CelestialBody celestialBody, GameObject orbitingBody)
    {
        param = orbitalParams;
        orbitingGO = orbitingBody;
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

        //suffixGO = orbitingGO.name + "Orbit" + param.suffixOrbitType[param.orbitRealPredType];
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
                    return (UniCsts.pv_j2000 - new Vector3d(orbitingGO.transform.position)).normalized;
                
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
                        //return new Vector3d(lineRenderer.GetPosition((int)(lineRenderer.positionCount/2)) - orbitedBody.transform.position).normalized;
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
                    return new Vector3d(orbitedBody.transform.position - orbitingGO.transform.position).normalized;

                case OrbitalTypes.typeOfVectorDir.tangentialVec:
                    Vector3d radial = ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.radialVec);
                    return Vector3d.Cross(radial, param.orbitPlane.normal).normalized;

                case OrbitalTypes.typeOfVectorDir.velocityVec:
                    if(ObjHand.GoTagAndStringAreEqual(UniverseRunner.goTags.Spaceship, orbitingGO.tag)) {
                        Spaceship ship = orbitingGO.GetComponent<Spaceship>();
                        return ship.relativeVel.normalized;
                    }
                    else {
                        CelestialBody body = orbitingGO.GetComponent<CelestialBody>();
                        return body.relativeVel.normalized;
                    }

                case OrbitalTypes.typeOfVectorDir.accelerationVec:
                    if(ObjHand.GoTagAndStringAreEqual(UniverseRunner.goTags.Spaceship, orbitingGO.tag)) {
                        Spaceship ship = orbitingGO.GetComponent<Spaceship>();
                        return ship.relativeAcc.normalized;
                    }
                    else {
                        CelestialBody body = orbitingGO.GetComponent<CelestialBody>();
                        return body.relativeAcc.normalized;
                    }
            }
        }
        return new Vector3d(double.NaN, double.NaN, double.NaN);
    }

    void Init_LineRenderer()
    {
        lineRendererPts = new List<Vector3>();
        _lineRenderer = new VectorLine("Orbit_" + orbitingGO.name , lineRendererPts, 10f);

        GameObject orbitGO;
        if(!orbitedBody.transform.Find("Orbit")) {
            orbitGO = new GameObject("Orbit");
            orbitGO.transform.parent = orbitedBody.transform;
        }
        else
            orbitGO = orbitedBody.transform.Find("Orbit").gameObject;

        _lineRenderer.drawTransform = orbitGO.transform;
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
        Quaterniond iRot = Quaterniond.AngleAxis(param.i, param.ascendingNodeLineDir).GetNormalized();

        param.apogeeLineDir = iRot * param.apogeeLineDir;

        // 3
        Vector3d orbitNormalUp = Vector3d.Cross(param.ascendingNodeLineDir, param.apogeeLineDir).normalized;
        Quaterniond perihelionArgRot = Quaterniond.AngleAxis(90d - param.omega, orbitNormalUp).GetNormalized();
        Quaterniond longAscendingNodeRot = Quaterniond.AngleAxis(-(param.lAscN + 90d), param.vpAxisUp).GetNormalized();
        Quaterniond rot = (longAscendingNodeRot*perihelionArgRot).GetNormalized();

        // 5
        // Taking into account the axial tilt of the planet
        // Will be used to rotate the equatorial plane/equatorial vectors
        /*Quaterniond equatorialAdjustment = Quaterniond.Identity;
        if(!orbitedBody.tag.Equals(UniverseRunner.goTags.Star.ToString()) )//&& !orbitedBody.spawnAsSimpleSphere)
        {
            //Vector3d tangentialVec = orbitedBody.orbit.ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.tangentialVec);
            Vector3d tangentialVec = orbitedBody.orbit.ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.tangentialVec);
            double orbitedBodyAxialTilt = orbitedBody.settings.planetaryParams.axialTilt.val;
            equatorialAdjustment = Quaterniond.AngleAxis(orbitedBodyAxialTilt, -tangentialVec).GetNormalized();
        }
        orbitedBody.equatorialPlane.forwardVec = equatorialAdjustment * orbitedBody.equatorialPlane.forwardVec;
        orbitedBody.equatorialPlane.rightVec = equatorialAdjustment * orbitedBody.equatorialPlane.rightVec;
        orbitedBody.equatorialPlane.normal = equatorialAdjustment * orbitedBody.equatorialPlane.normal;*/
        
        // Rotation Quaternion of the complete rotation (first inclination, then perihelion argument,
        // then longitude of the ascending node
        orbitRot = rot * iRot;

        // Drawing the ellipse
        for(int i = 0; i < param.orbitDrawingResolution; i++)
        {
            double theta = i * incr;
            double x = Mathd.Cos(theta)*tmp_p/(1 + param.e*Mathd.Cos(theta));
            double z = Mathd.Sin(theta)*tmp_p/(1 + param.e*Mathd.Cos(theta));

            Vector3d noRotPoint = orbitedBody.equatorialPlane.forwardVec * x + orbitedBody.equatorialPlane.rightVec * z;
            lineRendererPts.Add( (Vector3)(orbitRot * (noRotPoint /* *scalingFactor*/)) );
        }

        _lineRenderer.points3 = lineRendererPts;
        _lineRenderer.Draw3D();
        // 4
        // normalUp vector has changed after the rotation by longAscendingNodeRot.
        orbitNormalUp = /*equatorialAdjustment * */longAscendingNodeRot * orbitNormalUp;
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
        worldPos = orbit.orbitRot * worldPos;// * orbit.scalingFactor;
        return worldPos;
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