using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

using Mathd_Lib;
using Universe;
using UniCsts = UniverseConstants;
using ObjHand = CommonMethods.ObjectsHandling;
using distance = Units.distance;
using angle = Units.angle;

public class CelestialBody : MonoBehaviour, Dynamic_Obj_Common
{
    [SerializeField, HideInInspector] public bool inspectorFoldout_showInfo=false;
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

    [HideInInspector] public OrbitPlane equatorialPlane;

    public bool spawnAsUIPlanet=false;

    public bool usePredefinedPlanet=true;
    [HideInInspector] public CelestialBodiesConstants.planets chosenPredifinedPlanet;

    [HideInInspector] public CelestialBody orbitedBody;

    [HideInInspector,NonSerialized] GameObject _orbitFolderGO;
    public GameObject orbitFolderGO
    {
        get {
            return _orbitFolderGO;
        }
        set {
            _orbitFolderGO=value;
        }
    }

    void Awake()
    {
        if(settings == null)
            settings = ScriptableObject.CreateInstance<CelestialBodySettings>();
        if(_orbitalParams == null)
            _orbitalParams = ScriptableObject.CreateInstance<OrbitalParams>();

        SetMaterial();

        // Initializing equatorial plane, making sure it is back at its default values
        equatorialPlane.forwardVec = new Vector3d(1d, 0d, 0d);
        equatorialPlane.rightVec = new Vector3d(0d, 0d, 1d);
        equatorialPlane.normal = new Vector3d(0d, 1d, 0d);
        equatorialPlane.point = new Vector3d(0d, 0d, 0d);

        Create_OrbitFolder();
        if(spawnAsUIPlanet) {
            realPosition = Vector3d.zero;
            return;
        }

        // Code below only executed is 'spawnAsUIPlanet' is NOT SELECTED
        if(TryGetComponent<Rigidbody>(out _privateRB)) {}
        else
            Debug.LogErrorFormat("Error while trying to get the rigidbody of {0} via its interface.", name);
    }
    void SetMaterial()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Resources.Load<Material>(Filepaths.RSC_PlanetsMaterials + name + "_mat");
    }

    void Create_OrbitFolder()
    {
        string orbitFolderName = UniverseRunner.goTags.Orbit.ToString();
        if(_gameObject.transform.Find(orbitFolderName) == null) {
            _orbitFolderGO = new GameObject(orbitFolderName);
            _orbitFolderGO.transform.parent = _gameObject.transform;
            _orbitFolderGO.transform.localPosition = Vector3.zero;
            float orbitFolderScaleGO = 1f/transform.localScale.x;
            _orbitFolderGO.transform.localScale = new Vector3(orbitFolderScaleGO,orbitFolderScaleGO,orbitFolderScaleGO);
            _orbitFolderGO.transform.rotation = Quaternion.identity;
        }
        else
            _orbitFolderGO = _gameObject.transform.Find(orbitFolderName).gameObject;
    }

    /// <summary>
    /// Initializing the settings variable of the CelestialBody object, by reading its corresponding JSON CelestialBodySettings file.
    /// </summary>
    public void Init_CelestialBodySettings()
    {
        // Filepath of the CelestialBodySettings JSON file correpsonding to the this.name object
        string filepath = Application.persistentDataPath + Filepaths.celestBody_Folder + name + Filepaths.celestBodySettingsFile;
        JsonUtility.FromJsonOverwrite(File.ReadAllText(filepath), settings);

        if(!spawnAsUIPlanet) {
            if(_orbitalParams.orbitedBody == null && !settings.lightOrbitalParams.orbitedBodyName.stringVal.Equals("None"))
                _orbitalParams.orbitedBody = GameObject.Find(settings.lightOrbitalParams.orbitedBodyName.stringVal).GetComponent<CelestialBody>();
        }

        _orbitalParams.orbitRealPredType = OrbitalTypes.typeOfOrbit.realOrbit;
        _orbitalParams.orbitDefType = OrbitalTypes.orbitDefinitionType.rarp;
        _orbitalParams.bodyPosType = OrbitalTypes.bodyPositionType.nu;

        if(settings.lightOrbitalParams.apoapsis.HasUnit(distance.km))
            _orbitalParams.orbParamsUnits = OrbitalTypes.orbitalParamsUnits.km;
        else
            _orbitalParams.orbParamsUnits = OrbitalTypes.orbitalParamsUnits.AU;

        _orbitalParams.ra = settings.lightOrbitalParams.apoapsis.ConvertTo(distance.AU).val;
        _orbitalParams.rp = settings.lightOrbitalParams.periapsis.ConvertTo(distance.AU).val;
        _orbitalParams.i = settings.lightOrbitalParams.i.ConvertTo(angle.radian).val;
        _orbitalParams.lAscN = settings.lightOrbitalParams.lAscN.ConvertTo(angle.radian).val;
        _orbitalParams.omega = settings.lightOrbitalParams.periapsisArg.ConvertTo(angle.radian).val;
        _orbitalParams.nu = settings.lightOrbitalParams.trueAnomaly.ConvertTo(angle.radian).val;

        // If no JSON file is found matching the name of the this.name, thus need to compare the name of the celestialBody with the list of planets.
        // If one planet from the list matches the name of the this.name, need to copy its dictionary values to the settings variable of 'this'
        // And finally, need to save the settings object to disk by creating and saving its corresponding JSON file (will howevre work without this in the editor, but not at deployement).
    
        /*if(!spawnAsUIPlanet) {
            float simBodyRadius = (float)(settings.planetaryParams.radius.val.ToSim());
            transform.localScale = new Vector3(simBodyRadius, simBodyRadius, simBodyRadius);
        }*/
    }
}