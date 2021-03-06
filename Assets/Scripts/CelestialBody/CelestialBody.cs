using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Mathd_Lib;
using UnityEngine.SceneManagement;

public class CelestialBody: MonoBehaviour, FlyingObjCommonParams
{
    //public ComputeShader elevationComputeShader; 
    [HideInInspector] public UniverseRunner universeRunner; // Assigned in the Awake() function
    //=========================================
    public CelestialBodySettings settings;
    //=========================================
    public GameObject _gameObject { get{return this.gameObject;} set{_gameObject=this.gameObject;} }

    Vector3d _rbVelocity;
    public Vector3d rbVelocity
    {
        get {
            return _rbVelocity;
        }
        set {
            _rbVelocity=value;
        }
    }

    [HideInInspector, SerializeField]
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

    [HideInInspector, SerializeField]
    private OrbitalPredictor _predictor;
    public OrbitalPredictor predictor
    {
        get {
            return _predictor;
        }
        set {
            _predictor=value;
        }
    }

    public OrbitalParams _orbitalParams;
    public OrbitalParams orbitalParams
    {
        get {
            return _orbitalParams;
        }
        set {
            _orbitalParams=value;
        }
    }

    private double _distanceScaleFactor;
    public double distanceScaleFactor
    {
        get {
            return _distanceScaleFactor;
        }
        set {
            _distanceScaleFactor=value;
        }
    }

    [HideInInspector]
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

    [HideInInspector]
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

    [HideInInspector]
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

    [HideInInspector]
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
    
    public CelestialBodyPullForce[] _gravPullList;
    public CelestialBodyPullForce[] gravPullList
    {
        get {
            return _gravPullList;
        }
        set {
            _gravPullList=value;
        }
    }

    /// <summary>
    /// Returns the north pole axis of the CelestialBody, in WORLD space
    /// </summary>
    public Vector3 worldNorthPoleAxis {
        get {
            return transform.TransformDirection(Vector3.up);
        }
    }
    //=========================================
    public bool spawnAsSimpleSphere = false; // For the UI, will spawn a simple sphere (no LOD, culling etc.). For the simlation, will spawn the complex one
    //=========================================
    // Bools for editor
    [HideInInspector] public bool showCelestialBodyInfoPanel;
    [HideInInspector] public bool planetEditorFoldout;
    //=========================================
    // Distance from the CelestialBody to the active Spaceship
    [HideInInspector] public float distanceToPlayer;
    [HideInInspector] public float distanceToPlayerPow2;
    //=========================================
    // Related to CelestialBody Generation
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;
    //=========================================
    private IEnumerator planetGenerationCoroutine;
    public bool planetGenerationCoroutineIsRunning = false;
    private float nonLODTransitionDistance; // in unity units, using the 'pl2u' scaling factor
    string sphereTemplateCelestBodyName = "sphereTemplate";
    [Header("Force LOD/Non-LOD Sphere System")]
    public bool forceNonLODSphere = false;
    public bool forceLODSphere = false;
    //=========================================
    RungeKutta4<FlyingObjCommonParams> _rk4;
    public RungeKutta4<FlyingObjCommonParams> rk4
    {
        get {
            return _rk4;
        }
        set {
            _rk4=value;
        }
    }

    public void InitRK4()
    {
        rk4 = new RungeKutta4<FlyingObjCommonParams>(orbitalParams.orbitedBody, this, Time.fixedDeltaTime);
    }

    private string GetAssetName()
    {
        string assetName;
        if(SceneManager.GetActiveScene().name.Equals("Universe"))
            assetName = name;
        else
            assetName = name.Substring(0, name.Length - 9); // Removing "Planet_UI" from the GameObject's name == 9 characters
        return assetName;
    }

    void Awake()
    {
        _rbVelocity = Vector3d.zero;
        CheckOrbitalFileAndBodySettings();
        if(GameObject.Find("UniverseRunner") == null)
        {
            // Can not find any UniverseRunner in the scene, thus it is a celetialBody in a UI menu
            spawnAsSimpleSphere = true; // Enforcing simple sphere rendering as no universePlayerCamera will be found
            universeRunner = null;
        }
        else {
            universeRunner = GameObject.Find("UniverseRunner").GetComponent<UniverseRunner>();
            gravPullList = new CelestialBodyPullForce[universeRunner.simEnv.NBODYSIM_NB_BODY.value];
        }
    }

