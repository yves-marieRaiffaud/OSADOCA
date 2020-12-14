using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using distance = Units.distance;

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

    public CelestialBodiesConstants.planets chosenPredifinedPlanet;
    [HideInInspector] public Dictionary<string, UnitInterface> planetBaseParamsDict;

    public CelestialBody orbitedBody;
    public double mu;

    void Awake()
    {
        if(TryGetComponent<Rigidbody>(out _privateRB)) {}
        else
            Debug.LogErrorFormat("Error while trying to get the rigidbody of {0} via its interface.", name);

        mu = 3.986004418E14d;
        _relativeAcc = Vector3d.zero;
        _relativeVel = Vector3d.zero;
        _privateRB.velocity = Vector3.zero;
        _realPosition = Vector3d.zero;
    }

    /// <summary>
    /// Assigning the corresponding dictionnary with respect to the chosen predifined planet. If no predefinied 
    /// </summary>
    public void Assign_PlanetDict(Dictionary<string, UnitInterface> refDictOrbParams)
    {
        planetBaseParamsDict = refDictOrbParams;
        Init_OrbitalParams();
        /*if(settings.usePredifinedPlanets)
        {
            // Copying orbital values from the planet dictionary to the orbitalParams scriptable object
            InitializeOrbitalParameters();
        }*/
    }

    void Init_OrbitalParams()
    {
        // Called only if 'usePredefinedPlanet' is true
        // Copying orbital values from the planet dictionary to the orbitalParams scriptable object
        _orbitalParams.orbitRealPredType = OrbitalTypes.typeOfOrbit.realOrbit;
        _orbitalParams.orbitDefType = OrbitalTypes.orbitDefinitionType.rarp;
        _orbitalParams.bodyPosType = OrbitalTypes.bodyPositionType.nu;

        if((planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.aphelion.ToString()] as Distance).HasUnit(distance.km))
            _orbitalParams.orbParamsUnits = OrbitalTypes.orbitalParamsUnits.km_degree;
        else
            _orbitalParams.orbParamsUnits = OrbitalTypes.orbitalParamsUnits.AU_degree;

        _orbitalParams.ra = planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.aphelion.ToString()].val;
        _orbitalParams.rp = planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.perihelion.ToString()].val;
        _orbitalParams.i = planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.i.ToString()].val;
        _orbitalParams.lAscN = planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()].val;
        _orbitalParams.omega = planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()].val;
        _orbitalParams.nu = planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()].val;
    }

}