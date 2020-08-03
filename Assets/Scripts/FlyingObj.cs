using System;
using UnityEngine;
using Mathd_Lib;

public class FlyingObj
{
    private UniverseRunner universe;
    //==============
    public FlyingObj(UniverseRunner universeRunnerInstance)
    {
        universe = universeRunnerInstance;
    }
    //===============================================================================
    //===============================================================================
    //===============================================================================
    //===============================================================================
    //===============================================================================
    public static T GetObjectSettings<T>(UnityEngine.Object body)
    {
        // 'T' is either SpaceshipSettings or CelestialBodySettings
        if(body is Spaceship)
        {
            Spaceship spaceship = (Spaceship)body;
            return (T)(dynamic)spaceship.settings;
        }
        else
        {
            CelestialBody celestialBody = (CelestialBody)body;
            return (T)(dynamic)celestialBody.settings;
        }
    }

    public static T2 GetObjectSettings<T1, T2>(T1 body)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        // 'T1' is either Spaceship or CelestialBody
        // 'T2' is either SpaceshipSettings or CelestialBodySettings
        if(body is Spaceship)
        {
            Spaceship spaceship = (Spaceship)(dynamic)body;
            return (T2)(dynamic)spaceship.settings;
        }
        else
        {
            CelestialBody celestialBody = (CelestialBody)(dynamic)body;
            return (T2)(dynamic)celestialBody.settings;
        }
    }

    public static T CastObjectToType<T>(UnityEngine.Object body)
    {
        // T is either Spaceship or CelestialBody
        if(body is Spaceship)
        {
            Spaceship spaceship = (Spaceship)body;
            return (T)(dynamic)spaceship;
        }
        else
        {
            CelestialBody celestialBody = (CelestialBody)body;
            return (T)(dynamic)celestialBody;
        }
    }
    //===============================================================================
    //===============================================================================
    //===============================================================================
    //===============================================================================
    //===============================================================================
    public void InitializeFlyingObj<T1, T2>(UnityEngine.Object body)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        T1 castBody = CastObjectToType<T1>(body);
        T2 bodySettings = GetObjectSettings<T2>(body);

        castBody.SetDistanceScaleFactor(); // Set the scaleFactor of the body, depending on the definition of the orbit (in km or in AU)
        // From this point, we can directly use the 'distanceScaleFactor' property of the body

        if(body is Spaceship)
        {
            SpaceshipSettings shipSettings = (SpaceshipSettings)(dynamic)bodySettings;
            if(shipSettings.startFromGround)
            {
                // Init position of the ship and return == do not init any orbit
                InitializeBodyPosition<T1, T2>(body);
                return;
            }
        }

        InitializeOrbit<T1, T2>(body);
        InitializeBodyPosition<T1, T2>(body);
        InitializeOrbitalSpeed<T1, T2>(body);
        InitializeDirVecLineRenderers<T1, T2>(body);

        // Init Axial Tilt for CelestialBody
        UniverseRunner.goTags starTag = UniverseRunner.goTags.Star;
        UniverseRunner.goTags planetTag = UniverseRunner.goTags.Planet;
        if(UsefulFunctions.StringIsOneOfTheTwoTags(starTag, planetTag, castBody._gameObject.tag))
        {
            CelestialBody celestBody = (CelestialBody)body;
            celestBody.InitializeAxialTilt();
        }

    }

    /// <summary>
    /// Initialize Orbit, either for a Spaceship or a CelestialBody
    /// T1: The type of the argument 'body': either 'Spaceship' or 'CelestialBody'
    /// T2: The type of the body Settings variable: either 'SpaceshipSettings' or 'CelestialBodySettings'
    /// </summary>
    /// <param name="body">The object of type T1: either 'Spaceship' or 'CelestialBody'</param>
    public void InitializeOrbit<T1, T2>(UnityEngine.Object body)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        if(body is T1)
        {
            T1 castBody = CastObjectToType<T1>(body); // Spaceship or CelestialBody
            T2 settings = GetObjectSettings<T2>(body); // SpaceshipSettings or CelestialBodySettings
            castBody.orbit = new Orbit(castBody.orbitalParams, castBody.orbitalParams.orbitedBody.GetComponent<CelestialBody>(), castBody._gameObject);
        }
        else {
            Debug.LogError("Specified UnityEngine.Object is not of the specified generic type.");
        }
    }

    /// <summary>
    /// Initialize position of the body, either for a Spaceship or a CelestialBody
    /// T1: The type of the argument 'body': either 'Spaceship' or 'CelestialBody'
    /// T2: The type of the body Settings variable: either 'SpaceshipSettings' or 'CelestialBodySettings'
    /// </summary>
    /// <param name="body">The object of type T1: either 'Spaceship' or 'CelestialBody'</param>
    public void InitializeBodyPosition<T1, T2>(UnityEngine.Object body)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        if(body is T1)
        {
            T1 castBody = CastObjectToType<T1>(body); // Spaceship or CelestialBody
            T2 settings = GetObjectSettings<T2>(body); // SpaceshipSettings or CelestialBodySettings

            Vector3d bodyRelatedPos;
            if(body is Spaceship)
            {
                SpaceshipSettings shipSettings = (SpaceshipSettings)(dynamic)settings;
                if(shipSettings.startFromGround)
                {
                    // A spaceship with planetary surface init
                    double latitude = shipSettings.groundStartLatLong.x;
                    double longitude = shipSettings.groundStartLatLong.y;
                    bodyRelatedPos = castBody.orbitalParams.orbitedBody.GetWorldPositionFromGroundStart(latitude, longitude);
                }
                else {
                    // A spaceship with in orbit init
                    //bodyRelatedPos = Orbit.GetWorldPositionFromOrbit(castBody.orbit, OrbitalTypes.bodyPositionType.nu);
                    bodyRelatedPos = Orbit.GetWorldPositionFromLineRendererOrbit(castBody.orbit, OrbitalTypes.bodyPositionType.nu);
                }
            }
            else {
                // A celestialBody with in orbit init
                bodyRelatedPos = Orbit.GetWorldPositionFromOrbit(castBody.orbit, OrbitalTypes.bodyPositionType.nu);
            }

            castBody.realPosition = UsefulFunctions.AlignPositionVecWithParentPos(bodyRelatedPos, castBody.orbitalParams.orbitedBody.transform.position);
            castBody._gameObject.transform.position = (Vector3)castBody.realPosition;
        }
    }

    public void InitializeOrbitalSpeed<T1, T2>(UnityEngine.Object body)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        T1 castBody = CastObjectToType<T1>(body);
        T2 bodySettings = GetObjectSettings<T2>(body);

        // Init orbital speed
        Vector3d tangentialVec = castBody.orbit.ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.tangentialVec);
        double orbitalSpeed = castBody.orbit.GetOrbitalSpeedFromOrbit();
        castBody.orbitedBodyRelativeVel = tangentialVec * orbitalSpeed;

        Vector3d speedOfOrbitedBody = Vector3d.zero;
        if(!castBody.orbitalParams.orbitedBodyName.Equals("None")) {
            CelestialBody orbitedBody = castBody.orbitalParams.orbitedBody;
            speedOfOrbitedBody = orbitedBody.orbitedBodyRelativeVel;
        } 

        // Init orbital speed of the Rigidbody
        double scaleFactor = castBody.distanceScaleFactor;
        Rigidbody rb = castBody._gameObject.GetComponent<Rigidbody>();
        Vector3d absoluteScaledVelocity = castBody.orbitedBodyRelativeVel*scaleFactor + speedOfOrbitedBody*UniCsts.m2km2au2u;
        rb.velocity = (Vector3)absoluteScaledVelocity;

        if(body is Spaceship) {
            InitializeSpaceshipRotation<T1>(castBody, (Vector3)tangentialVec);
        }
    }

    public void InitializeSpaceshipRotation<T1>(T1 shipBody, Vector3 directionVector)
    where T1: FlyingObjCommonParams
    {
        // The y-axis of the spaceship will be aligned with the passed 'directionVector'
        shipBody._gameObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, (Vector3)directionVector);
    }

    /// <summary>
    /// Initialize LineRenderers of the direction vectors, for an orbit
    /// T1: The type of the argument 'body': either 'Spaceship' or 'CelestialBody'
    /// T2: The type of the body Settings variable: either 'SpaceshipSettings' or 'CelestialBodySettings'
    /// </summary>
    /// <param name="body">The object of type T1: either 'Spaceship' or 'CelestialBody'</param>
    public void InitializeDirVecLineRenderers<T1, T2>(UnityEngine.Object body)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        if(body is T1)
        {
            T1 castBody = CastObjectToType<T1>(body); // Spaceship or CelestialBody
            T2 settings = GetObjectSettings<T2>(body); // SpaceshipSettings or CelestialBodySettings

            castBody.orbit.RecomputeMainDirectionVectors();
            float lineLength = 10f; // Default length for a spaceship orbiting a planet
            if(castBody.orbit.orbitingGO.tag == UniverseRunner.goTags.Planet.ToString()) {
                lineLength = 100f;
            }
            if(castBody.orbitalParams.drawDirections)
            {
                foreach(OrbitalTypes.typeOfVectorDir vectorDir in Enum.GetValues(typeof(OrbitalTypes.typeOfVectorDir)))
                {
                    if (castBody.orbitalParams.selectedVectorsDir.HasFlag(vectorDir))
                    {
                        if(vectorDir == OrbitalTypes.typeOfVectorDir.radialVec || vectorDir == OrbitalTypes.typeOfVectorDir.tangentialVec
                        || vectorDir == OrbitalTypes.typeOfVectorDir.velocityVec || vectorDir == OrbitalTypes.typeOfVectorDir.accelerationVec)
                        {
                            castBody.orbit.DrawDirection(vectorDir, lineLength, 10_000f, castBody._gameObject.transform.position);
                        }
                        else{
                            castBody.orbit.DrawDirection(vectorDir, lineLength);
                        }
                    }
                    castBody.orbit.DrawVector(castBody.orbitalParams.orbitPlane.normal, "NormalUp");
                }
            }
        }
        else {
            Debug.LogError("Specified UnityEngine.Object is not of the specified generic type.");
        }
    }

    //===============================================================================
    //===============================================================================
    //===============================================================================
    //===============================================================================
    //===============================================================================
    public void GravitationalStep()
    {
        // First computing the acceleration updates for each Star, Planet or Spaceship 
        foreach(Transform obj in universe.physicsObjArray)
        {
            ComputeNewPosition(obj, obj.tag);
        }

        // Once everything has been computed, apply the new accelerations at every objects
        foreach(Transform obj in universe.physicsObjArray)
        {
            ApplyNewPosition(obj, obj.tag);
        }
    }

    public void ComputeNewPosition(Transform obj, string objTag)
    {
        // Compute acceleration, velocity and the new position of either a Planet or a Spaceship, due to gravitational pull
        CelestialBody orbitedBody;
        switch(UsefulFunctions.CastStringToGoTags(objTag))
        {
            case UniverseRunner.goTags.Star:
                // Do nothing for now
                break;

            case UniverseRunner.goTags.Planet:
                CelestialBody celestBody = obj.GetComponent<CelestialBody>();
                if(celestBody.orbitalParams.orbitedBody != null)
                {
                    orbitedBody = celestBody.orbitalParams.orbitedBody.GetComponent<CelestialBody>();
                    GravitationalUpdate<CelestialBody, CelestialBodySettings>(orbitedBody, celestBody);
                }
                break;
            
            case UniverseRunner.goTags.Spaceship:
                Spaceship ship = obj.GetComponent<Spaceship>();
                orbitedBody = ship.orbitalParams.orbitedBody.GetComponent<CelestialBody>();
                GravitationalUpdate<Spaceship, SpaceshipSettings>(orbitedBody, ship);
                break;
        }
    }

    public void ApplyNewPosition(Transform obj, string objTag)
    {
        // Move the rigidbody of a Planet or a Spaceship to its new position due to gravitational pull
        switch(UsefulFunctions.CastStringToGoTags(objTag))
        {
            case UniverseRunner.goTags.Planet:
                CelestialBody celestBody = obj.GetComponent<CelestialBody>();
                ApplyRigbidbodyAccUpdate<CelestialBody, CelestialBodySettings>(celestBody, celestBody.settings);
                break;
            
            case UniverseRunner.goTags.Spaceship:
                Spaceship ship = obj.GetComponent<Spaceship>();
                ApplyRigbidbodyAccUpdate<Spaceship, SpaceshipSettings>(ship, ship.settings);
                break;
        }
    }
    //===============================================================================
    //===============================================================================
    //===============================================================================
    //===============================================================================
    public void GravitationalUpdate<T1 ,T2>(CelestialBody pullingBody, T1 orbitingBody)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        if(orbitingBody is T1 && pullingBody != null)
        {
            T2 settings = GetObjectSettings<T1, T2>(orbitingBody);

            //ComputeGravitationalAcc<T1, T2>(pullingBody, orbitingBody, settings);
            ComputeGravitationalAccNBODY<T1, T2>(pullingBody, orbitingBody, settings);
            
            ComputeUpdatedVelocity<T1, T2>(orbitingBody, settings);
            ComputeUpdatedPosition<T1, T2>(orbitingBody, settings);
        }
    }

    /// <summary>
    /// Compute gravitationnal pull from a CelestialBody on a Spaceship or a CelestialBody
    /// T1: The type of the argument 'orbitingBody': either 'Spaceship' or 'CelestialBody'
    /// T2: The type of the orbitingBody Settings variable: either 'SpaceshipSettings' or 'CelestialBodySettings'
    /// </summary>
    /// <param name="pullingBody">A CelestialBody that is pulling the orbiting body</param>
    /// <param name="orbitingBody">The orbiting body, either a 'Spaceship' or a 'CelestialBody'</param>
    public void ComputeGravitationalAcc<T1, T2>(CelestialBody pullingBody, T1 orbitingBody, T2 settings)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        Vector3d pullinBodyPos = new Vector3d(pullingBody.transform.position);
        Transform castOrbitingBodyTr = orbitingBody._gameObject.transform; // Spaceship Tr or CelestialBody Tr

        Vector3d r = new Vector3d(castOrbitingBodyTr.position) - pullinBodyPos;
        double scalingFactor = UniCsts.u2au * UniCsts.au2km; // km, for planets
        if(orbitingBody.orbitalParams.orbParamsUnits == OrbitalTypes.orbitalParamsUnits.km_degree)
        {
            scalingFactor = UniCsts.u2pl; // km, for spaceships
        }

        r *= scalingFactor; // km
        double dstPow3 = Mathd.Pow(r.magnitude, 3); // km^3
        double mu = pullingBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.mu.ToString()];
        Vector3d acc =  - Mathd.Pow(10,7) * mu * r / dstPow3; // m.s-2

        if(!Vector3d.IsValid(acc) || UsefulFunctions.DoublesAreEqual(dstPow3, 0d))
        {
            Debug.LogError("Acc is not valid or distance between the pulling body and the target body is null");
            orbitingBody.orbitedBodyRelativeAcc = Vector3d.positiveInfinity;
        }
        else {
            orbitingBody.orbitedBodyRelativeAcc = acc;
        }
    }

    public void ComputeGravitationalAccNBODY<T1, T2>(CelestialBody pullingBody, T1 orbitingBody, T2 settings)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        Vector3d pullinBodyPos = new Vector3d(pullingBody.transform.position);
        Transform castOrbitingBodyTr = orbitingBody._gameObject.transform; // Spaceship Tr or CelestialBody Tr

        Vector3d r = new Vector3d(castOrbitingBodyTr.position) - pullinBodyPos;
        double scalingFactor = UniCsts.u2au * UniCsts.au2km; // km, for planets
        if(orbitingBody.orbitalParams.orbParamsUnits == OrbitalTypes.orbitalParamsUnits.km_degree)
        {
            scalingFactor = UniCsts.u2pl; // km, for spaceships
        }

        r *= scalingFactor; // km
        double dstPow3 = Mathd.Pow(r.magnitude, 3); // km^3
        double mu = pullingBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.mu.ToString()];
        Vector3d acc =  - Mathd.Pow(10,7) * mu * r / dstPow3; // m.s-2

        if(!Vector3d.IsValid(acc) || UsefulFunctions.DoublesAreEqual(dstPow3, 0d))
        {
            Debug.LogError("Acc is not valid or distance between the pulling body and the target body is null");
            orbitingBody.orbitedBodyRelativeAcc = Vector3d.positiveInfinity;
        }
        else {
            orbitingBody.orbitedBodyRelativeAcc = acc;
        }
    }

    public void ComputeUpdatedVelocity<T1, T2>(T1 orbitingBody, T2 settings)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        orbitingBody.orbitedBodyRelativeVelIncr = Time.fixedDeltaTime * orbitingBody.orbitedBodyRelativeAcc;
        orbitingBody.orbitedBodyRelativeVel += orbitingBody.orbitedBodyRelativeVelIncr;
    }

    public void ComputeUpdatedPosition<T1, T2>(T1 castBody, T2 settings)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        Vector3d updatedPos = Time.fixedDeltaTime * castBody.distanceScaleFactor * castBody.orbitedBodyRelativeVel;
        castBody.realPosition += updatedPos;
    }

    public void ApplyRigbidbodyAccUpdate<T1, T2>(T1 castBody, T2 settings)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        Rigidbody rb = castBody._gameObject.GetComponent<Rigidbody>();

        double scaleFactor = castBody.distanceScaleFactor;

        Vector3d force = castBody.orbitedBodyRelativeAcc * scaleFactor;
        rb.AddForce((Vector3)force, ForceMode.Acceleration);

        Vector3d orbitedBodyVel = Vector3d.zero;
        if(!castBody.orbitalParams.orbitedBodyName.Equals("None")) {
            CelestialBody orbitedBody = castBody.orbitalParams.orbitedBody;
            orbitedBodyVel = orbitedBody.orbitedBodyRelativeVel;
        } 
        rb.velocity = (Vector3)(castBody.orbitedBodyRelativeVel*scaleFactor + orbitedBodyVel*UniCsts.m2km2au2u);
    }
    //===============================================================================
    //===============================================================================
    //===============================================================================
    //===============================================================================
    //===============================================================================
    public Vector3d GetAbsoluteVelocity<T1>(T1 castBody)
    where T1 : FlyingObjCommonParams
    {
        // Returns a Vector3d of the absolute velocity of the passed flyingObjBody: either a Spaceship or a CelestialBody
        Vector3d speedOfOrbitedBody = Vector3d.zero;
        if(!castBody.orbitalParams.orbitedBodyName.Equals("None")) {
            CelestialBody orbitedBody = castBody.orbitalParams.orbitedBody;
            speedOfOrbitedBody = orbitedBody.orbitedBodyRelativeVel;
        } 

        double scaleFactor = castBody.distanceScaleFactor;
        return castBody.orbitedBodyRelativeVel*scaleFactor + speedOfOrbitedBody*UniCsts.m2km2au2u;
    }
}