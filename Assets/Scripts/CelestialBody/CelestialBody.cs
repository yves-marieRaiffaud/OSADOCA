using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using distance = Units.distance;

public class CelestialBody : MonoBehaviour, Dynamic_Obj_Common
{
    [HideInInspector] public bool orbDataFoldoutBool=true; // For universeRunner custom editor
    //--------------------

    [HideInInspector] public CelestialBodySettings settings;

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

    [HideInInspector] public OrbitPlane equatorialPlane;


    public bool usePredefinedPlanet=true;
    [HideInInspector] public CelestialBodiesConstants.planets chosenPredifinedPlanet;
    [HideInInspector] public Dictionary<string, UnitInterface> planetBaseParamsDict;

    [HideInInspector] public CelestialBody orbitedBody;

    void Awake()
    {
        if(TryGetComponent<Rigidbody>(out _privateRB)) {}
        else
            Debug.LogErrorFormat("Error while trying to get the rigidbody of {0} via its interface.", name);

        _relativeAcc = Vector3d.zero;
        _relativeVel = Vector3d.zero;
        _privateRB.velocity = Vector3.zero;
        _realPosition = Vector3d.zero;

        // Initializing equatorial plane, making sure it is back at its default values
        equatorialPlane.forwardVec = new Vector3d(1d, 0d, 0d);
        equatorialPlane.rightVec = new Vector3d(0d, 0d, 1d);
        equatorialPlane.normal = new Vector3d(0d, 1d, 0d);
        equatorialPlane.point = new Vector3d(0d, 0d, 0d);

        /*string celestBodyOrbitalParamsFilePath = Application.persistentDataPath + Filepaths.celestBody_orbitalParams_p1 + name + Filepaths.celestBody_orbitalParams_p2;
        if(orbitalParams == null || !File.Exists(shipOrbitalParamsFilePath))
            orbitalParams = ScriptableObject.CreateInstance<OrbitalParams>();
        JsonUtility.FromJsonOverwrite(File.ReadAllText(shipOrbitalParamsFilePath), orbitalParams);
        orbitalParams.orbitedBody = GameObject.Find(orbitalParams.orbitedBodyName).GetComponent<CelestialBody>();*/
    }

    /// <summary>
    /// Assigning the corresponding dictionnary with respect to the chosen predifined planet. If no predefinied 
    /// </summary>
    public void Assign_PlanetDict(Dictionary<string, UnitInterface> refDictOrbParams)
    {
        planetBaseParamsDict = refDictOrbParams;
    }

    public void Init_Orbit()
    {
        if(_orbitalParams == null)
            _orbitalParams = ScriptableObject.CreateInstance<OrbitalParams>();
        if(_orbitalParams.orbitedBody == null)
            _orbitalParams.orbitedBody = orbitedBody;

        // Called only if 'usePredefinedPlanet' is true
        // Copying orbital values from the planet dictionary to the orbitalParams scriptable object
        _orbitalParams.orbitRealPredType = OrbitalTypes.typeOfOrbit.realOrbit;
        _orbitalParams.orbitDefType = OrbitalTypes.orbitDefinitionType.rarp;
        _orbitalParams.bodyPosType = OrbitalTypes.bodyPositionType.nu;

        if((planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.apoapsis.ToString()] as Distance).HasUnit(distance.km))
            _orbitalParams.orbParamsUnits = OrbitalTypes.orbitalParamsUnits.km;
        else
            _orbitalParams.orbParamsUnits = OrbitalTypes.orbitalParamsUnits.AU;

        _orbitalParams.ra = planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.apoapsis.ToString()].val;
        _orbitalParams.rp = planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.periapsis.ToString()].val;
        _orbitalParams.i = planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.i.ToString()].val;
        _orbitalParams.lAscN = planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.lAscN.ToString()].val;
        _orbitalParams.omega = planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.periapsisArg.ToString()].val;
        _orbitalParams.nu = planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()].val;

        _orbit = new Orbit(orbitalParams, orbitalParams.orbitedBody, _gameObject);
    }

    public void Init_Position()
    {
        Vector3d worldPos = Orbit.GetWorldPositionFromOrbit(orbit);
        Debug.Log("worldPos for " + name + ": " + worldPos);
        transform.position = (Vector3)worldPos + orbitedBody.transform.position;
    }

}