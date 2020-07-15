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

    [HideInInspector]
    public CelestialBody _orbitedBody;
    [HideInInspector]
    public CelestialBody orbitedBody
    {
        get {
            return _orbitedBody;
        }
        set {
            _orbitedBody=value;
        }
    }

    [HideInInspector]
    public Vector3d _realPosition;
    [HideInInspector]
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
    public Vector3d _orbitedBodyRelativeAcc;
    [HideInInspector]
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
    public Vector3d _orbitedBodyRelativeVelIncr;
    [HideInInspector]
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
    public Vector3d _orbitedBodyRelativeVel;
    [HideInInspector]
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
    [HideInInspector] public bool showCelestialBodyInfoPanel;
    [HideInInspector] public bool planetEditorFoldout;
    
    // Active Camera and distance
    private Transform universePlayerCamera;
    [HideInInspector] public float distanceToPlayer;
    [HideInInspector] public float distanceToPlayerPow2;
    
    // Related to CelestialBody Generation
    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    public void AwakeCelestialBody()
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

        // Apply oblateness to the planet
        float flatenningVal = (float)settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()];
        float flatenningScale = 1f;
        if(!UsefulFunctions.FloatsAreEqual(flatenningVal, 0f))
        {
            flatenningScale = 1f - 1f/flatenningVal;
        }
        Vector3 bodyScale = new Vector3(1f, flatenningScale, 1f);
        gameObject.transform.localScale = bodyScale;
    }

    private void InitializeBodyParameters()
    {
        settings.radiusU = settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()] * UniCsts.pl2u; // km
        
        double siderealPeriod = settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()];
        if(!UsefulFunctions.DoublesAreEqual(siderealPeriod, 0d))
        {
            settings.rotationSpeed = (double)Time.fixedDeltaTime * 360d / siderealPeriod; // in °.s-1

        }
        else {
            settings.rotationSpeed = 0d;
        }
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
                MeshRenderer renderer = meshObj.GetComponent<MeshRenderer>();
                renderer.receiveShadows = false;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
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
    public void StartCelestialBody()
    {
        distanceToPlayer = Vector3.Distance(transform.position, universePlayerCamera.position);
        distanceToPlayerPow2 = distanceToPlayer * distanceToPlayer;
        StartCoroutine(PlanetGenerationLoop());
    }

    public void InitializeAxialTilt()
    {
        float axialTitleAngle = (float)settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()];
        // Rotation vector is in the orbital plane and perpendicular to the radial vector
        Vector3 positivePole = Vector3.up;
        Debug.Log("orbitalParams.orbitPlane.normal = " + orbitalParams.orbitPlane.normal);
        Vector3 normalUp = (Vector3)orbitalParams.orbitPlane.normal;
        //orbit.param.orbitPlane.normal does not work.
        Vector3 rotAxis = Vector3.Cross(positivePole, normalUp);

        Quaternion tiltRotation = Quaternion.AngleAxis(axialTitleAngle, rotAxis);
        gameObject.transform.rotation *= tiltRotation;
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
        if(!UsefulFunctions.DoublesAreEqual(settings.rotationSpeed, 0d))
        {
            RotatePlanet();
        }
    }

    private void RotatePlanet()
    {
        Vector3 rotVec = Vector3.Cross((Vector3)settings.equatorialPlane.rightVec, (Vector3)settings.equatorialPlane.forwardVec);
        gameObject.transform.rotation = transform.rotation * Quaternion.Euler((float)-settings.rotationSpeed * rotVec);  // Sign "-" for the prograde rotation
    }
    //=========================================
    public void AssignRefDictOrbitalParams(Dictionary<string,double> refDictOrbParams)
    {
        settings.planetBaseParamsDict = refDictOrbParams;
        if(settings.usePredifinedPlanets && orbitalParams==null)
        {
            // If the orbitalParams is not null (an 'OrbitalParams' file has been specified), priority is given to the file
            InitializeOrbitalParameters();
        } 
        else if(!settings.usePredifinedPlanets && orbitalParams==null)
        {
            // else, no need to initialize as the orbitalParams File is provided
            //Debug.LogError(this.name + " of type 'CelestialBody' has neither an 'orbitalParams' file nor a predifined planet (set to false).");
        }
    }

    private void InitializeOrbitalParameters()
    {
        orbitalParams = (OrbitalParams)OrbitalParams.CreateInstance<OrbitalParams>();

        orbitalParams.orbitRealPredType = OrbitalParams.typeOfOrbit.realOrbit;
        orbitalParams.orbitDefType = OrbitalParams.orbitDefinitionType.rarp;
        orbitalParams.orbParamsUnits = OrbitalParams.orbitalParamsUnits.AU_degree;
        orbitalParams.bodyPosType = OrbitalParams.bodyPositionType.nu;

        orbitalParams.ra = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.aphelion.ToString()];
        orbitalParams.rp = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.perihelion.ToString()];
        orbitalParams.i = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.i.ToString()];
        orbitalParams.lAscN = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()];
        orbitalParams.omega = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()];
        orbitalParams.nu = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()];

        orbitalParams.orbitDrawingResolution = 300;
        orbitalParams.drawOrbit = true;
        orbitalParams.drawDirections = true;
        orbitalParams.selectedVectorsDir = (OrbitalParams.typeOfVectorDir)1022; // Draw everything
    }

    public void InitializeOrbitalPredictor()
    {
        predictor = new OrbitalPredictor(this, orbitedBody.GetComponent<CelestialBody>(), orbit);
    }


    public static bool CelestialBodyHasTagName(CelestialBody body, string tagName)
    {
        if(body.tag == tagName) { return true; }
        else{ return false; }
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