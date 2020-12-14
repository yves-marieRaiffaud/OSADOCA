using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using System.IO;
//using System.Linq;
//using UnityEngine.Events;

using FlyingObjects;
using ComFcn = CommonMethods;
using UniCsts = UniverseConstants;
using PlanetsCsts = CelestialBodiesConstants;

namespace Universe
{
    [DisallowMultipleComponent, System.Serializable]
    public class UniverseRunner : MonoBehaviour
    {
        [HideInInspector] public bool simulationEnvFoldout=true; // For universeRunner custom editor

        // Keeping date and time of the current state of the universe
        [HideInInspector] public UniverseClock universeClock;
        [HideInInspector] public SimulationEnv simEnv;

        public FlyingDynamics flyingDynamics;
        public Vector3 universeOffset; // Keeping track of the offset of the whole universe

        public CelestialBody earthCB;
        public Spaceship activeSC;
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

        void Awake()
        {
            universeClock = GetComponent<UniverseClock>(); // First action to do: starting the universe clock
            InitSimEnv(); // Applying the simulation parameters to the application
            flyingDynamics = new FlyingDynamics(this);
            universeOffset = new Vector3(0f, 0f, 0f);

            // Ordering hierarchy tree
            physicsObjGO = ComFcn.ObjectsHandling.CreateAssignGameObject(folderNames.PhysicsObjs.ToString());
            physicsObjArray = new List<Transform>();
            physicsRigidbodies = new List<Rigidbody>();
            InitFolders();
            FillFolders();

            if(earthCB == null || activeSC == null)
                Debug.LogError("One of the two required GameObject has not been specified");
        }

        /// <summary>
        /// Reading the JSON simEnv file from disk and applying the read parameters to the current Unity Application
        /// </summary>
        void InitSimEnv()
        {
            if(simEnv == null) {
                simEnv = ScriptableObject.CreateInstance<SimulationEnv>();
            }
            // Reading the simEnv JSON file from disk 
            string simEnvFilepath = Application.persistentDataPath + Filepaths.simulation_settings;
            JsonUtility.FromJsonOverwrite(File.ReadAllText(simEnvFilepath), simEnv);

            if(simEnv.useTargetFrameRate.value) {
                Application.targetFrameRate = simEnv.targetFrameRate.value;
            }
            Time.fixedDeltaTime = 1f/simEnv.physicsUpdateRate.value;
            Time.timeScale = simEnv.timeScale.value;
            //if(GameObject.Find("missionTimer") != null)
              //  simEnv.missionTimer = GameObject.Find("missionTimer").GetComponent<StopWatch>();
        }

        void InitFolders()
        {
            starFolder = ComFcn.ObjectsHandling.CreateAssignGameObject(folderNames.Stars.ToString(), physicsObjGO);
            planetsFolder = ComFcn.ObjectsHandling.CreateAssignGameObject(folderNames.Planets.ToString(), physicsObjGO);
            spaceshipFolder = ComFcn.ObjectsHandling.CreateAssignGameObject(folderNames.Spaceships.ToString(), physicsObjGO);
            orbitFolder = ComFcn.ObjectsHandling.CreateAssignGameObject(folderNames.Orbits.ToString());
        }

        void FillFolders()
        {
            // FILLING ORDER: Stars, Planets, Spaceships
            foreach(GameObject star in GameObject.FindGameObjectsWithTag(goTags.Star.ToString())) {
                // CelestialBodies and Spaceships must have done their Awake() at this point
                InitializeRigidbody(star);
                AddGameObjectToPhysicsFolders(star, starFolder);
            }

            foreach(GameObject planet in GameObject.FindGameObjectsWithTag(goTags.Planet.ToString())) {
                // CelestialBodies and Spaceships must have done their Awake() at this point
                InitializeRigidbody(planet);
                AddGameObjectToPhysicsFolders(planet, planetsFolder);
            }

            foreach(GameObject vessel in GameObject.FindGameObjectsWithTag(goTags.Spaceship.ToString())) {
                InitializeRigidbody(vessel);
                AddGameObjectToPhysicsFolders(vessel, spaceshipFolder);
            }
        }

