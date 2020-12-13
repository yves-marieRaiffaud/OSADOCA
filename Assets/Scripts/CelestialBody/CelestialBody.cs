using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;

public class CelestialBody : MonoBehaviour, Dynamic_Obj_Common
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

    public double mu;

    void Awake()
    {
        if(TryGetComponent<Rigidbody>(out _privateRB)) {}
        else
            Debug.LogErrorFormat("Error while trying to get the rigidbody of {0} via its interface.", name);

        mu = 3.986004418E14d;
        _relativeAcc = Vector3d.zero;
        _relativeVel = Vector3d.zero;
        _realPosition = Vector3d.zero;
    }

}