using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using System.IO;

[DisallowMultipleComponent, System.Serializable]
public class UniverseRunner : MonoBehaviour
{
    [HideInInspector] public bool simulationEnvFoldout=true; // For universeRunner custom editor
    //======
    [HideInInspector] public SimulationEnv simEnv;
    [HideInInspector] public FlyingObj flyingObjInst;

    public Spaceship activeSpaceship;
    public GameObject playerCamera; // Camera attached to the debugging spacecraft

    [HideInInspector] public enum folderNames { PhysicsObjs, Stars, Planets, Spaceships, Orbits };
    [HideInInspector] public enum goTags { Star, Planet, Spaceship, Orbit };

    [HideInInspector] public GameObject physicsObjGO; //Parent GameObject of all Physics Objects
    [HideInInspector] public GameObject planetsFolder; // GameObject containing every orbiting Planets
    [HideInInspector] public GameObject starFolder; // GameObject containing every Star(s)
    [HideInInspector] public GameObject spaceshipFolder; // GameObject containing every spaceship/spacecraft
    [HideInInspector] public GameObject orbitFolder; // GameObject containing every orbits, both drawn and only computed orbits

    public List<Rigidbody> physicsRigidbodies;
    public List<Transform> physicsObjArray;

    public Vector3 universeOffset; // Keeping track of the offset of the whole universe
    //=========================================
    void Awake()
    {
        InitializeSimulationEnv();
        flyingObjInst = new FlyingObj(this);
        universeOffset = new Vector3(0f, 0f, 0f);
        physicsObjGO = UsefulFunctions.CreateAssignGameObject(folderNames.PhysicsObjs.ToString());
        
        physicsObjArray = new List<Transform>();
        physicsRigidbodies = new List<Rigidbody>();

        InitFolders();
        FillFolders();
    }

    private void InitializeSimulationEnv() {
        if(simEnv == null) {
            simEnv = ScriptableObject.CreateInstance<SimulationEnv>();
        }
        string simEnvFilepath = Application.persistentDataPath + Filepaths.simulation_settings;
        JsonUtility.FromJsonOverwrite(File.ReadAllText(simEnvFilepath), simEnv);
        
        if(simEnv.useTargetFrameRate.value) {
            Application.targetFrameRate = simEnv.targetFrameRate.value;
        }
        Time.fixedDeltaTime = 1f/simEnv.physicsUpdateRate.value;
        Time.timeScale = simEnv.timeScale.value;
        
        if(GameObject.Find("missionTimer") != null)
            simEnv.missionTimer = GameObject.Find("missionTimer").GetComponent<StopWatch>();
    }

    private void InitFolders()
    {
        starFolder = UsefulFunctions.CreateAssignGameObject(folderNames.Stars.ToString(), physicsObjGO);
        planetsFolder = UsefulFunctions.CreateAssignGameObject(folderNames.Planets.ToString(), physicsObjGO);
        spaceshipFolder = UsefulFunctions.CreateAssignGameObject(folderNames.Spaceships.ToString(), physicsObjGO);
        orbitFolder = UsefulFunctions.CreateAssignGameObject(folderNames.Orbits.ToString());
    }

    private void FillFolders()
    {
        // FILLING ORDER: Stars, Planets, Spaceships
        foreach(GameObject star in GameObject.FindGameObjectsWithTag(goTags.Star.ToString()))
        {
            // CelestialBodies and Spaceships must have done their Awake() at this point
            InitializeRigidbody(star, 100000f);
            AddGameObjectToPhysicsFolders(star, starFolder);
        }

        foreach(GameObject planet in GameObject.FindGameObjectsWithTag(goTags.Planet.ToString()))
        {
            // CelestialBodies and Spaceships must have done their Awake() at this point
            InitializeRigidbody(planet, 1000f);
            AddGameObjectToPhysicsFolders(planet, planetsFolder);
        }

        foreach(GameObject vessel in GameObject.FindGameObjectsWithTag(goTags.Spaceship.ToString()))
        {
            InitializeRigidbody(vessel, 0.01f);
            AddGameObjectToPhysicsFolders(vessel, spaceshipFolder);
        }
    }

