using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;

[DisallowMultipleComponent, System.Serializable]
public class UniverseRunner : MonoBehaviour
{
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
        InitializePhysicsTime();
        flyingObjInst = new FlyingObj(this);
        universeOffset = new Vector3(0f, 0f, 0f);
        physicsObjGO = UsefulFunctions.CreateAssignGameObject(folderNames.PhysicsObjs.ToString());
        
        physicsObjArray = new List<Transform>();
        physicsRigidbodies = new List<Rigidbody>();

        InitFolders();
        FillFolders();
    }

    private void InitializePhysicsTime() {
        simEnv = SimulationEnvSaveData.LoadObjectFromJSON(Application.persistentDataPath + Filepaths.simulation_settings);

        if(simEnv.useTargetFrameRate) {
            Application.targetFrameRate = simEnv.targetFrameRate;
        }
        Time.fixedDeltaTime = 1f/simEnv.physicsUpdateRate;
        Time.timeScale = simEnv.timeScale;
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
            //InitializeCelestialSphereCollider(star);

            AddGameObjectToPhysicsFolders(star, starFolder);
        }

        foreach(GameObject planet in GameObject.FindGameObjectsWithTag(goTags.Planet.ToString()))
        {
            // CelestialBodies and Spaceships must have done their Awake() at this point
            InitializeRigidbody(planet, 1000f);
            //InitializeCelestialSphereCollider(planet);

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
        rb.mass = rbMass;
        rb.angularDrag = 0f;
        rb.drag = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        if(UsefulFunctions.GoTagAndStringAreEqual(goTags.Spaceship, physicGameObject.tag))
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        else {
            rb.interpolation = RigidbodyInterpolation.None;
        }
        
        rb.constraints = RigidbodyConstraints.None;
        rb.detectCollisions = true;
        rb.isKinematic = false;
        rb.useGravity = false;
    }

    private void AddGameObjectToPhysicsFolders(GameObject gameObjectToAdd, GameObject parentFolder)
    {
        // Add the GameObject to all of the physics list (list of Transforms, Rigidbody)
        physicsRigidbodies.Add(gameObjectToAdd.GetComponent<Rigidbody>());
        physicsObjArray.Add(gameObjectToAdd.transform);
        UsefulFunctions.parentObj(gameObjectToAdd, parentFolder);
    }

    private void InitializeCelestialSphereCollider(GameObject bodyGO)
    {
       // Need to find a smart way to create colliders for the celestialBodies
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
                    UniCsts.planetsDict[starBody.settings.chosenPredifinedPlanet][CelestialBodyParamsBase.planetaryParams.radius.ToString()] = 8_000d;
                    UniCsts.planetsDict[starBody.settings.chosenPredifinedPlanet][CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()] = 8_000d;
                    starBody.AwakeCelestialBody(UniCsts.planetsDict[starBody.settings.chosenPredifinedPlanet]);
                    break;

                case goTags.Planet:
                    CelestialBody celestBody = obj.GetComponent<CelestialBody>();
                    if(GameObject.FindGameObjectsWithTag(goTags.Star.ToString()).Length > 0)
                    {
                        celestBody.AwakeCelestialBody(UniCsts.planetsDict[celestBody.settings.chosenPredifinedPlanet]);
                        flyingObjInst.InitializeFlyingObj<CelestialBody, CelestialBodySettings>(celestBody);
                    }
                    else {
                        celestBody.transform.position = Vector3.zero;
                        celestBody.orbitedBodyRelativeVel = Vector3d.zero;
                    }
                    break;
                
                case goTags.Spaceship:
                    Spaceship ship = obj.GetComponent<Spaceship>();
                    flyingObjInst.InitializeFlyingObj<Spaceship, SpaceshipSettings>(ship);
                    break;
            }
        }
    }

    void FixedUpdate()
    {
        if(simEnv.simulateGravity)
        {
            flyingObjInst.GravitationalStep();
        }
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
    }


}