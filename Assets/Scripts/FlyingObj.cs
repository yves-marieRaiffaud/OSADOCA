using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Mathd_Lib;
using System.Collections.Generic;
using Funcs = UsefulFunctions;
using Verse = UniverseRunner;

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
        if(body is Spaceship) {
            Spaceship spaceship = (Spaceship)body;
            return (T)(dynamic)spaceship.settings;
        }
        else {
            CelestialBody celestialBody = (CelestialBody)body;
            return (T)(dynamic)celestialBody.settings;
        }
    }

    public static T2 GetObjectSettings<T1, T2>(T1 body)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        // 'T1' is either Spaceship or CelestialBody
        // 'T2' is either SpaceshipSettings or CelestialBodySettings
        if(body is Spaceship) {
            Spaceship spaceship = (Spaceship)(dynamic)body;
            return (T2)(dynamic)spaceship.settings;
        }
        else {
            CelestialBody celestialBody = (CelestialBody)(dynamic)body;
            return (T2)(dynamic)celestialBody.settings;
        }
    }

    public static T CastObjectToType<T>(UnityEngine.Object body)
    {
        // T is either Spaceship or CelestialBody
        if(body is Spaceship) {
            Spaceship spaceship = (Spaceship)body;
            return (T)(dynamic)spaceship;
        }
        else {
            CelestialBody celestialBody = (CelestialBody)body;
            return (T)(dynamic)celestialBody;
        }
    }
    //===============================================================================
    //===============================================================================
    //===============================================================================
    //===============================================================================
    //===============================================================================
    public void InitializeFlyingObj<T1, T2>(UnityEngine.Object body, bool initOrbitalPredictor)
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
                //InitializeOrbitalPredictor<T1, T2>(body, initOrbitalPredictor);
                return;
            }
        }

        InitializeOrbit<T1, T2>(body);
        //InitializeOrbitalPredictor<T1, T2>(body, initOrbitalPredictor);
        InitializeBodyPosition<T1, T2>(body);
        InitializeOrbitalSpeed<T1, T2>(body);
        InitializeDirVecLineRenderers<T1, T2>(body);

        // Init Axial Tilt for CelestialBody
        UniverseRunner.goTags starTag = UniverseRunner.goTags.Star;
        UniverseRunner.goTags planetTag = UniverseRunner.goTags.Planet;
        if(UsefulFunctions.StringIsOneOfTheTwoTags(starTag, planetTag, castBody._gameObject.tag)) {
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
            if(castBody.orbitalParams.orbitedBody == null && !castBody.orbitalParams.orbitedBodyName.Equals("None") && !castBody.orbitalParams.orbitedBodyName.Equals(""))
                castBody.orbitalParams.orbitedBody = GameObject.Find(castBody.orbitalParams.orbitedBodyName).GetComponent<CelestialBody>();
            castBody.orbit = new Orbit(castBody.orbitalParams, castBody.orbitalParams.orbitedBody, castBody._gameObject);
        }
        else {
            Debug.LogError("Specified UnityEngine.Object is not of the specified generic type.");
        }
    }

    public void InitializeOrbitalPredictor<T1, T2>(UnityEngine.Object body, bool initOrbitalPredictor)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        if(body is T1)
        {
            T1 castBody = CastObjectToType<T1>(body); // Spaceship or CelestialBody
            T2 settings = GetObjectSettings<T2>(body); // SpaceshipSettings or CelestialBodySettings
            if(initOrbitalPredictor)
            {
                //Debug.Log("Initializing OrbitalPredictor for " + castBody._gameObject.name);
                castBody.predictor = new OrbitalPredictor(castBody, castBody.orbitalParams.orbitedBody, castBody.orbit);
                //castBody.predictor.DebugLog_Predictor();
            }
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
                if(castBody.orbitalParams.orbitedBody == null && !castBody.orbitalParams.orbitedBodyName.Equals("None") && !castBody.orbitalParams.orbitedBodyName.Equals(""))
                    castBody.orbitalParams.orbitedBody = GameObject.Find(castBody.orbitalParams.orbitedBodyName).GetComponent<CelestialBody>();

                SpaceshipSettings shipSettings = (SpaceshipSettings)(dynamic)settings;
                Spaceship ship = (Spaceship)(dynamic)castBody;
                if(shipSettings.startFromGround)
                {
                    // A spaceship with planetary surface init
                    double latitude = shipSettings.groundStartLatLong.x;
                    double longitude = shipSettings.groundStartLatLong.y;
                    bodyRelatedPos = castBody.orbitalParams.orbitedBody.GetWorldPositionFromGroundStart(latitude, longitude);
                    //=================
                    // Because we start from ground, we must also init the ship's rigidbody velocity
                    // Init the ship's rb velocity as the planet absolute velocity (the 'orbitedBodyRelativeVel' is 0 m/s)
                    // + the eastward boost
                    Vector3d speedOfOrbitedBody = Vector3d.zero;
                    if(!castBody.orbitalParams.orbitedBodyName.Equals("None")) {
                        CelestialBody orbitedBody = castBody.orbitalParams.orbitedBody;
                        speedOfOrbitedBody = orbitedBody.orbitedBodyRelativeVel; // in m.s
                    }
                    UniCsts.planets planet = UsefulFunctions.CastStringTo_Unicsts_Planets(castBody.orbitalParams.orbitedBodyName);
                    (LaunchPad,bool) lpOut = LaunchPad.GetLaunchPadFromName(shipSettings.startLaunchPadName, planet, true);
                    LaunchPad startLP = lpOut.Item1 != null ? lpOut.Item1 : castBody.orbitalParams.orbitedBody.GetDefaultLaunchPad();
                    //=====
                    Rigidbody rb = castBody._gameObject.GetComponent<Rigidbody>();
                    //===============================================================
                    rb.constraints = RigidbodyConstraints.FreezeRotation;//==========
                    //===============================================================
                    Vector3d eastwardBoosDir = startLP.eastwardBoost * ship.GetEasternDirection(longitude); // in m/s
                    castBody.orbitedBodyRelativeVel = eastwardBoosDir;
                    //====
                    Vector3d absoluteScaledVelocity = speedOfOrbitedBody*UniCsts.m2km2au2u + eastwardBoosDir*UniCsts.pl2u;
                    rb.velocity = (Vector3)absoluteScaledVelocity;
                    //=================
                    // Init the rotation of the spaceship to make it stand up on the ground
                    Quaternion planetRot = ship.orbitalParams.orbitedBody.transform.rotation;
                    Vector3 spherePos = planetRot*(Vector3)LaunchPad.LatitudeLongitude_to_3DWorldUNITPoint(0d-90d, longitude-180d);
                    ship.transform.rotation = Quaternion.FromToRotation(ship.transform.up, spherePos)*ship.transform.rotation;
                }
                else {
                    // A spaceship with in orbit init
                    //bodyRelatedPos = Orbit.GetWorldPositionFromOrbit(castBody.orbit, OrbitalTypes.bodyPositionType.nu);
                    bodyRelatedPos = Orbit.GetWorldPositionFromLineRendererOrbit(castBody.orbit);
                }
            }
            else {
                // A celestialBody with in orbit init
                bodyRelatedPos = Orbit.GetWorldPositionFromOrbit(castBody.orbit);
            }
            //===================
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
        if(castBody._gameObject.name.Equals("Diamant_A"))
            Debug.Log("orbitalSpeed = " + orbitalSpeed + " m/s");
        castBody.orbitedBodyRelativeVel = tangentialVec * orbitalSpeed; // in m.s

        Vector3 speedOfOrbitedBody = Vector3.zero;
        if(!castBody.orbitalParams.orbitedBodyName.Equals("None")) {
            CelestialBody orbitedBody = castBody.orbitalParams.orbitedBody;
            speedOfOrbitedBody = orbitedBody.GetComponent<Rigidbody>().velocity;
        }
        // Init orbital speed of the Rigidbody
        double scaleFactor = castBody.distanceScaleFactor;

        Rigidbody rb = castBody._gameObject.GetComponent<Rigidbody>();
        rb.velocity = (Vector3)(castBody.orbitedBodyRelativeVel*scaleFactor + speedOfOrbitedBody);
        castBody.rbVelocity = new Vector3d(rb.velocity);

        if(body is Spaceship)
            InitializeSpaceshipRotation<T1>(castBody, (Vector3)tangentialVec);
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
            ComputeNewPosition(obj, obj.tag);

        // Once everything has been computed, apply the new accelerations at every objects
        foreach(Transform obj in universe.physicsObjArray)
            ApplyNewPosition(obj, obj.tag);
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
                // As the body RigidBody is kinematic, we compute the updated position to then use: 'rb.MovePosition()'
                CelestialBody celestBody = obj.GetComponent<CelestialBody>();
                if(celestBody.orbitalParams.orbitedBody != null) {
                    orbitedBody = celestBody.orbitalParams.orbitedBody.GetComponent<CelestialBody>();
                    GravitationalUpdate<CelestialBody, CelestialBodySettings>(orbitedBody, celestBody);
                }
                break;
            
            case UniverseRunner.goTags.Spaceship:
                // The RigidBody is not Kinematic, we compute the acceleration to then use: 'rb.AddForce(mode=Acceleration)' 
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
            case UniverseRunner.goTags.Star:
                CelestialBody starBody = obj.GetComponent<CelestialBody>();
                ApplyRigbidbodyAccUpdate<CelestialBody, CelestialBodySettings>(starBody, starBody.settings);
                break;

            case UniverseRunner.goTags.Planet:
                CelestialBody celestBody = obj.GetComponent<CelestialBody>();
                ApplyRigbidbodyAccUpdate<CelestialBody, CelestialBodySettings>(celestBody, celestBody.settings);
                break;
            
            case UniverseRunner.goTags.Spaceship:
                Spaceship ship = obj.GetComponent<Spaceship>();
                ApplyRigbidbodyAccUpdate<Spaceship, SpaceshipSettings>(ship, ship.settings);
                // Update the orbital predictor if it has been defined for the object
                if(ship.predictor != null)
                    ship.predictor.smartPredictor();
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
        // 'pullingBody' is the CelestialBody with the strongest attraction force on the orbitingBody (Spaceship or CelestialBody)
        if(orbitingBody is T1 && pullingBody != null)
        {
            T2 settings = GetObjectSettings<T1, T2>(orbitingBody);
            if(universe.simEnv.useNBodySimulation.value)
                Debug.LogError("ERROR");
                //ComputeGravitationalAccNBODY<T1, T2>(orbitingBody, settings, true);
            else
                //Kepler.GravitationalAcc<T1>(pullingBody, orbitingBody, orbitingBody.Get_RadialVec()*1000d, true);
                UpdateVelocityPos_RK4<T1>(orbitingBody);
        }
    }
    //=====================================================
    //=====================================================
    public void InitGravitationalPullLists()
    {
        // Looping through every object in the 'universeRunner.physicsObjArray'
        for(int i = 0; i < universe.physicsObjArray.Count; i++)
        {
            if(Funcs.GoTagAndStringAreEqual(Verse.goTags.Planet, universe.physicsObjArray[i].tag))
            {
                CelestialBody celestBody = universe.physicsObjArray[i].GetComponent<CelestialBody>();
                GetSortedGravPullListForBody<CelestialBody, CelestialBodySettings>(celestBody, celestBody.settings);
            }
            else if(Funcs.GoTagAndStringAreEqual(Verse.goTags.Spaceship, universe.physicsObjArray[i].tag))
            {
                Spaceship ship = universe.physicsObjArray[i].GetComponent<Spaceship>();
                GetSortedGravPullListForBody<Spaceship, SpaceshipSettings>(ship, ship.settings);
            }
        }
    }

    private void GetSortedGravPullListForBody<T1, T2>(T1 orbitingBody, T2 settings)
    where T1 : FlyingObjCommonParams where T2 : FlyingObjSettings
    {
        // Returns the array of length[universe.simEnv.NBODYSIM_NB_BODY.value] containing the CelestialBodies
        // from the one applying strongest gravitational pull to the one applying the lowest gravitational pull while in the NBODYSIM_NB_BODY value
        List<Transform> physicObjArr = universe.physicsObjArray;
        List<CelestialBodyPullForce> bodiesGravPull_ALL = new List<CelestialBodyPullForce>();
        //==================
        for(int i = 0; i < physicObjArr.Count; i++)
        {
            if(!Funcs.StringIsOneOfTheTwoTags(Verse.goTags.Star, Verse.goTags.Planet, physicObjArr[i].tag)) {
                // If the picked item is not a Planet CelestialBody, we skip this round as we won't be able to assign it as the orbitedBody
                continue;
            }
            if(physicObjArr[i].name.Equals(orbitingBody.orbitalParams.name)) { continue; }
            
            CelestialBody orbitedBody = physicObjArr[i].GetComponent<CelestialBody>();
            Vector3d gravForce = Kepler.GravitationalAcc<T1>(orbitedBody, orbitingBody, orbitingBody.Get_RadialVec()*UniCsts.km2m, false);
            bodiesGravPull_ALL.Add(new CelestialBodyPullForce(orbitedBody, gravForce));
        }
        
        // Sorting the list with index 0 as the weakest grav force (as magnitude)
        bodiesGravPull_ALL.Sort(delegate(CelestialBodyPullForce x, CelestialBodyPullForce y) {
            return x.gravForce.magnitude.CompareTo(y.gravForce.magnitude);
        });
        // Reversing the list to get index 0 as the strongest grav force magnitude
        bodiesGravPull_ALL.Reverse();
        //===============
        // Retaining only the 'NBODYSIM_NB_BODY' first values
        if(universe.simEnv.NBODYSIM_NB_BODY.value > bodiesGravPull_ALL.Count)
            Debug.LogWarning("WARNING ! The specified SimSetting 'NBODYSIM_NB_BODY' is partially applied as its value is greater than the total number of CelestialBodies in the universe. Thus, 'NBODYSIM_NB_BODY' has been automatically set to the greatest value possible : " + bodiesGravPull_ALL.Count + ".");
        int nbItemsToRetain = Mathf.Min(universe.simEnv.NBODYSIM_NB_BODY.value, bodiesGravPull_ALL.Count);

        if(orbitingBody.gravPullList.Length != nbItemsToRetain)
            orbitingBody.gravPullList = new CelestialBodyPullForce[nbItemsToRetain];
        
        for(int j = 0; j < nbItemsToRetain; j++)
            orbitingBody.gravPullList[j] = bodiesGravPull_ALL[j];
    }
    //=====================================================
    //=====================================================
    private Vector3d ComputeGravitationalAccNBODY<T1, T2>(T1 orbitingBody, T2 settings, bool saveAccToOrbitingBodyParam)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        UpdateBodyGravPullList<T1, T2>(orbitingBody, settings);
        
        Vector3d acc = new Vector3d();
        foreach(CelestialBodyPullForce item in orbitingBody.gravPullList)
            acc += item.gravForce;

        if(saveAccToOrbitingBodyParam)
            orbitingBody.orbitedBodyRelativeAcc = acc;
        return acc;
    }

    private void UpdateBodyGravPullList<T1, T2>(T1 orbitingBody, T2 settings)
    where T1 : FlyingObjCommonParams where T2 : FlyingObjSettings
    {
        for(int i = 0; i < orbitingBody.gravPullList.Length; i++)
        {
            Vector3d acc = Kepler.GravitationalAcc<T1>(orbitingBody.gravPullList[i].celestBody, orbitingBody, orbitingBody.Get_RadialVec()*UniCsts.km2m, false);
            orbitingBody.gravPullList[i].gravForce = acc;
        }
    }
    //===============================================================================
    //===============================================================================
    //===============================================================================
    public void UpdateVelocityPos_RK4<T1>(T1 orbitingBody)
    where T1: FlyingObjCommonParams
    {
        // Calling the 'rk4.Step()' in a separate Thread for performance
        Vector3d[] stepRes = Task.Factory.StartNew<Vector3d[]>(() => orbitingBody.rk4.Step()).Result;

        orbitingBody.orbitedBodyRelativeVel = stepRes[1];

        Vector3d speedOfOrbitedBody = Vector3d.zero;
        double orbitedBodySF = 0d;
        Vector3d OBBodyrealPos = Vector3d.zero;
        if(!orbitingBody.orbitalParams.orbitedBodyName.Equals("None")) {
            speedOfOrbitedBody = orbitingBody.orbitalParams.orbitedBody.orbitedBodyRelativeVel;
            orbitedBodySF = orbitingBody.orbitalParams.orbitedBody.distanceScaleFactor;
            OBBodyrealPos = orbitingBody.orbitalParams.orbitedBody.realPosition;
        }
        double scaleFactor = orbitingBody.distanceScaleFactor;
        orbitingBody.rbVelocity = orbitingBody.orbitedBodyRelativeVel*scaleFactor + speedOfOrbitedBody*orbitedBodySF;

        orbitingBody.realPosition = stepRes[0]*scaleFactor + OBBodyrealPos;
    }

    public void ApplyRigbidbodyAccUpdate<T1, T2>(T1 castBody, T2 settings)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        Rigidbody rb = castBody._gameObject.GetComponent<Rigidbody>();
        //rb.velocity = (Vector3)castBody.rbVelocity;
        rb.MovePosition((Vector3)castBody.realPosition);
    }
}