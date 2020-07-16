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

    private void InitSunPointLight(GameObject starGO)
    {
        GameObject sunPointLightGO = UsefulFunctions.CreateAssignGameObject("sunPointLight", typeof(Light));
        UsefulFunctions.parentObj(sunPointLightGO, starGO);
        Light sunPointLight = sunPointLightGO.GetComponent<Light>();
        sunPointLight.type = LightType.Point;
        sunPointLight.range = Mathf.Pow(10, 5);
        sunPointLight.color = Color.white;
        sunPointLight.shadows = LightShadows.Soft;
        sunPointLight.cullingMask |= 1 << LayerMask.NameToLayer("Everything");
        sunPointLight.cullingMask &=  ~(1 << LayerMask.NameToLayer("Orbit"));
        sunPointLight.lightmapBakeType = LightmapBakeType.Baked;
        sunPointLight.intensity = 5f;
    }

    void Start()
    {
        foreach(Transform obj in physicsObjArray)
        {
            Vector3d tangentialVec;
            double orbitalSpeed;

            switch(UsefulFunctions.CastStringToGoTags(obj.tag))
            {
                case goTags.Star:
                    obj.position = Vector3.zero; // Position the Sun at the center of the Universe
                    CelestialBody starBody = obj.GetComponent<CelestialBody>();
                    starBody.AssignRefDictOrbitalParams(UniCsts.planetsDict[starBody.settings.chosenPredifinedPlanet]);
                    starBody.AwakeCelestialBody();
                    starBody.StartCelestialBody();
                    InitSunPointLight(obj.gameObject); 
                    break;

                case goTags.Planet:
                    CelestialBody celestBody = obj.GetComponent<CelestialBody>();
                    if(GameObject.FindGameObjectsWithTag(goTags.Star.ToString()).Length > 0)
                    {
                        celestBody.AssignRefDictOrbitalParams(UniCsts.planetsDict[celestBody.settings.chosenPredifinedPlanet]);
                        celestBody.AwakeCelestialBody();
                        celestBody.StartCelestialBody();
                        FlyingObj.InitializeOrbit<CelestialBody, CelestialBodySettings>(celestBody);
                        FlyingObj.InitializeBodyPosition<CelestialBody, CelestialBodySettings>(celestBody);
                        FlyingObj.InitializeDirVecLineRenderers<CelestialBody, CelestialBodySettings>(celestBody);
                        celestBody.InitializeAxialTilt();

                        tangentialVec = celestBody.orbit.ComputeDirectionVector(OrbitalParams.typeOfVectorDir.tangentialVec);
                        orbitalSpeed = celestBody.orbit.GetOrbitalSpeedFromOrbit();
                        celestBody.orbitedBodyRelativeVel = tangentialVec * orbitalSpeed;
                        //celestBody.InitializeOrbitalPredictor();
                    }
                    else {
                        celestBody.transform.position = Vector3.zero;
                        celestBody.orbitedBodyRelativeVel = Vector3d.zero;
                    }
                    break;
                
                case goTags.Spaceship:
                    Spaceship ship = obj.GetComponent<Spaceship>();
                    FlyingObj.InitializeOrbit<Spaceship, SpaceshipSettings>(ship);
                    FlyingObj.InitializeBodyPosition<Spaceship, SpaceshipSettings>(ship);
                    FlyingObj.InitializeDirVecLineRenderers<Spaceship, SpaceshipSettings>(ship);

                    tangentialVec = ship.orbit.ComputeDirectionVector(OrbitalParams.typeOfVectorDir.tangentialVec);
                    orbitalSpeed = ship.orbit.GetOrbitalSpeedFromOrbit();
                    ship.orbitedBodyRelativeVel = tangentialVec * orbitalSpeed;
                    //ship.InitializeOrbitalPredictor();
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
                if(celestBody.orbitedBody != null)
                {
                    orbitedBody = celestBody.orbitedBody.GetComponent<CelestialBody>();
                    FlyingObj.GravitationalUpdate<CelestialBody, CelestialBodySettings>(orbitedBody, celestBody);
                    FlyingObj.InitializeDirVecLineRenderers<CelestialBody, CelestialBodySettings>(celestBody);
                    //celestBody.InitializeOrbitalPredictor();
                }
                break;
            
            case goTags.Spaceship:
                Spaceship ship = obj.GetComponent<Spaceship>();
                orbitedBody = ship.orbitedBody.GetComponent<CelestialBody>();
                FlyingObj.GravitationalUpdate<Spaceship, SpaceshipSettings>(orbitedBody, ship);
                FlyingObj.InitializeDirVecLineRenderers<Spaceship, SpaceshipSettings>(ship);
                //ship.InitializeOrbitalPredictor();
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
                FlyingObj.ApplyRigbidbodyPosUpdate<CelestialBody, CelestialBodySettings>(celestBody, celestBody.settings);
                break;
            
            case goTags.Spaceship:
                Spaceship ship = obj.GetComponent<Spaceship>();
                FlyingObj.ApplyRigbidbodyPosUpdate<Spaceship, SpaceshipSettings>(ship, ship.settings);
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
                    ship.orbit.UpdateLineRendererPos();
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