    private void InitializeRigidbody(GameObject physicGameObject, float rbMass)
    {
        // Initialize everything of the rigidbody except its mass as its need to access the object Settings (done in 'Start' of the UniverseRunner)
        Rigidbody rb = (Rigidbody) UsefulFunctions.CreateAssignComponent(typeof(Rigidbody), physicGameObject);
        rb.drag = 0f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        if(physicGameObject.CompareTag(goTags.Star.ToString()) || physicGameObject.CompareTag(goTags.Planet.ToString())) {
            rb.angularDrag = 0.05f;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }
        else {
            rb.angularDrag = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        rb.constraints = RigidbodyConstraints.None;
        rb.isKinematic = false;

        if(physicGameObject.CompareTag(goTags.Star.ToString()) || physicGameObject.CompareTag(goTags.Planet.ToString()))
            rb.mass = 1e9f;
        else
            rb.mass = 10f;
        rb.useGravity = false;
        rb.detectCollisions = true;
    }

    public void AddGameObjectToPhysicsFolders(GameObject gameObjectToAdd, GameObject parentFolder)
    {
        // Add the GameObject to all of the physics list (list of Transforms, Rigidbody)
        physicsRigidbodies.Add(gameObjectToAdd.GetComponent<Rigidbody>());
        physicsObjArray.Add(gameObjectToAdd.transform);
        UsefulFunctions.parentObj(gameObjectToAdd, parentFolder);
    }
    //=========================================
    void Start()
    {
        foreach(Transform obj in physicsObjArray)
        {
            switch(UsefulFunctions.CastStringToGoTags(obj.tag))
            {
                case goTags.Star:
                    obj.position = Vector3.zero; // Position the Sun at the center of the Universe
                    CelestialBody starBody = obj.GetComponent<CelestialBody>();
                    // Modifying the radius of the sun, else it will be too large to be rendered
                    //UniCsts.planetsDict[starBody.settings.chosenPredifinedPlanet][CelestialBodyParamsBase.planetaryParams.radius.ToString()].value = 8_000d;
                    //UniCsts.planetsDict[starBody.settings.chosenPredifinedPlanet][CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()].value = 8_000d;
                    starBody.AwakeCelestialBody(UniCsts.planetsDict[starBody.settings.chosenPredifinedPlanet]);
                    break;

                case goTags.Planet:
                    CelestialBody celestBody = obj.GetComponent<CelestialBody>();
                    if(GameObject.FindGameObjectsWithTag(goTags.Star.ToString()).Length > 0)
                    {
                        celestBody.AwakeCelestialBody(UniCsts.planetsDict[celestBody.settings.chosenPredifinedPlanet]);
                        flyingObjInst.InitializeFlyingObj<CelestialBody, CelestialBodySettings>(celestBody, false);
                    }
                    else {
                        celestBody.transform.position = Vector3.zero;
                        celestBody.orbitedBodyRelativeVel = Vector3d.zero;
                    }
                    break;
                
                case goTags.Spaceship:
                    Spaceship ship = obj.GetComponent<Spaceship>();
                    flyingObjInst.InitializeFlyingObj<Spaceship, SpaceshipSettings>(ship, true);
                    break;
            }
        }
        flyingObjInst.InitGravitationalPullLists();
        //==============================
        if(simEnv.missionTimer != null)
            simEnv.missionTimer.Start_Stopwatch();
    }

    void FixedUpdate()
    {
        if(simEnv.simulateGravity.value)
        {
            flyingObjInst.GravitationalStep();
        }
        //Debug.Log(activeSpaceship.GetOrbitedBodyAtmospherePressure().ConvertTo(Units.pressure.atm));
        updateFloatingOrigin();
    }

    private void updateFloatingOrigin()
    {   
        Vector3 originOffset = playerCamera.transform.position;
        float dstFromOrigin = originOffset.magnitude;
        if(dstFromOrigin > UniCsts.dstThreshold)
        {
            foreach(Transform t in physicsObjArray)
            {
                t.position -= originOffset; // Offset every Star, Planet and Spaceship
                
                // Also update their 'realPosition' variable, as it's the variable used to compute gravitational pull at each timestep
                switch(UsefulFunctions.CastStringToGoTags(t.tag))
                {
                    case goTags.Spaceship:
                        Spaceship ship = t.gameObject.GetComponent<Spaceship>();
                        ship.realPosition -= originOffset;
                        break;
                    default:
                        CelestialBody celestBody = t.gameObject.GetComponent<CelestialBody>();
                        celestBody.realPosition -= originOffset;
                        break;
                }
            }
            universeOffset += originOffset;
        }
    }

    void LateUpdate()
    {
        UpdateOrbitLineRenderers();
    }

    private void UpdateOrbitLineRenderers()
    {
        foreach(Transform obj in physicsObjArray)
        {
            switch(UsefulFunctions.CastStringToGoTags(obj.tag))
            {
                case goTags.Spaceship:
                    Spaceship ship = obj.GetComponent<Spaceship>();
                    if(ship.orbit != null)
                    {
                        ship.orbit.UpdateLineRendererPos();
                    }
                    //if(ship.predictor != null && ship.predictor.predictedOrbit != null)
                        //ship.predictor.predictedOrbit.UpdateLineRendererPos();
                    break;
                
                case goTags.Planet:
                    CelestialBody celestBody = obj.GetComponent<CelestialBody>();
                    if(celestBody.orbit != null)
                    {
                        celestBody.orbit.UpdateLineRendererPos();
                    }
                    break;
            }
        }

        foreach(GameObject obj in GameObject.FindGameObjectsWithTag(goTags.Orbit.ToString()))
        {
            float dist = Vector3.Distance(obj.transform.position, playerCamera.transform.position);///1_000f;
            LineRenderer orbitLR = obj.GetComponent<LineRenderer>();
            orbitLR.startWidth *= dist;
            orbitLR.endWidth *= dist;
        }
    }

}