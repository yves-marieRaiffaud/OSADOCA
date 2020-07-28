using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;

[DisallowMultipleComponent]
public class UniverseRunner : MonoBehaviour
{
    [HideInInspector] public SimulationEnv simEnv;
    //public SimulationEnv simulationEnv;

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
        universeOffset = new Vector3(0f, 0f, 0f);
        physicsObjGO = UsefulFunctions.CreateAssignGameObject(folderNames.PhysicsObjs.ToString());
        
        physicsObjArray = new List<Transform>();
        physicsRigidbodies = new List<Rigidbody>();

        InitFolders();
        FillFolders();
    }

    private void InitializePhysicsTime() {
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
        rb.interpolation = RigidbodyInterpolation.Interpolate;
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
       //
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
                        FlyingObj.InitializeFlyingObj<CelestialBody, CelestialBodySettings>(celestBody);
                    }
                    else {
                        celestBody.transform.position = Vector3.zero;
                        celestBody.orbitedBodyRelativeVel = Vector3d.zero;
                    }
                    break;
                
                case goTags.Spaceship:
                    Spaceship ship = obj.GetComponent<Spaceship>();
                    FlyingObj.InitializeFlyingObj<Spaceship, SpaceshipSettings>(ship);
                    break;
            }
        }
    }

    void FixedUpdate()
    {
        if(simEnv.simulateGravity)
        {
            GravitationalStep();
        }
        updateFloatingOrigin();
    }

    private void GravitationalStep()
    {
        // First computing the acc, velocity and position updates for each Star, Planet or Spaceship 
        foreach(Transform obj in physicsObjArray)
        {
            ComputeNewPosition(obj, obj.tag);
        }

        // Once everything has been computed, apply the new position ot every objects
        foreach(Transform obj in physicsObjArray)
        {
            ApplyNewPosition(obj, obj.tag);
        }
    }

    private void ComputeNewPosition(Transform obj, string objTag)
    {
        // Compute acceleration, velocity and the new position of either a Planet or a Spaceship, due to gravitational pull
        CelestialBody orbitedBody;
        switch(UsefulFunctions.CastStringToGoTags(objTag))
        {
            case goTags.Star:
                //do nothing for now
                break;

            case goTags.Planet:
                CelestialBody celestBody = obj.GetComponent<CelestialBody>();
                if(celestBody.orbitalParams.orbitedBody != null)
                {
                    orbitedBody = celestBody.orbitalParams.orbitedBody.GetComponent<CelestialBody>();
                    FlyingObj.GravitationalUpdate<CelestialBody, CelestialBodySettings>(orbitedBody, celestBody);
                }
                break;
            
            case goTags.Spaceship:
                Spaceship ship = obj.GetComponent<Spaceship>();
                orbitedBody = ship.orbitalParams.orbitedBody.GetComponent<CelestialBody>();
                FlyingObj.GravitationalUpdate<Spaceship, SpaceshipSettings>(orbitedBody, ship);
                break;
        }
    }

    private void ApplyNewPosition(Transform obj, string objTag)
    {
        // Move the rigidbody of a Planet or a Spaceship to its new position due to gravitational pull
        switch(UsefulFunctions.CastStringToGoTags(objTag))
        {
            case goTags.Planet:
                CelestialBody celestBody = obj.GetComponent<CelestialBody>();
                FlyingObj.ApplyRigbidbodyAccUpdate<CelestialBody, CelestialBodySettings>(celestBody, celestBody.settings);
                break;
            
            case goTags.Spaceship:
                Spaceship ship = obj.GetComponent<Spaceship>();
                FlyingObj.ApplyRigbidbodyAccUpdate<Spaceship, SpaceshipSettings>(ship, ship.settings);
                break;
        }
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