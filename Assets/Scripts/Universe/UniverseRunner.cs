using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using System.IO;
//using System.Linq;
//using UnityEngine.Events;

using FlyingObjects;

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

        public CelestialBody earthCB;
        public Spaceship activeSC;

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
            // First action to do: starting the universe clock
            universeClock = GetComponent<UniverseClock>();
            // Applying the simulation parameters to the application
            InitSimEnv();

            if(earthCB == null || activeSC == null)
                Debug.LogError("One of the two required GameObject has not been specified");
            flyingDynamics = new FlyingDynamics(this);
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

        void Start()
        {
            activeSC.relativeAcc = Kepler.GetGravAcc(activeSC.transform.position, activeSC.orbitedBody.transform.position, activeSC.orbitedBody.mu);
            activeSC._rigidbody.AddForce((Vector3) activeSC.relativeVel, ForceMode.VelocityChange);
        }

        void FixedUpdate()
        {
            flyingDynamics.Gravity_Method(activeSC); //flyingDynamics.Leapfrog_Method(sat);
            PrintAltitude();
        }

        private void PrintAltitude()
        {
            Debug.LogFormat("h = {0} km", (double) activeSC.transform.position.magnitude);
        }
    }
}