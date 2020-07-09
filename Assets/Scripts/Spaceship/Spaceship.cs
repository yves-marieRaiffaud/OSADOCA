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

    [SerializeField, HideInInspector]
    private Orbit _orbit;
    [SerializeField, HideInInspector]
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
    private OrbitalPredictor _predictor;
    [SerializeField, HideInInspector]
    public OrbitalPredictor predictor
    {
        get {
            return _predictor;
        }
        set {
            _predictor=value;
        }
    }

    [SerializeField, HideInInspector]
    private OrbitalParams _orbitalParams;
    [SerializeField, HideInInspector]
    public OrbitalParams orbitalParams
    {
        get {
            return _orbitalParams;
        }
        set {
            _orbitalParams=value;
        }
    }

    [SerializeField, HideInInspector]
    private CelestialBody _orbitedBody;
    [SerializeField, HideInInspector]
    public CelestialBody orbitedBody
    {
        get {
            return _orbitedBody;
        }
        set {
            _orbitedBody=value;
        }
    }

    [SerializeField, HideInInspector]
    private Vector3d _realPosition;
    [SerializeField, HideInInspector]
    public Vector3d realPosition
    {
        get {
            return _realPosition;
        }
        set {
            _realPosition=value;
        }
    }

    [SerializeField, HideInInspector]
    private Vector3d _orbitedBodyRelativeAcc;
    [SerializeField, HideInInspector]
    public Vector3d orbitedBodyRelativeAcc
    {
        get {
            return _orbitedBodyRelativeAcc;
        }
        set {
            _orbitedBodyRelativeAcc=value;
        }
    }

    [SerializeField, HideInInspector]
    private Vector3d _orbitedBodyRelativeVelIncr;
    [SerializeField, HideInInspector]
    public Vector3d orbitedBodyRelativeVelIncr
    {
        get {
            return _orbitedBodyRelativeVelIncr;
        }
        set {
            _orbitedBodyRelativeVelIncr=value;
        }
    }

    [SerializeField, HideInInspector]
    private Vector3d _orbitedBodyRelativeVel;
    [SerializeField, HideInInspector]
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
        // Do nothing for now
    }

    public void InitializeOrbitalPredictor()
    {
        predictor = new OrbitalPredictor(this, orbitedBody.GetComponent<CelestialBody>(), orbit);
    }

    public Vector3d GetRelativeRealWorldPosition()
    {
        return new Vector3d(transform.position - orbitedBody.transform.position) * UniCsts.u2pl;
    }

    public Vector3d GetRelativeVelocity()
    {
        return orbitedBodyRelativeVel;
    }

    public double GetRelativeVelocityMagnitude()
    {
        return orbitedBodyRelativeVel.magnitude;
    }
}