        void InitializeRigidbody(GameObject physicGameObject)
        {
            // Initialize everything of the rigidbody except its mass as its need to access the object Settings (done in 'Start' of the UniverseRunner)
            Rigidbody rb = (Rigidbody) ComFcn.ObjectsHandling.CreateAssignComponent(typeof(Rigidbody), physicGameObject);
            rb.drag = 0f;
            rb.angularDrag = 0f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.detectCollisions = true;

            if(physicGameObject.CompareTag(goTags.Star.ToString()) || physicGameObject.CompareTag(goTags.Planet.ToString())) {
                rb.mass = 1e9f;
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.constraints = RigidbodyConstraints.None;
            }
            else if(physicGameObject.CompareTag(goTags.Spaceship.ToString())) {
                rb.mass = 10f;
                rb.constraints = RigidbodyConstraints.FreezeRotation; // Freezing rotation of the S/C
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }
        }

        public void AddGameObjectToPhysicsFolders(GameObject gameObjectToAdd, GameObject parentFolder)
        {
            // Add the GameObject to all of the physics list (list of Transforms, Rigidbody)
            physicsRigidbodies.Add(gameObjectToAdd.GetComponent<Rigidbody>());
            physicsObjArray.Add(gameObjectToAdd.transform);
            ComFcn.ObjectsHandling.ParentObj(gameObjectToAdd, parentFolder);
        }

        void Start()
        {
            foreach(Transform objTr in physicsObjArray) {
                switch(ComFcn.ObjectsHandling.Str_2_GoTags(objTr.tag))
                {
                    case goTags.Star:
                        objTr.position = Vector3.zero;
                        CelestialBody starBody = objTr.GetComponent<CelestialBody>();
                        //starBody.Awake();
                        break;

                    case goTags.Planet:
                        objTr.position = Vector3.zero;
                        CelestialBody celestBody = objTr.GetComponent<CelestialBody>();
                        if(GameObject.FindGameObjectsWithTag(goTags.Star.ToString()).Length > 0) {
                            celestBody.Assign_PlanetDict(PlanetsCsts.planetsDict[celestBody.chosenPredifinedPlanet]);
                            //flyingDynamics.init();
                            celestBody.transform.position = Vector3.zero;
                            celestBody.relativeVel = Vector3d.zero;
                        }
                        else {
                            celestBody.transform.position = Vector3.zero;
                            celestBody.relativeVel = Vector3d.zero;
                        }
                        break;

                    case goTags.Spaceship:
                        Spaceship ship = objTr.GetComponent<Spaceship>();
                        //flyingDynamics.init();
                        break;
                }
            }

            // Init spaceship movement
            activeSC.relativeAcc = Kepler.GetGravAcc(activeSC.transform.position, activeSC.orbitedBody.transform.position, activeSC.orbitedBody.mu);
            activeSC._rigidbody.AddForce((Vector3) activeSC.relativeVel, ForceMode.VelocityChange);
        }

        void FixedUpdate()
        {
            if(simEnv.simulateGravity.value) {
                flyingDynamics.GravitationnalStep();
                PrintAltitude();
            }
            updateFloatingOrigin();
        }

        private void PrintAltitude()
        {
            Debug.LogFormat("h = {0} km", (double) (activeSC.transform.position - earthCB.transform.position).magnitude);
        }

        private void updateFloatingOrigin()
        {
            Vector3 originOffset = playerCamera.transform.position;
            float dstFromOrigin = originOffset.magnitude;
            if(dstFromOrigin > UniCsts.dstThreshold)
            {
                foreach(Transform t in physicsObjArray) {
                    t.position -= originOffset; // Offset every Star, Planet and Spaceship
                }
                universeOffset += originOffset;
            }
        }
    
    }
}