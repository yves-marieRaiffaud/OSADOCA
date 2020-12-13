using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;

public class Spaceship : MonoBehaviour, Dynamic_Obj_Common
{
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
    public Vector3d initStartPos;
    public CelestialBody orbitedBody;

    void Awake()
    {
        if(orbitedBody == null)
            Debug.LogErrorFormat("OrbitedBody for {0} is null !", name);
        if(TryGetComponent<Rigidbody>(out _privateRB)) {}
        else
            Debug.LogErrorFormat("Error while trying to get the rigidbody of {0} via its interface.", name);

        _relativeAcc = Vector3d.zero;

        // Making sure the specified input spacecraft velocity is in km/s
        if(!initStartVel.HasUnit(Units.velocity.kms))
            initStartVel = initStartVel.ConvertTo(Units.velocity.kms);
        _relativeVel = initStartVel.val;
        
        _realPosition = initStartPos;
        gameObject.transform.position = (Vector3)_realPosition;
    }
}