using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using System.IO;

public class Spaceship : MonoBehaviour, Dynamic_Obj_Common
{
    [HideInInspector] public bool orbDataFoldoutBool=true; // For universeRunner custom editor
    //--------------------

    public GameObject _gameObject
    {
        get {
            return this.gameObject;
        }
    }
    Rigidbody _privateRB;
    public Rigidbody _rigidbody
    {
        get {
            return _privateRB;
        }
    }

    [SerializeField]
    OrbitalParams _orbitalParams;
    public OrbitalParams orbitalParams
    {
        get {
            return _orbitalParams;
        }
        set {
            _orbitalParams=value;
        }
    }
    Orbit _orbit;
    public Orbit orbit
    {
        get {
            return _orbit;
        }
    }

    Vector3d _relativeAcc;
    public Vector3d relativeAcc
    {
        get {
            return _relativeAcc;
        }
        set {
            _relativeAcc=value;
        }
    }

    Vector3d _relativeVel;
    public Vector3d relativeVel
    {
        get {
            return _relativeVel;
        }
        set {
            _relativeVel=value;
        }
    }

    Vector3d _realPosition;
    public Vector3d realPosition
    {
        get {
            return _realPosition;
        }
        set {
            _realPosition=value;
        }
    }

    public Velocity3d initStartVel;
    public Distance3d initStartPos;
    //public CelestialBody orbitedBody;

    void Awake()
    {
        //if(orbitedBody == null)
            //Debug.LogErrorFormat("OrbitedBody for {0} is null !", name);
        if(TryGetComponent<Rigidbody>(out _privateRB)) {}
        else
            Debug.LogErrorFormat("Error while trying to get the rigidbody of {0} via its interface.", name);

        _relativeAcc = Vector3d.zero;

        // Making sure the specified input spacecraft velocity is in km/s
        initStartVel = initStartVel.EnsureUnit(Units.velocity.kms);
        _relativeVel = initStartVel.val3d;

        // Making sure the specified input position is in km
        initStartPos = initStartPos.EnsureUnit(Units.distance.km);
        _realPosition = initStartPos.val3d;
        gameObject.transform.position = (Vector3)_realPosition;

        //-----------------------------
        
    }

    public void InitOrbit()
    {
        string shipOrbitalParamsFilePath = Application.persistentDataPath + Filepaths.shipToLoad_orbitalParams;
        if(orbitalParams == null || !File.Exists(shipOrbitalParamsFilePath))
            orbitalParams = ScriptableObject.CreateInstance<OrbitalParams>();
        JsonUtility.FromJsonOverwrite(File.ReadAllText(shipOrbitalParamsFilePath), orbitalParams);
        if(orbitalParams.orbitedBody == null)
            orbitalParams.orbitedBody = GameObject.Find(orbitalParams.orbitedBodyName).GetComponent<CelestialBody>();
            
        _orbit = new Orbit(orbitalParams, orbitalParams.orbitedBody, _gameObject);
    }

    public void Init_Position()
    {
        Vector3d worldPos = Orbit.GetWorldPositionFromOrbit(orbit);
        transform.position = (Vector3)worldPos + orbit.orbitedBody.transform.position;
    }
}