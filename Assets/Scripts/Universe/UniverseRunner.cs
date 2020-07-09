using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;

[DisallowMultipleComponent]
public class UniverseRunner : MonoBehaviour
{
    [HideInInspector] public SimulationEnv simEnv;
    //public SimulationEnv simulationEnv;
    public GameObject playerCamera; // Camera attached to the debugging spacecraft

    [HideInInspector] public enum folderNames { PhysicsObjs, Stars, Planets, Spaceships, Orbits };
    [HideInInspector] public enum goTags { Star, Planet, Spaceship };

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
            InitializeRigidbody(star);
            InitializeCelestialSphereCollider(star);

            AddGameObjectToPhysicsFolders(star, starFolder);
        }
        foreach(GameObject planet in GameObject.FindGameObjectsWithTag(goTags.Planet.ToString()))
        {
            // CelestialBodies and Spaceships must have done their Awake() at this point
            InitializeRigidbody(planet);
            InitializeCelestialSphereCollider(planet);

            AddGameObjectToPhysicsFolders(planet, planetsFolder);
        }
        foreach(GameObject vessel in GameObject.FindGameObjectsWithTag(goTags.Spaceship.ToString()))
        {
            InitializeRigidbody(vessel);
            AddGameObjectToPhysicsFolders(vessel, spaceshipFolder);
        }             
    }

    private void InitializeRigidbody(GameObject physicGameObject)
    {
        // Initialize everything of the rigidbody except its mass as its need to access the object Settings (done in 'Start' of the UniverseRunner)
        Rigidbody rb = (Rigidbody) UsefulFunctions.CreateAssignComponent(typeof(Rigidbody), physicGameObject);
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
        CelestialBody celestialBody = bodyGO.GetComponent<CelestialBody>();
        SphereCollider sp_c = (SphereCollider) UsefulFunctions.CreateAssignComponent(typeof(SphereCollider), bodyGO);
        sp_c.radius = (float) celestialBody.settings.radiusU;
        PhysicMaterial sp_c_Material = new PhysicMaterial();
        sp_c_Material.bounciness = 0f;
        sp_c_Material.dynamicFriction = 0f;
        sp_c_Material.staticFriction = 0f;
        sp_c_Material.frictionCombine = PhysicMaterialCombine.Average;
        sp_c_Material.bounceCombine = PhysicMaterialCombine.Average;
        sp_c.material = sp_c_Material;
    }
    //=========================================

    void Start()
    {
        //EarlyInitPlanet(); // Init every planets at (0;0;0)
        foreach(Transform obj in physicsObjArray)
        {
            Vector3d tangentialVec;
            double orbitalSpeed;

            switch(UsefulFunctions.CastStringToGoTags(obj.tag))
            {
                

                case goTags.Planet:
                    obj.position = Vector3.zero;
                    /*CelestialBody body = obj.GetComponent<CelestialBody>();
                    body.AssignRefDictOrbitalParams(UniCsts.planetsDict[body.settings.chosenPredifinedPlanet]);
                    FlyingObj.InitializeOrbit<CelestialBody, CelestialBodySettings>(body);
                    FlyingObj.InitializeBodyPosition<CelestialBody, CelestialBodySettings>(body);
                    FlyingObj.InitializeDirVecLineRenderers<CelestialBody, CelestialBodySettings>(body);

                    tangentialVec = body.orbit.ComputeDirectionVector(OrbitalParams.typeOfVectorDir.tangentialVec);
                    orbitalSpeed = body.orbit.GetOrbitalSpeedFromOrbit();
                    body.orbitedBodyRelativeVel = tangentialVec * orbitalSpeed;*/
                    break;
                
                case goTags.Spaceship:
                    Spaceship ship = obj.GetComponent<Spaceship>();
                    FlyingObj.InitializeOrbit<Spaceship, SpaceshipSettings>(ship);
                    FlyingObj.InitializeBodyPosition<Spaceship, SpaceshipSettings>(ship);
                    FlyingObj.InitializeDirVecLineRenderers<Spaceship, SpaceshipSettings>(ship);

                    tangentialVec = ship.orbit.ComputeDirectionVector(OrbitalParams.typeOfVectorDir.tangentialVec);
                    orbitalSpeed = ship.orbit.GetOrbitalSpeedFromOrbit();
                    ship.orbitedBodyRelativeVel = tangentialVec * orbitalSpeed;
                    break;
            }
        }
    }

    private void EarlyInitPlanet()
    {
        GameObject[] goList = GameObject.FindGameObjectsWithTag(goTags.Planet.ToString());
        foreach(GameObject go in goList)
        {
            go.transform.position = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        if(simEnv.simulateGravity)
        {
            GravitationalStep();
        }
    }

    private void GravitationalStep()
    {
        foreach(Transform obj in physicsObjArray)
        {
            ComputeNewPosition(obj, obj.tag);
        }

        foreach(Transform obj in physicsObjArray)
        {
            ApplyNewPosition(obj, obj.tag);
        }
    }

    private void ComputeNewPosition(Transform obj, string objTag)
    {
        CelestialBody orbitedBody;
        switch(UsefulFunctions.CastStringToGoTags(objTag))
        {
            case goTags.Planet:
                //do nothing for now
                break;
            
            case goTags.Spaceship:
                Spaceship ship = obj.GetComponent<Spaceship>();
                orbitedBody = ship.orbitedBody.GetComponent<CelestialBody>();
                FlyingObj.GravitationalUpdate<Spaceship, SpaceshipSettings>(orbitedBody, ship);
                break;
        }
    }

    private void ApplyNewPosition(Transform obj, string objTag)
    {
        switch(UsefulFunctions.CastStringToGoTags(objTag))
        {
            case goTags.Planet:
                //do nothing for now
                break;
            
            case goTags.Spaceship:
                Spaceship ship = obj.GetComponent<Spaceship>();
                FlyingObj.ApplyRigbidbodyPosUpdate<Spaceship, SpaceshipSettings>(ship, ship.settings);
                break;
        }
    }
}