using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;

public class Spaceship : MonoBehaviour, FlyingObjCommonParams
{
    [HideInInspector] public bool foldoutFlyingObjInfoPanel;

    [HideInInspector] public SpaceshipSettings settings;
    //=========================================
    public GameObject _gameObject { get{return this.gameObject;} set{_gameObject=this.gameObject;} }

    [HideInInspector]
    private Orbit _orbit;
    public Orbit orbit
    {
        get {
            return _orbit;
        }
        set {
            _orbit=value;
        }
    }

    [HideInInspector]
    private OrbitalPredictor _predictor;
    public OrbitalPredictor predictor
    {
        get {
            return _predictor;
        }
        set {
            _predictor=value;
        }
    }

    public OrbitalParams _orbitalParams;
    public OrbitalParams orbitalParams
    {
        get {
            return _orbitalParams;
        }
        set {
            _orbitalParams=value;
        }
    }

    [HideInInspector]
    private Vector3d _realPosition;
    public Vector3d realPosition
    {
        get {
            return _realPosition;
        }
        set {
            _realPosition=value;
        }
    }

    [HideInInspector]
    private Vector3d _orbitedBodyRelativeAcc;
    public Vector3d orbitedBodyRelativeAcc
    {
        get {
            return _orbitedBodyRelativeAcc;
        }
        set {
            _orbitedBodyRelativeAcc=value;
        }
    }

    [HideInInspector]
    private Vector3d _orbitedBodyRelativeVelIncr;
    public Vector3d orbitedBodyRelativeVelIncr
    {
        get {
            return _orbitedBodyRelativeVelIncr;
        }
        set {
            _orbitedBodyRelativeVelIncr=value;
        }
    }

    [HideInInspector]
    private Vector3d _orbitedBodyRelativeVel;
    public Vector3d orbitedBodyRelativeVel
    {
        get {
            return _orbitedBodyRelativeVel;
        }
        set {
            _orbitedBodyRelativeVel=value;
        }
    }
    //=========================================
    
    void Awake()
    {
        // FOR DEBUG PURPOSES
        if(GameObject.Find("DEBUG") != null) 
        {
            DebugGameObject debugGO = GameObject.Find("DEBUG").GetComponent<DebugGameObject>();
            if(GameObject.Find("UniverseRunner") != null && orbitalParams == null && !debugGO.loadShipDataFromUIDiskFile)
            {
                Debug.Log("in");
                Debug.Log("/Spaceship/Rocket/OrbitalParams/" + gameObject.name + "_OrbitalParams.asset");
                orbitalParams = Resources.Load<OrbitalParams>("/Spaceship/Rocket/OrbitalParams/" + gameObject.name + "_OrbitalParams.asset");
                Debug.Log(orbitalParams);
                if(orbitalParams.orbitedBodyName.Equals("None"))
                {
                    orbitalParams.orbitedBody = null;
                }
            }
            else
            {
                DEBUG_LOAD_DATA_TO_SCRIPTABLE_OBJS();
                orbitalParams.orbitedBody = GameObject.Find(orbitalParams.orbitedBodyName).GetComponent<CelestialBody>();
            }
        }

        
    }

    public void InitializeOrbitalPredictor()
    {
        predictor = new OrbitalPredictor(this, orbitalParams.orbitedBody.GetComponent<CelestialBody>(), orbit);
    }

    public Vector3d GetRelativeRealWorldPosition()
    {
        return new Vector3d(transform.position - orbitalParams.orbitedBody.transform.position) * UniCsts.u2pl;
    }

    public Vector3d GetRelativeVelocity()
    {
        return orbitedBodyRelativeVel;
    }

    public double GetRelativeVelocityMagnitude()
    {
        return orbitedBodyRelativeVel.magnitude;
    }

    public void DEBUG_LOAD_DATA_TO_SCRIPTABLE_OBJS()
    {
        if(GameObject.Find("DEBUG") != null)
        {
            DebugGameObject scriptInstance = GameObject.Find("DEBUG").GetComponent<DebugGameObject>();
            bool needToLoadJSONFiles = scriptInstance.loadShipDataFromUIDiskFile;
            if(needToLoadJSONFiles)
            {
                // Reading & Copying the JSON files to the right Scriptable Objects of the spaceship
                settings = SpaceshipSettingsSaveData.LoadObjectFromJSON(Application.persistentDataPath + Filepaths.shipToLoad_settings);
                orbitalParams = OrbitalParamsSaveData.LoadObjectFromJSON(Application.persistentDataPath + Filepaths.shipToLoad_orbitalParams);
            }
        }
        // don't need to do anything if we want to use the Scriptables objects set up in the Unity Editor Inspector
    }
}
