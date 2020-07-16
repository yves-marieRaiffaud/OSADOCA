using UnityEngine;
using Mathd_Lib;
using UnityEngine.Rendering;

public class Orbit
{
    private const double CIRCULAR_ORBIT_TOLERANCE = 10e-5d; // Tolerance for the excentricity value with respect to zero

    public OrbitalParams param;
    public double scalingFactor; // Either 'UniCsts.pl2u' or 'UniCsts.au2u'

    private GameObject lineRendererGO;
    public LineRenderer lineRenderer;
    public CelestialBody orbitedBody;
    public GameObject orbitingGO;
    public Quaterniond orbitRot; // Rotation from XZ ellipse to XYZ ellipse by applying i, Omega, omega

    public double n; // Orbit's Mean motion, in rad.s-1

    public string suffixGO; // Suffix used for every gameObject that is created and parented to the spaceship
    
    public Orbit(OrbitalParams orbitalParams, CelestialBody celestialBody, GameObject orbitingBody) {
        if(orbitalParams.orbParamsUnits == OrbitalParams.orbitalParamsUnits.AU_degree) {
            scalingFactor = UniCsts.au2u;
        }
        else {
            scalingFactor = UniCsts.pl2u;
        }

        switch(orbitalParams.orbitDefType)
        {
            case OrbitalParams.orbitDefinitionType.rarp:
                param = orbitalParams;
                orbitingGO = orbitingBody;
                orbitedBody = celestialBody;

                param.e = (param.ra - param.rp)/(param.ra + param.rp);
                param.p = 2 * param.ra * param.rp/(param.ra + param.rp);
                
                param.a = (param.ra + param.rp)/2;
                param.b = Mathd.Sqrt(param.p * param.a);
                param.c = param.e * param.a;

                param.period = ComputeOrbitalPeriod();
                n = ComputeMeanMotion();

                param.vp = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.vernalPoint);
                param.vpAxisRight = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.vpAxisRight);
                param.vpAxisUp = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.vpAxisUp);

                suffixGO = orbitingGO.name + "Orbit" + param.suffixOrbitType[param.orbitRealPredType];
                CreateAssignOrbitLineRenderer(suffixGO);
                DrawOrbit();
                break;

            case OrbitalParams.orbitDefinitionType.rpe:
                param = orbitalParams;
                orbitingGO = orbitingBody;
                orbitedBody = celestialBody;

                param.ra = param.rp * (-1 - param.e)/(param.e - 1);
                param.p = param.rp * (1 + param.e);

                param.a = (param.ra + param.rp)/2;
                param.b = Mathd.Sqrt(param.p * param.a);
                param.c = param.e * param.a;

                param.period = ComputeOrbitalPeriod();
                n = ComputeMeanMotion();

                param.vp = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.vernalPoint);
                param.vpAxisRight = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.vpAxisRight);
                param.vpAxisUp = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.vpAxisUp);

                suffixGO = orbitingGO.name + "Orbit" + param.suffixOrbitType[param.orbitRealPredType];
                CreateAssignOrbitLineRenderer(suffixGO);
                DrawOrbit();
                break;
            
            case OrbitalParams.orbitDefinitionType.pe:
                param = orbitalParams;
                orbitingGO = orbitingBody;
                orbitedBody = celestialBody;

                param.rp = param.p/(1 + param.e);
                param.ra = param.rp*(-1 - param.e)/(param.e - 1);
                
                param.a = (param.ra + param.rp)/2;
                param.b = Mathd.Sqrt(param.p * param.a);
                param.c = param.e * param.a;

                param.period = ComputeOrbitalPeriod();
                n = ComputeMeanMotion();
                
                param.vp = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.vernalPoint);
                param.vpAxisRight = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.vpAxisRight);
                param.vpAxisUp = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.vpAxisUp);

                suffixGO = orbitingGO.name + "Orbit" + param.suffixOrbitType[param.orbitRealPredType];
                CreateAssignOrbitLineRenderer(suffixGO);
                DrawOrbit();
                break;
        }
    }

    private void CreateAssignOrbitLineRenderer(string nameOrbitingBody)
    {
        lineRendererGO = UsefulFunctions.CreateAssignGameObject(suffixGO, typeof(LineRenderer));
        lineRendererGO.layer = 10; // Layer 10 is 'Orbit' Layer (which is not rendered in teh background camera)
        lineRendererGO.tag = UniverseRunner.goTags.Orbit.ToString();

        lineRenderer = lineRendererGO.GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.receiveShadows = false;
        lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        lineRenderer.transform.parent = GameObject.Find("Orbits").transform;
        lineRenderer.widthCurve = AnimationCurve.Constant(0f, 1f, 30f);
        lineRenderer.sharedMaterial = Resources.Load("OrbitMaterial", typeof(Material)) as Material;
    }


    public void UpdateLineRendererPos()
    {
        lineRendererGO.transform.localPosition = orbitedBody.transform.localPosition;
    }

    public static Vector3d GetWorldPositionFromOrbit(Orbit orbit, OrbitalParams.bodyPositionType posType)
    {
        // Return the Vector3d position in the world (X,Y,Z) from the position of the body on its orbit
        Vector3d worldPos = Vector3d.NaN();
        switch(posType)
        {
            case OrbitalParams.bodyPositionType.nu:
                double posValue = orbit.param.nu * UniCsts.deg2rad;
                double cosNu = Mathd.Cos(posValue);
                double r = orbit.param.p / (1 + orbit.param.e*cosNu);

                Vector3d fVec = orbit.orbitedBody.settings.equatorialPlane.forwardVec;
                Vector3d rVec = orbit.orbitedBody.settings.equatorialPlane.rightVec;
                worldPos = r*cosNu*fVec + r*Mathd.Sin(posValue)*rVec;
                //worldPos = new Vector3d(r * cosNu, 0d, r*Mathd.Sin(posValue)); // Position without the ellispe rotation
                worldPos = orbit.orbitRot * worldPos * orbit.scalingFactor;
                break;

            case OrbitalParams.bodyPositionType.m0:
                break;
        }
        return worldPos;
    }


    public void DrawOrbit()
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
        Vector3[] pos = new Vector3[param.orbitDrawingResolution];
        double incr = 2*Mathf.PI/param.orbitDrawingResolution;

        double thetaAphelion = (int)(param.orbitDrawingResolution/2) * incr;
        double xAphelion = Mathd.Cos(thetaAphelion)*param.p/(1 + param.e*Mathd.Cos(thetaAphelion));
        double zAphelion = Mathd.Sin(thetaAphelion)*param.p/(1 + param.e*Mathd.Cos(thetaAphelion));
        Vector3d aphelionNoRot = new Vector3d(xAphelion, 0d, zAphelion) * scalingFactor;

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
        Quaterniond equatorialAdjustment = Quaterniond.Identity;
        if(!orbitedBody.tag.Equals(UniverseRunner.goTags.Star.ToString()))
        {
            Vector3d tangentialVec = orbitedBody.orbit.ComputeDirectionVector(OrbitalParams.typeOfVectorDir.tangentialVec);
            double orbitedBodyAxialTilt = orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()];
            equatorialAdjustment = Quaterniond.AngleAxis(orbitedBodyAxialTilt, -tangentialVec).GetNormalized();
        }
        orbitedBody.settings.equatorialPlane.forwardVec = equatorialAdjustment * orbitedBody.settings.equatorialPlane.forwardVec;
        orbitedBody.settings.equatorialPlane.rightVec = equatorialAdjustment * orbitedBody.settings.equatorialPlane.rightVec;
        orbitedBody.settings.equatorialPlane.normal = equatorialAdjustment * orbitedBody.settings.equatorialPlane.normal;
        
        // Rotation Quaternion of the complete rotation (first inclination, then perihelion argument,
        // then longitude of the ascending node
        orbitRot = rot * iRot;

        // Drawing the ellipse
        for(int i = 0; i < param.orbitDrawingResolution; i++)
        {
            double theta = i * incr;
            double x = Mathd.Cos(theta)*param.p/(1 + param.e*Mathd.Cos(theta));
            double z = Mathd.Sin(theta)*param.p/(1 + param.e*Mathd.Cos(theta));

            Vector3d noRotPoint = orbitedBody.settings.equatorialPlane.forwardVec * x + orbitedBody.settings.equatorialPlane.rightVec * z;
            pos[i] = (Vector3)(orbitRot * (noRotPoint * scalingFactor));
        }

        lineRenderer.positionCount = param.orbitDrawingResolution;
        lineRenderer.SetPositions(pos);
        if(param.drawOrbit) { lineRenderer.enabled = true; }
        else { lineRenderer.enabled = false; }

        // 4
        // normalUp vector has changed after the rotation by longAscendingNodeRot.
        orbitNormalUp = equatorialAdjustment * longAscendingNodeRot * orbitNormalUp;
        param.apogeeLineDir = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.apogeeLine);
        Vector3d orbitPlaneRightVec = Vector3d.Cross(param.apogeeLineDir, orbitNormalUp);

        if(param.orbitPlane == null) { param.orbitPlane = new OrbitPlane(); }
        param.orbitPlane.AssignOrbitPlane(param.apogeeLineDir, orbitPlaneRightVec, new Vector3d(pos[(int)(pos.Length/2)]));
        //Orbit plane.normal will point downward.
        
        if(!Vector3d.IsValidAndNoneZero(param.ascendingNodeLineDir)) {
            param.ascendingNodeLineDir = Vector3d.Cross(param.apogeeLineDir, orbitNormalUp).normalized; // Orbit is circular (coincident planes) or does not intersect equatorial plane
        }
    }



    public double ComputeOrbitalPeriod()
    {
        if(orbitedBody == null)
        {
            return float.NaN;
        }
        else{
            double T = 2d * Mathd.PI * Mathd.Pow(10,-2) * Mathd.Sqrt(Mathd.Pow(param.a, 3) / orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.mu.ToString()]); // without E13 for real value
            return T;
        }
    }

    public double ComputeMeanMotion()
    {
        n = 2d*Mathd.PI/param.period; // rad.s-1
        return n;
    }

    public Vector3d GetPerigeePoint()
    {
        // Perigee point is the first point of the Line Renderer
        if(lineRenderer != null){
            return new Vector3d(lineRenderer.GetPosition(0));
        }
        return Vector3d.NaN();
    }

    public Vector3d GetApogeePoint()
    {
        // Apogee point is the point at lineRenderer.Length/2
        if(lineRenderer != null){
            return new Vector3d(lineRenderer.GetPosition((int)(lineRenderer.positionCount/2)));
        }
        return Vector3d.NaN();
    }

    public void RecomputeMainDirectionVectors()
    {
        param.apogeeLineDir = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.apogeeLine);
        param.ascendingNodeLineDir = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.ascendingNodeLine);
    }

    public Vector3d ComputeDirectionVector(OrbitalParams.typeOfVectorDir vecType)
    {
        if(orbitedBody == null)
        {
            return new Vector3d(double.NaN, double.NaN, double.NaN);
        }
        else {
            switch(vecType)
            {
                case OrbitalParams.typeOfVectorDir.vernalPoint:
                    return (UniCsts.pv - new Vector3d(orbitingGO.transform.position)).normalized;
                
                case OrbitalParams.typeOfVectorDir.vpAxisRight:
                    if(!Vector3d.IsValid(param.vp)){
                        ComputeDirectionVector(OrbitalParams.typeOfVectorDir.vernalPoint);
                    }
                    return new Vector3d(param.vp.y, param.vp.z, param.vp.x).normalized;
                
                case OrbitalParams.typeOfVectorDir.vpAxisUp:
                    if(!Vector3d.IsValid(param.vp)){
                        ComputeDirectionVector(OrbitalParams.typeOfVectorDir.vernalPoint);
                    }
                    return new Vector3d(param.vp.z, param.vp.x, param.vp.y).normalized;

                case OrbitalParams.typeOfVectorDir.apogeeLine:                    
                    if(lineRenderer != null) {
                        //return new Vector3d(lineRenderer.GetPosition((int)(lineRenderer.positionCount/2)) - orbitedBody.transform.position).normalized;
                        return new Vector3d(lineRenderer.GetPosition((int)(lineRenderer.positionCount/2))).normalized;
                    }
                    return new Vector3d(double.NaN, double.NaN, double.NaN);

                case OrbitalParams.typeOfVectorDir.ascendingNodeLine:
                    if(UsefulFunctions.DoublesAreEqual(param.i, 0d, 10e-4))
                    {
                        // There is an infinity of Ascending nodes
                        // Returning the ascending node line as the perpendicular vector with respect to the apogee line, and in the orbit plane
                        Vector3d tmp = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.apogeeLine);
                        return Vector3d.Cross(tmp, param.orbitPlane.normal);
                    }
                    else {
                        Vector3d intersectPoint = new Vector3d();
                        Vector3d intersectLine = new Vector3d();
                        OrbitPlane.PlaneIntersectPlane(param.orbitPlane, orbitedBody.settings.equatorialPlane, out intersectPoint, out intersectLine);
                        return intersectLine;
                    }
                
                case OrbitalParams.typeOfVectorDir.radialVec:
                    return new Vector3d(orbitedBody.transform.position - orbitingGO.transform.position).normalized;

                case OrbitalParams.typeOfVectorDir.tangentialVec:
                    Vector3d radial = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.radialVec);
                    return Vector3d.Cross(radial, param.orbitPlane.normal).normalized;
                
                case OrbitalParams.typeOfVectorDir.velocityVec:
                    if(UsefulFunctions.GoTagAndStringAreEqual(UniverseRunner.goTags.Spaceship, orbitingGO.tag))
                    {
                        Spaceship ship = orbitingGO.GetComponent<Spaceship>();
                        return ship.orbitedBodyRelativeVel.normalized;
                    }
                    else {
                        CelestialBody body = orbitingGO.GetComponent<CelestialBody>();
                        return body.orbitedBodyRelativeVel.normalized;
                    }
                
                case OrbitalParams.typeOfVectorDir.accelerationVec:
                    if(UsefulFunctions.GoTagAndStringAreEqual(UniverseRunner.goTags.Spaceship, orbitingGO.tag))
                    {
                        Spaceship ship = orbitingGO.GetComponent<Spaceship>();
                        return ship.orbitedBodyRelativeAcc.normalized;
                    }
                    else {
                        CelestialBody body = orbitingGO.GetComponent<CelestialBody>();
                        return body.orbitedBodyRelativeAcc.normalized;
                    }
            }
        }
        return new Vector3d(double.NaN, double.NaN, double.NaN);
    }

    public void DrawDirection(OrbitalParams.typeOfVectorDir dirType, float lineWidth=1f, float lineLength=100f, Vector3 startPos=default(Vector3))
    {
        if(!Vector3d.IsValid(param.vp)) {
            return;
        }
        else {
            // Check if GameObject already Exists
            GameObject dirGO = UsefulFunctions.CreateAssignGameObject(param.suffixVectorDir[dirType] + suffixGO);
            dirGO.layer = 10; // 'Orbit' layer
            dirGO.tag = UniverseRunner.goTags.Orbit.ToString();
            LineRenderer dirLR = (LineRenderer) UsefulFunctions.CreateAssignComponent(typeof(LineRenderer), dirGO);
            dirGO.transform.parent = GameObject.Find(suffixGO).transform;

            Vector3[] pos = new Vector3[2];
            switch(dirType)
            {
                case OrbitalParams.typeOfVectorDir.vernalPoint:
                    pos[0] = Vector3.Scale(orbitedBody.transform.position, (Vector3)param.vp);
                    pos[1] = (float)(param.ra*scalingFactor) * (Vector3)param.vp;
                    break;
                
                case OrbitalParams.typeOfVectorDir.vpAxisRight:
                    pos[0] = Vector3.Scale(orbitedBody.transform.position, (Vector3)param.vpAxisRight);
                    pos[1] = (float)(param.ra*scalingFactor) * (Vector3)param.vpAxisRight;
                    break;
                
                case OrbitalParams.typeOfVectorDir.vpAxisUp:
                    pos[0] = Vector3.Scale(orbitedBody.transform.position, (Vector3)param.vpAxisUp);
                    pos[1] = (float)(param.ra*scalingFactor) * (Vector3)param.vpAxisUp;
                    break;
                
                case OrbitalParams.typeOfVectorDir.apogeeLine:
                    pos[0] = (float)(-param.rp*scalingFactor) * (Vector3)param.apogeeLineDir;
                    pos[1] = (float)(param.ra*scalingFactor) * (Vector3)param.apogeeLineDir;
                    break;
                
                case OrbitalParams.typeOfVectorDir.ascendingNodeLine:
                    pos[0] = (float)-(param.ra*scalingFactor) * (Vector3)param.ascendingNodeLineDir;
                    pos[1] = (float)(param.ra*scalingFactor) * (Vector3)param.ascendingNodeLineDir;
                    break;
                
                case OrbitalParams.typeOfVectorDir.tangentialVec:
                    if(startPos != null){
                        Vector3d tangentialVec = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.tangentialVec);
                        pos[0] = Vector3.zero;
                        pos[1] = lineLength * (Vector3)tangentialVec;
                        dirLR.transform.position = startPos;
                    }
                    break;

                case OrbitalParams.typeOfVectorDir.radialVec:
                    if(startPos != null){
                        Vector3d radialVec = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.radialVec);
                        pos[0] = Vector3.zero;
                        pos[1] = lineLength * (Vector3)radialVec;
                        dirLR.transform.position = startPos;
                    }
                    break;
                
                case OrbitalParams.typeOfVectorDir.velocityVec:
                    if(startPos != null){
                        Vector3d speedVec = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.velocityVec);
                        pos[0] = Vector3.zero;
                        pos[1] = lineLength * (Vector3)speedVec;
                        dirLR.transform.position = startPos;
                    }
                    break;
                
                case OrbitalParams.typeOfVectorDir.accelerationVec:
                    if(startPos != null){
                        Vector3d accVec = ComputeDirectionVector(OrbitalParams.typeOfVectorDir.accelerationVec);
                        pos[0] = Vector3.zero;
                        pos[1] = lineLength * (Vector3)accVec;
                        dirLR.transform.position = startPos;
                    }
                    break;
            }
            dirLR.useWorldSpace = false;
            dirLR.positionCount = pos.Length;
            dirLR.SetPositions(pos);
            dirLR.widthCurve = AnimationCurve.Constant(0f, lineLength, lineWidth);
            dirLR.sharedMaterial = Resources.Load("OrbitMaterial", typeof(Material)) as Material;
        }
    }

    public void DrawAllDirections()
    {
        foreach(OrbitalParams.typeOfVectorDir direction in param.suffixVectorDir.Keys)
        {
            DrawDirection(direction);
        }
    }

    public void DrawVector(Vector3d vector, string nameGameObject)
    {
        if(!Vector3d.IsValid(vector)) {
            return;
        }
        else {
            // Check if GameObject already Exists
            GameObject dirGO = UsefulFunctions.CreateAssignGameObject(nameGameObject + suffixGO);
            dirGO.layer = 10; // 'Orbit' layer
            LineRenderer dirLR = (LineRenderer) UsefulFunctions.CreateAssignComponent(typeof(LineRenderer), dirGO);
            dirGO.tag = UniverseRunner.goTags.Orbit.ToString();
            dirGO.transform.parent = GameObject.Find(suffixGO).transform;

            Vector3[] pos = new Vector3[2];
            pos[0] = (float)(-param.ra*scalingFactor) * (Vector3)vector;
            pos[1] = (float)(param.ra*scalingFactor) * (Vector3)vector;

            dirLR.useWorldSpace = false;
            dirLR.positionCount = pos.Length;
            dirLR.SetPositions(pos);
            dirLR.widthCurve = AnimationCurve.Constant(0f, 1f, 100f);
            dirLR.sharedMaterial = Resources.Load("OrbitMaterial", typeof(Material)) as Material;
        }
    }



    public double GetOrbitalSpeedFromOrbit()
    {
        // r : radius of the celestialBody + altitude of the orbiting object, in km
        if(UsefulFunctions.DoublesAreEqual(param.e, 0d, Mathd.Pow(10,-5)))
        {
            // Orbit is circular
            return GetCircularOrbitalSpeed();
        }
        else if (UsefulFunctions.isInRange(param.e, 0d, 1d, UsefulFunctions.isInRangeFlags.both_excluded)){
            // Orbit is elliptic
            double a = param.a;
            if(param.orbParamsUnits == OrbitalParams.orbitalParamsUnits.AU_degree) {
                a *= UniCsts.au2km;
            }
            double velocity = Mathd.Pow(10,5) * Mathd.Sqrt(orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.mu.ToString()] * (2d/(Get_R()) - 1d/a));
            return velocity;
        }
        return double.NaN;
    }

    public double ComputeLiberationSpeed()
    {
        double velocity = Mathd.Sqrt(2d) * GetCircularOrbitalSpeed();
        return velocity;
    }

    public double GetCircularOrbitalSpeed()
    {
        double velocity = Mathd.Pow(10,5) * Mathd.Sqrt(orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.mu.ToString()] / Get_R());
        return velocity;
    }



    public double Get_R()
    {
        // Returns the distance in real world (in km) from the spaceship position to the centre of the orbited body
        double distance = Vector3d.Distance(new Vector3d(orbitingGO.transform.position), new Vector3d(orbitedBody.transform.position));
        distance /= scalingFactor; // Either km or AU
        if(param.orbParamsUnits == OrbitalParams.orbitalParamsUnits.AU_degree) {
            // AU
            return distance * UniCsts.au2km;
        }
        else {
            return distance; // distance already in km
        }
    }

    public double Get_R(Vector3d shipPosition)
    {
        // Returns the distance in real world (in km) from the spaceship position to the centre of the orbited body
        double distance = Vector3d.Distance(shipPosition, new Vector3d(orbitedBody.transform.position));
        distance /= scalingFactor; // Either km or AU
        if(param.orbParamsUnits == OrbitalParams.orbitalParamsUnits.AU_degree) {
            // AU
            return distance * UniCsts.au2km;
        }
        else {
            return distance; // distance already in km
        }
    }

    public double GetAltitude()
    {
        // Returns the altitude in real world (in km) from the spaceship position to the surface of the orbited body
        return Get_R() - orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()];
    }

    public double GetAltitude(Vector3d shipPosition)
    {
        // Returns the altitude in real world (in km) from the spaceship position to the surface of the orbited body
        return Get_R(shipPosition) - orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()];
    }

    public bool IsCircular()
    {
        // Check if the orbit is circular
        if(UsefulFunctions.DoublesAreEqual(param.e, 0d, CIRCULAR_ORBIT_TOLERANCE))
        {
            return true; // orbit is circular
        }
        else { return false; } // orbit is not circular
    }

}