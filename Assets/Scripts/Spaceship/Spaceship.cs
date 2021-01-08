using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using System.IO;

public class Spaceship : MonoBehaviour, Dynamic_Obj_Common
{
    [SerializeField, HideInInspector]
    public bool inspectorFoldout_showInfo=false; // Only for development in the inspector

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

    [SerializeField, HideInInspector]
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
        set {
            _orbit=value;
        }
    }

    [SerializeField, HideInInspector]
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

    [SerializeField, HideInInspector]
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

    [SerializeField, HideInInspector]
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

    Quaterniond _previousRot;
    public Quaterniond deltaRotation
    {
        get {
            Quaterniond outQuat = new Quaterniond(transform.rotation) * Quaterniond.Invert(_previousRot);
            //_previousRot = outQuat;
            return outQuat;
        }
    }

    Vector3d _previousVel;
    public Vector3d deltaRelativeVel
    {
        get {
            Vector3d outDeltaVel = relativeVel - _previousVel;
            //_previousVel = relativeVel;
            return outDeltaVel;
        }
    }

    [SerializeField]
    public bool spawnAs_UI_SC=false;

    void Awake()
    {
        if(spawnAs_UI_SC) {
            realPosition = Vector3d.zero;
            return;
        }

        if(TryGetComponent<Rigidbody>(out _privateRB)) {}
        else
            Debug.LogErrorFormat("Error while trying to get the rigidbody of {0} via its interface.", name);

        string shipOrbitalParamsFilePath = Application.persistentDataPath + Filepaths.shipToLoad_orbitalParams;
        if(orbitalParams == null || !File.Exists(shipOrbitalParamsFilePath))
            orbitalParams = ScriptableObject.CreateInstance<OrbitalParams>();
        JsonUtility.FromJsonOverwrite(File.ReadAllText(shipOrbitalParamsFilePath), orbitalParams);
        if(orbitalParams.orbitedBody == null)
            orbitalParams.orbitedBody = GameObject.Find(orbitalParams.orbitedBodyName).GetComponent<CelestialBody>();
    }
}