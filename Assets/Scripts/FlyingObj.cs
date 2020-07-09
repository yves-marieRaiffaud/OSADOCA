using System;
using UnityEngine;
using Mathd_Lib;

public static class FlyingObj
{

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

    /// <summary>
    /// Initialize Orbit, either for a Spaceship or a CelestialBody
    /// T1: The type of the argument 'body': either 'Spaceship' or 'CelestialBody'
    /// T2: The type of the body Settings variable: either 'SpaceshipSettings' or 'CelestialBodySettings'
    /// </summary>
    /// <param name="body">The object of type T1: either 'Spaceship' or 'CelestialBody'</param>
    public static void InitializeOrbit<T1, T2>(UnityEngine.Object body)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        if(body is T1)
        {
            T1 castBody = CastObjectToType<T1>(body); // Spaceship or CelestialBody
            T2 settings = GetObjectSettings<T2>(body); // SpaceshipSettings or CelestialBodySettings
            castBody.orbit = new Orbit(castBody.orbitalParams, castBody.orbitedBody.GetComponent<CelestialBody>(), castBody._gameObject);
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
    public static void InitializeBodyPosition<T1, T2>(UnityEngine.Object body)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        if(body is T1)
        {
            T1 castBody = CastObjectToType<T1>(body); // Spaceship or CelestialBody
            T2 settings = GetObjectSettings<T2>(body); // SpaceshipSettings or CelestialBodySettings

            Vector3d bodyRelatedPos = Orbit.GetWorldPositionFromOrbit(castBody.orbit, OrbitalParams.bodyPositionType.nu);
            castBody.realPosition = UsefulFunctions.AlignPositionVecWithParentPos(bodyRelatedPos, castBody.orbitedBody.transform.position);
            castBody._gameObject.transform.position = (Vector3)castBody.realPosition;
        }
    }

    /// <summary>
    /// Initialize LineRenderers of the direction vectors, for an orbit
    /// T1: The type of the argument 'body': either 'Spaceship' or 'CelestialBody'
    /// T2: The type of the body Settings variable: either 'SpaceshipSettings' or 'CelestialBodySettings'
    /// </summary>
    /// <param name="body">The object of type T1: either 'Spaceship' or 'CelestialBody'</param>
    public static void InitializeDirVecLineRenderers<T1, T2>(UnityEngine.Object body)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        if(body is T1)
        {
            T1 castBody = CastObjectToType<T1>(body); // Spaceship or CelestialBody
            T2 settings = GetObjectSettings<T2>(body); // SpaceshipSettings or CelestialBodySettings

            float lineLength = 10f; // Default length for a spaceship orbiting a planet
            if(castBody.orbit.orbitingGO.tag == UniverseRunner.goTags.Planet.ToString()) {
                lineLength = 100f;
            }
            if(castBody.orbitalParams.drawDirections)
            {
                foreach(OrbitalParams.typeOfVectorDir vectorDir in Enum.GetValues(typeof(OrbitalParams.typeOfVectorDir)))
                {
                    if (castBody.orbitalParams.selectedVectorsDir.HasFlag(vectorDir))
                    {
                        if(vectorDir == OrbitalParams.typeOfVectorDir.radialVec || vectorDir == OrbitalParams.typeOfVectorDir.tangentialVec
                        || vectorDir == OrbitalParams.typeOfVectorDir.velocityVec || vectorDir == OrbitalParams.typeOfVectorDir.accelerationVec)
                        {
                            castBody.orbit.DrawDirection(vectorDir, lineLength, 5_000f, castBody._gameObject.transform.position);
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



    public static void GravitationalUpdate<T1 ,T2>(CelestialBody pullingBody, T1 orbitingBody)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        if(orbitingBody is T1 && pullingBody != null)
        {
            T2 settings = GetObjectSettings<T1, T2>(orbitingBody);

            ComputeGravitationalAcc<T1, T2>(pullingBody, orbitingBody, settings);
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
    public static void ComputeGravitationalAcc<T1, T2>(CelestialBody pullingBody, T1 orbitingBody, T2 settings)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        Vector3d pullinBodyPos = new Vector3d(pullingBody.transform.position);
        Transform castOrbitingBodyTr = orbitingBody._gameObject.transform; // Spaceship Tr or CelestialBody Tr

        Vector3d r = new Vector3d(castOrbitingBodyTr.position) - pullinBodyPos;
        double scalingFactor = UniCsts.u2au * UniCsts.au2km; // km, for planets
        if(orbitingBody.orbitalParams.orbParamsUnits == OrbitalParams.orbitalParamsUnits.km_degree)
        {
            scalingFactor = UniCsts.u2pl; // km, for spaceships
        }

        r *= scalingFactor; // km
        double dstPow3 = Mathd.Pow(r.magnitude, 3); // km^3
        Vector3d acc =  - Mathd.Pow(10,7) * pullingBody.settings.mu * r / dstPow3; // m.s-2

        if(!Vector3d.IsValid(acc) || UsefulFunctions.DoublesAreEqual(dstPow3, 0d))
        {
            Debug.Log("Acc is not valid or distance between the pulling body and the target body is null");
            orbitingBody.orbitedBodyRelativeAcc = Vector3d.positiveInfinity;
        }
        else {
            orbitingBody.orbitedBodyRelativeAcc = acc;
        }
    }

    public static void ComputeUpdatedVelocity<T1, T2>(T1 orbitingBody, T2 settings)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        orbitingBody.orbitedBodyRelativeVelIncr = Time.fixedDeltaTime * orbitingBody.orbitedBodyRelativeAcc;
        orbitingBody.orbitedBodyRelativeVel += orbitingBody.orbitedBodyRelativeVelIncr;
    }

    public static void ComputeUpdatedPosition<T1, T2>(T1 castBody, T2 settings)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        double scaleFact = UniCsts.km2au * UniCsts.au2u; // Default for planets and stars
        if(castBody._gameObject.tag == UniverseRunner.goTags.Spaceship.ToString()) {
            scaleFact = UniCsts.pl2u;
        }
        //POSSIBLE ERROR HERE WITH THE TWO DIFFERENT SCALES

        Vector3d updatedPos = Time.fixedDeltaTime * UniCsts.m2km * scaleFact * castBody.orbitedBodyRelativeVel;
        castBody.realPosition += updatedPos;
    }

    public static void ApplyRigbidbodyPosUpdate<T1, T2>(T1 castBody, T2 settings)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        Rigidbody rb = castBody._gameObject.GetComponent<Rigidbody>();
        rb.MovePosition((Vector3)castBody.realPosition);
    }
}