    private void CheckOrbitalFileAndBodySettings()
    {
        string orbitalParamsfilepath = Filepaths.DEBUG_planetOrbitalParams_0 + GetAssetName();
        if(_orbitalParams == null)
            _orbitalParams = Resources.Load<OrbitalParams>(orbitalParamsfilepath);
        //Debug.Log("orbitalParams: " + orbitalParams + "; _orbitalParams: " + _orbitalParams + "; current scene: " + SceneManager.GetActiveScene().name);
            
        if(settings == null)
            settings = Resources.Load<CelestialBodySettings>(Filepaths.DEBUG_planetSettings_0 + GetAssetName());
    }

    public void AwakeCelestialBody(Dictionary<string, UnitInterface> refDictOrbParams)
    {
        CheckOrbitalFileAndBodySettings();
        AssignRefDictOrbitalParams(refDictOrbParams);
        if(spawnAsSimpleSphere) { 
            gameObject.GetComponent<MeshRenderer>().material = settings.sphereTemplateMaterial;
            InitializeBodyParameters();
            return; // Early exit if it's a UI celestialBody
        }
        
        InitializeBodyParameters();

        if (gameObject.GetComponent<Presets>() == null)
        {
            Presets presetScript = gameObject.AddComponent<Presets>();
        }

        // InitializeOrbitalParams cannot be here as the CreateComponent function will not be called at this point. Must be called after the Awake() and Start()
        GetDistancesToCamera();
        GeneratePlanet();
        ApplyFlatenningScale();
        CreateAssignSunPointLight();
        UpdateLODDistances();

        planetGenerationCoroutine = PlanetGenerationLoop();
        SwitchFromLODSystemToNonLODSphere();
    }

    private void UpdateLODDistances()
    {
        settings.detailLevelDistances[0] = float.PositiveInfinity;
        settings.detailLevelDistances[1] = (float)settings.radiusU * UniCsts.ratioCelestBodiesLODDistances[0];
        settings.detailLevelDistances[2] = (float)settings.radiusU * UniCsts.ratioCelestBodiesLODDistances[1];
        settings.detailLevelDistances[3] = (float)settings.radiusU * UniCsts.ratioCelestBodiesLODDistances[2];
        settings.detailLevelDistances[4] = (float)settings.radiusU * UniCsts.ratioCelestBodiesLODDistances[3]; 
        settings.detailLevelDistances[5] = (float)settings.radiusU * UniCsts.ratioCelestBodiesLODDistances[4];
        settings.detailLevelDistances[6] = (float)settings.radiusU * UniCsts.ratioCelestBodiesLODDistances[5];
    }

    private void GetDistancesToCamera()
    {
        distanceToPlayer = Vector3.Distance(transform.position, universeRunner.activeSpaceship.transform.position);
        distanceToPlayerPow2 = distanceToPlayer * distanceToPlayer;
    }

