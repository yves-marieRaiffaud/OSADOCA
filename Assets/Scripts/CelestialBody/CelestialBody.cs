using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mathd_Lib;

public class CelestialBody: MonoBehaviour, FlyingObjCommonParams
{
    public CelestialBodySettings settings;
    //=========================================
    public GameObject _gameObject { get{return this.gameObject;} set{_gameObject=this.gameObject;} }

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

    private OrbitalParams _orbitalParams;
    public OrbitalParams orbitalParams
    {
        get {
            return _orbitalParams;
        }
        set {
            _orbitalParams=value;
        }
    }

    public CelestialBody _orbitedBody;
    public CelestialBody orbitedBody
    {
        get {
            return _orbitedBody;
        }
        set {
            _orbitedBody=value;
        }
    }

    public Vector3d _realPosition;
    public Vector3d realPosition
    {
        get {
            return _realPosition;
        }
        set {
            _realPosition=value;
        }
    }

    public Vector3d _orbitedBodyRelativeAcc;
    public Vector3d orbitedBodyRelativeAcc
    {
        get {
            return _orbitedBodyRelativeAcc;
        }
        set {
            _orbitedBodyRelativeAcc=value;
        }
    }

    public Vector3d _orbitedBodyRelativeVelIncr;
    public Vector3d orbitedBodyRelativeVelIncr
    {
        get {
            return _orbitedBodyRelativeVelIncr;
        }
        set {
            _orbitedBodyRelativeVelIncr=value;
        }
    }

    public Vector3d _orbitedBodyRelativeVel;
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
    // Bools for editor
    public bool showCelestialBodyInfoPanel;
    public bool planetEditorFoldout;
    
    // Active Camera and distance
    private Transform universePlayerCamera;
    [HideInInspector] public float distanceToPlayer;
    [HideInInspector] public float distanceToPlayerPow2;
    
    // Related to CelestialBody Generation
    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

     void Awake()
    {
        UniverseRunner verse = GameObject.Find("UniverseRunner").GetComponent<UniverseRunner>();
        universePlayerCamera = verse.playerCamera.transform;
        InitializeBodyParameters();
        
        if (gameObject.GetComponent<Presets>() == null)
        {
            Presets presetScript = gameObject.AddComponent<Presets>();
        }
        // InitializeOrbitalParams cannot be here as the CreateComponent function will not be called at this point. Must be called after the Awake() and Start()

        //distanceToPlayer = 10_000f; // Init with a standard value as the camera is not initialized at this point
        distanceToPlayer = Vector3.Distance(transform.position, universePlayerCamera.position);
        distanceToPlayerPow2 = distanceToPlayer * distanceToPlayer;
        GeneratePlanet();
    }

    private void InitializeBodyParameters()
    {
        settings.radiusU = settings.radius * UniCsts.pl2u; // km
    }

    public void GeneratePlanet()
    {
        InitializePlanet();
        GenerateMesh();
    }

    private void InitializePlanet()
    {   
        if(meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }

        Dictionary<Vector3, string> directionsDict = new Dictionary<Vector3, string>()
        {
            {Vector3.up, "up"},
            {Vector3.down, "down"},
            {Vector3.left, "left"},
            {Vector3.right, "right"},
            {Vector3.forward, "forward"},
            {Vector3.back, "back"}
        };

        terrainFaces = new TerrainFace[6];

        for(int i=0; i<6; i++)
        {
            if(meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject(directionsDict[directionsDict.Keys.ElementAt(i)]);
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = settings.bodyMaterial;
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
                meshObj.layer = 9;
            }
            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, directionsDict.Keys.ElementAt(i), this);
        }
    }

    private void GenerateMesh()
    {
        foreach(TerrainFace face in terrainFaces)
        {
            face.ConstructTree();
        }
    }
    //=========================================
    void Start()
    {
        distanceToPlayer = Vector3.Distance(transform.position, universePlayerCamera.position);
        distanceToPlayerPow2 = distanceToPlayer * distanceToPlayer;
        StartCoroutine(PlanetGenerationLoop());
    }

    private IEnumerator PlanetGenerationLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(settings.planetRefreshPeriod);
            UpdatePlanet();
        }
    }

    public void UpdatePlanet()
    {
        foreach (TerrainFace face in terrainFaces)
        {
            face.UpdateTree();
        }
    }
    //=========================================
    void FixedUpdate()
    {
        distanceToPlayer = Vector3.Distance(transform.position, universePlayerCamera.position);
        distanceToPlayerPow2 = distanceToPlayer * distanceToPlayer;
        RotatePlanet();
    }

    private void RotatePlanet()
    {
        Vector3 newRot = (Vector3)settings.rotationAxis * (float)settings.rotationSpeed * Time.deltaTime;
        gameObject.transform.rotation = transform.rotation * Quaternion.Euler(newRot);  
    }
    //=========================================
    public void AssignRefDictOrbitalParams(Dictionary<UniCsts.orbitalParams,double> refDictOrbParams)
    {
        settings.refDictOrbitalParams = refDictOrbParams;
        if(settings.usePredifinedPlanets && orbitalParams==null)
        {
            // If the orbitalParams is not null (an 'OrbitalParams' file has been specified), priority is given to the file
            InitializeOrbitalParameters();
        } 
        else if(!settings.usePredifinedPlanets && orbitalParams==null)
        {
            // else, no need to initialize as the orbitalParams File is provided
            Debug.LogError(this.name + " of type 'CelestialBody' has neither an 'orbitalParams' file nor a predifined planet (set to false).");
        }
    }

    private void InitializeOrbitalParameters()
    {
        Debug.Log("Using a predefined planet for " + this.name);
        orbitalParams = (OrbitalParams)OrbitalParams.CreateInstance<OrbitalParams>();

        orbitalParams.orbitRealPredType = OrbitalParams.typeOfOrbit.realOrbit;
        orbitalParams.orbitDefType = OrbitalParams.orbitDefinitionType.rarp;
        orbitalParams.orbParamsUnits = OrbitalParams.orbitalParamsUnits.AU_degree;
        orbitalParams.bodyPosType = OrbitalParams.bodyPositionType.nu;

        orbitalParams.ra = settings.refDictOrbitalParams[UniCsts.orbitalParams.aphelion];
        orbitalParams.rp = settings.refDictOrbitalParams[UniCsts.orbitalParams.perihelion];
        orbitalParams.i = settings.refDictOrbitalParams[UniCsts.orbitalParams.i];
        orbitalParams.lAscN = settings.refDictOrbitalParams[UniCsts.orbitalParams.longAscendingNode];
        orbitalParams.omega = settings.refDictOrbitalParams[UniCsts.orbitalParams.perihelionArg];
        orbitalParams.nu = settings.refDictOrbitalParams[UniCsts.orbitalParams.trueAnomaly];

        orbitalParams.orbitDrawingResolution = 300;
        orbitalParams.drawOrbit = true;
        orbitalParams.drawDirections = true;
        orbitalParams.selectedVectorsDir = (OrbitalParams.typeOfVectorDir)256;
    }



    public static bool CelestialBodyHasTagName(CelestialBody body, string tagName)
    {
        if(body.tag == tagName) { return true; }
        else{ return false; }
    }

}