    private void ApplyFlatenningScale()
    {
        // Apply oblateness to the planet
        float flatenningVal = (float)settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()].value;
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
        // RadiusU
        settings.radiusU = settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()].value * UniCsts.pl2u; // km

        // Rotation Speed
        double siderealPeriod = settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()].value;
        if(!UsefulFunctions.DoublesAreEqual(siderealPeriod, 0d))
        {
            settings.rotationSpeed = (double)Time.fixedDeltaTime * 360d / siderealPeriod; // in °.s-1

        }
        else {
            settings.rotationSpeed = 0d;
        }

        // Initializing equatorial plane, making sure it is back at its default values
        settings.equatorialPlane.forwardVec = new Vector3d(1d, 0d, 0d);
        settings.equatorialPlane.rightVec = new Vector3d(0d, 0d, 1d);
        settings.equatorialPlane.normal = new Vector3d(0d, 1d, 0d);
        settings.equatorialPlane.point = new Vector3d(0d, 0d, 0d);
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

        bool hasDefaultHeightMap = false;
        if(settings.heightMap == null) {
            // Set the variable to the default (all black) Texture2D
            settings.heightMap = Resources.Load<Texture2D>(Filepaths.DEBUG_defaultHeightMap);
            hasDefaultHeightMap = true;
        }
        if(!settings.planetBaseParamsDict.ContainsKey(CelestialBodyParamsBase.biomeParams.highestBumpAlt.ToString()))
        {
            // Set variable to a default key
            settings.planetBaseParamsDict.Add(CelestialBodyParamsBase.biomeParams.highestBumpAlt.ToString(), new DoubleNoDim(0d));
        }
        // Need rotate the texture in another variable to avoid the heightmap field from being reset in the inspector because of a type mismatch at runtime
        Texture2D rotatedHeightMap = UsefulFunctions.RotateTexture180(settings.heightMap);

        for(int i=0; i<6; i++)
        {
            if(meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject(directionsDict[directionsDict.Keys.ElementAt(i)]);
                meshObj.transform.parent = transform;
                meshObj.transform.position = transform.position;

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = settings.bodyMaterial;
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
                meshFilters[i].sharedMesh.name = directionsDict[directionsDict.Keys.ElementAt(i)].ToString() + "_mesh";
                meshObj.layer = 9;
                meshObj.SetActive(false);
            }
            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, directionsDict.Keys.ElementAt(i), this, rotatedHeightMap, hasDefaultHeightMap, universeRunner.activeSpaceship.transform);
        }

        // Finally, create an additional gameobject to hold the sphere template when the celestialBody is super far from the activeCamera
        // By default, the NON-LOD sphere system is disabled
        CreateNonLODSphere_GameObject();
    }

    private void CreateNonLODSphere_GameObject()
    {
        GameObject sphereGO = new GameObject(sphereTemplateCelestBodyName, typeof(MeshRenderer), typeof(MeshFilter));
        sphereGO.transform.parent = transform;
        sphereGO.transform.position = transform.position;
        sphereGO.layer = 9;
        float scaleToApply = 2f * (float)settings.radiusU;
        sphereGO.transform.localScale = new Vector3(scaleToApply, scaleToApply, scaleToApply);

        MeshFilter meshFilterToCopy = GameObject.Find(UniCsts.sphereNoLODTemplate_GO).GetComponent<MeshFilter>();
        MeshFilter meshFilter = transform.Find(sphereTemplateCelestBodyName).GetComponent<MeshFilter>();
        meshFilter.mesh = meshFilterToCopy.mesh;
        MeshRenderer meshRenderer = transform.Find(sphereTemplateCelestBodyName).GetComponent<MeshRenderer>();
        meshRenderer.material = settings.sphereTemplateMaterial;
        sphereGO.SetActive(true);
    }

    private void GenerateMesh()
    {
        int idx = 0;
        foreach(TerrainFace face in terrainFaces)
        {
            face.ConstructTree();
            idx += 1;
        }
    }
    //=========================================
    public void InitializeAxialTilt()
    {
        float axialTitleAngle = (float)settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()].value;
        // Rotation vector is in the orbital plane and perpendicular to the radial vector
        Vector3 tangentialVec = (Vector3)orbit.ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.tangentialVec);

        Quaternion tiltRotation = Quaternion.AngleAxis(axialTitleAngle, -tangentialVec);
        gameObject.transform.rotation *= tiltRotation;
    }

    private IEnumerator PlanetGenerationLoop()
    {
        while (true)
        {
            UpdatePlanet();
            yield return new WaitForSeconds(settings.planetRefreshPeriod);
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
        if(spawnAsSimpleSphere)
            return;

        GetDistancesToCamera();
        CheckUpdate_LOD_NonLOD_System();

        if(!UsefulFunctions.DoublesAreEqual(settings.rotationSpeed, 0d))
            RotatePlanet();
    }

    void Update()
    {
        // 'spawnAsSimpleSphere' only refers to the UI SimpleSphere, not the NON-LOD sphere 
        if(spawnAsSimpleSphere) {
            gameObject.transform.rotation = Quaternion.AngleAxis(-0.01f, Vector3.up) * transform.rotation;
            return;
        }
    }

    private void CheckUpdate_LOD_NonLOD_System()
    {
        if(forceNonLODSphere) {
            if(planetGenerationCoroutineIsRunning) {
                SwitchFromLODSystemToNonLODSphere();
            }
            return;
        }
        if(forceLODSphere) {
            if(!planetGenerationCoroutineIsRunning) {
                SwitchFromNonLODSphereToLODSystem();
            }
        }

        string currShipOrbitedBodyName = universeRunner.activeSpaceship.orbitalParams.orbitedBodyName;
        if(currShipOrbitedBodyName.Equals(gameObject.name) && !planetGenerationCoroutineIsRunning)
        {
            SwitchFromNonLODSphereToLODSystem();
        }
        else if(!currShipOrbitedBodyName.Equals(gameObject.name) && planetGenerationCoroutineIsRunning)
        {
            SwitchFromLODSystemToNonLODSphere();
        }
    }

    private void SwitchFromNonLODSphereToLODSystem()
    {
        foreach(Transform child in transform)
        {
            if(child.name.Equals(sphereTemplateCelestBodyName))
            {
                // Disabing the sphere template
                child.gameObject.SetActive(false);
            }
            else {
                // Else enabling the LOD sphere rendering GameObjects
                child.gameObject.SetActive(true);
            }
        }
        planetGenerationCoroutineIsRunning = true;
        StartCoroutine(planetGenerationCoroutine);
    }

    private void SwitchFromLODSystemToNonLODSphere()
    {
        // Stop the LOD update
        if(planetGenerationCoroutineIsRunning) {
            StopCoroutine(planetGenerationCoroutine);
        }
        planetGenerationCoroutineIsRunning = false;

        foreach(Transform child in transform)
        {
            if(child.name.Equals(sphereTemplateCelestBodyName) || child.name.Equals("sunPointLight"))
            {
                // Either enabling the NON-LOD Sphere
                child.gameObject.SetActive(true);
            }
            else {
                // It is a gameobject for the LOD sphere rendering
                // Thus, disabling it as the sphere template is active
                child.gameObject.SetActive(false);
            }
        }
    }

    private void RotatePlanet()
    {
        Vector3 positivePoleVec = (Vector3)settings.equatorialPlane.normal;        
        gameObject.transform.rotation = Quaternion.AngleAxis((float)-settings.rotationSpeed, positivePoleVec) * transform.rotation;  // Sign "-" for the prograde rotation
    }
    //=========================================
    private void AssignRefDictOrbitalParams(Dictionary<string, UnitInterface> refDictOrbParams)
    {
        settings.planetBaseParamsDict = refDictOrbParams;
        if(settings.usePredifinedPlanets)
        {
            // Copying orbital values from the planet dictionary to the orbitalParams scriptable object
            InitializeOrbitalParameters();
        }
        // if 'usePredefinedPlanet' is false, then the orbital values are directly user input in the scriptable object 'orbitalParams' file
    }

    private void InitializeOrbitalParameters()
    {
        CheckOrbitalFileAndBodySettings();
        
        // Called only if 'usePredefinedPlanet' is true
        // Copying orbital values from the planet dictionary to the orbitalParams scriptable object
        _orbitalParams.orbitRealPredType = OrbitalTypes.typeOfOrbit.realOrbit;
        _orbitalParams.orbitDefType = OrbitalTypes.orbitDefinitionType.rarp;
        _orbitalParams.bodyPosType = OrbitalTypes.bodyPositionType.nu;

        if((settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.aphelion.ToString()] as Distance).unit.Equals(Units.distance.km))
            _orbitalParams.orbParamsUnits = OrbitalTypes.orbitalParamsUnits.km_degree;
        else
            _orbitalParams.orbParamsUnits = OrbitalTypes.orbitalParamsUnits.AU_degree;

        _orbitalParams.ra = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.aphelion.ToString()].value;
        _orbitalParams.rp = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.perihelion.ToString()].value;
        _orbitalParams.i = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.i.ToString()].value;
        _orbitalParams.lAscN = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()].value;
        _orbitalParams.omega = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()].value;
        _orbitalParams.nu = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()].value;
    }

    public void InitializeOrbitalPredictor()
    {
        predictor = new OrbitalPredictor(this, orbitalParams.orbitedBody.GetComponent<CelestialBody>(), orbit);
    }

    public void CreateAssignSunPointLight()
    {
        if(UsefulFunctions.CastStringToGoTags(gameObject.tag).Equals(UniverseRunner.goTags.Star))
        {
            GameObject sunPointLightGO = UsefulFunctions.CreateAssignGameObject("sunPointLight", typeof(Light));
            UsefulFunctions.parentObj(sunPointLightGO, gameObject);
            Light sunPointLight = sunPointLightGO.GetComponent<Light>();
            sunPointLight.type = LightType.Point;
            sunPointLight.range = Mathf.Pow(10, 6);
            sunPointLight.color = Color.white;
            sunPointLight.shadows = LightShadows.Soft;
            sunPointLight.cullingMask |= 1 << LayerMask.NameToLayer("Everything");
            sunPointLight.cullingMask &=  ~(1 << LayerMask.NameToLayer("Orbit"));
            //sunPointLight.lightmapBakeType = LightmapBakeType.Baked;
            sunPointLight.intensity = 10f;
        }
    }

    public static bool CelestialBodyHasTagName(CelestialBody body, string tagName)
    {
        if(body.tag == tagName) { return true; }
        else{ return false; }
    }

    public Vector3d GetRelativeRealWorldPosition()
    {
        return new Vector3d(transform.position - orbitalParams.orbitedBody.transform.position) * UniCsts.u2pl;
    }

    public Vector3d GetWorldPositionFromGroundStart(double latitude, double longitude)
    {
        // Latitude and longitude in degrees
        // Substracting 180° to the longitude as the function considers the CelestialBody local +X axis as longitude 0, while longitude 0 is along local axis -X
        Vector3d sphereUnitPos = LaunchPad.LatitudeLongitude_to_3DWorldUNITPoint(latitude-90d, longitude-180d);
        //=======
        double equaRad = settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()].value;
        double polarRad = settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()].value;
        //=======
        double geocentricRad = LaunchPad.ComputeGeocentricRadius(equaRad, polarRad, latitude*UniCsts.deg2rad) * UniCsts.pl2u;
        double additionalHeight = 20d * UniCsts.m2km * UniCsts.pl2u; 
        //Debug.Log("geocentricRad = " + geocentricRad);
        //Debug.Log("sphereUnitPos: " + sphereUnitPos);
        // Direction of the normal to the planet surface of the spawn location (the chosen starting launchPad)
        Vector3d normalDirection = new Quaterniond(transform.rotation) * sphereUnitPos;
        Vector3d worldPos = normalDirection * (geocentricRad + additionalHeight);
        return worldPos;
    }

    public LaunchPad GetDefaultLaunchPad()
    {
        return new LaunchPad(LaunchPadList.GetOriginLaunchPadDict(name));
    }

    public double SetDistanceScaleFactor()
    {
        if(orbitalParams.orbParamsUnits == OrbitalTypes.orbitalParamsUnits.km_degree)
            distanceScaleFactor = UniCsts.m2km2pl2u; // If the orbit is defined in km_degree
        else
            distanceScaleFactor = UniCsts.m2km2au2u; // If the orbit is defined in AU_degree
        return distanceScaleFactor;
    }

    public Vector3d Get_RadialVec()
    {
        // Returns the radial vector, from the orbitedBody centre to this gameObject position, in KM
        if(orbitalParams.orbitedBody == null)
            Debug.LogError("'orbitedBody' is null");

        Vector3d pullinBodyPos = new Vector3d(orbitalParams.orbitedBody.transform.position);
        Vector3d r = new Vector3d(transform.position) - pullinBodyPos;
        double scalingFactor;
        if(orbitalParams.orbParamsUnits.Equals(OrbitalTypes.orbitalParamsUnits.km_degree))
            scalingFactor = UniCsts.u2pl; // km, for planets
        else
            scalingFactor = UniCsts.u2au * UniCsts.au2km;

        r *= scalingFactor; // km
        return r;
    }

    /// <summary>
    /// 'revPeriod' is the revolution period around its Star, must be in days
    /// 'rotPeriod' is the rotation period of the body, in seconds
    /// Returns the angular velocity at which the body is rotating on its orbit, in RAD/s
    /// </summary>
    public static double Get_Body_Angular_Rotation_Velocity(double revPeriod, double rotPeriod)
    {
        return 2d*Mathd.PI * (1 + 1/revPeriod) / (rotPeriod);
    }
}

[System.Serializable]
public struct CelestialBodyPullForce
{
    // Struct used to store the celestialBody and its gravitational attraction force on either a CelestialBody or a Spaceship 
    public CelestialBody celestBody;
    public Vector3d gravForce;
    public CelestialBodyPullForce(CelestialBody _celestBody, Vector3d _gravForce) {
        celestBody = _celestBody;
        gravForce = _gravForce;
    }

    public override string ToString()
    {
        return "[" + celestBody.name + "; " + gravForce.ToString() + "]";
    }
};