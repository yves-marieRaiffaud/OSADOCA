using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mathd_Lib;

public class CelestialBody: MonoBehaviour, FlyingObjCommonParams
{
    [HideInInspector] public UniverseRunner universeRunner; // Assigned in the Awake() function
    //=========================================
    public CelestialBodySettings settings;
    //=========================================
    public GameObject _gameObject { get{return this.gameObject;} set{_gameObject=this.gameObject;} }

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
    //=========================================
    public bool spawnAsSimpleSphere = false; // For the UI, will spawn a simple sphere (no LOD, culling etc.). For the simlation, will spawn the complex one
    //=========================================
    // Bools for editor
    [HideInInspector] public bool showCelestialBodyInfoPanel;
    [HideInInspector] public bool planetEditorFoldout;
    //=========================================
    // Active Camera and distance
    private Transform universePlayerCamera;
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
    public bool addMeshColliders = true;
    [Header("Force LOD/Non-LOD Sphere System")]
    public bool forceNonLODSphere = false;
    public bool forceLODSphere = false;
    //=========================================

    void Awake()
    {
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

        // FOR DEBUG PURPOSES
        if(GameObject.Find("DEBUG") == null) { return; }

        DebugGameObject debugGO = GameObject.Find("DEBUG").GetComponent<DebugGameObject>();
        if(universeRunner != null && orbitalParams == null)
        {
            orbitalParams = Resources.Load<OrbitalParams>(Filepaths.DEBUG_planetOrbitalParams_0 + gameObject.name + Filepaths.DEBUG_planetOrbitalParams_2);
            if(orbitalParams.orbitedBodyName.Equals("None"))
            {
                orbitalParams.orbitedBody = null;
            }
        }
        else
        {
            if(orbitalParams.orbitedBodyName.Equals("None"))
            {
                orbitalParams.orbitedBody = null;
            }
            else {
                string suffix = spawnAsSimpleSphere ? "Planet_UI" : "";
                orbitalParams.orbitedBody = GameObject.Find(orbitalParams.orbitedBodyName+suffix).GetComponent<CelestialBody>();
            }
        }
        
    }

    public void AwakeCelestialBody(Dictionary<string, UnitInterface> refDictOrbParams)
    {
        AssignRefDictOrbitalParams(refDictOrbParams);
        if(spawnAsSimpleSphere) { 
            gameObject.GetComponent<MeshRenderer>().material = settings.sphereTemplateMaterial;
            InitializeBodyParameters();
            return; // Early exit if it's a UI celestialBody
        }

        // Get playerCamera defined in the UniverseRunner Instance
        universePlayerCamera = universeRunner.playerCamera.transform;
        
        InitializeBodyParameters();

        if (gameObject.GetComponent<Presets>() == null)
        {
            Presets presetScript = gameObject.AddComponent<Presets>();
        }

        // InitializeOrbitalParams cannot be here as the CreateComponent function will not be called at this point. Must be called after the Awake() and Start()
        GetDistancesToCamera();
        GeneratePlanet();
        //ApplyFlatenningScale();
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
        distanceToPlayer = Vector3.Distance(transform.position, universePlayerCamera.position);
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
        settings.heightMap = UsefulFunctions.RotateTexture180(settings.heightMap);

        for(int i=0; i<6; i++)
        {
            if(meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject(directionsDict[directionsDict.Keys.ElementAt(i)]);
                meshObj.transform.parent = transform;
                meshObj.transform.position = transform.position;

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = settings.bodyMaterial;
                if(addMeshColliders) {
                    InitMeshColliders(meshObj);
                }
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
                meshFilters[i].sharedMesh.name = directionsDict[directionsDict.Keys.ElementAt(i)].ToString() + "_mesh";
                meshObj.layer = 9;
                meshObj.SetActive(false);
            }
            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, directionsDict.Keys.ElementAt(i), this, settings.heightMap, hasDefaultHeightMap, universeRunner.activeSpaceship.transform, universeRunner.playerCamera.transform);
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
            if(addMeshColliders) {
                MeshCollider mCollider = meshFilters[idx].gameObject.GetComponent<MeshCollider>();
                mCollider.sharedMesh = face.mesh;
            }
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
        // 'spawnAsSimpleSphere' only refers to the UI SimpleSphere, not the NON-LOD sphere 
        if(spawnAsSimpleSphere) { return; }
        GetDistancesToCamera();
        CheckUpdate_LOD_NonLOD_System();

        if(!UsefulFunctions.DoublesAreEqual(settings.rotationSpeed, 0d))
        {
            RotatePlanet();
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
        // Called only if 'usePredefinedPlanet' is true
        // Copying orbital values from the planet dictionary to the orbitalParams scriptable object
        orbitalParams.orbitRealPredType = OrbitalTypes.typeOfOrbit.realOrbit;
        orbitalParams.orbitDefType = OrbitalTypes.orbitDefinitionType.rarp;
        orbitalParams.orbParamsUnits = OrbitalTypes.orbitalParamsUnits.AU_degree;
        orbitalParams.bodyPosType = OrbitalTypes.bodyPositionType.nu;

        orbitalParams.ra = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.aphelion.ToString()].value;
        orbitalParams.rp = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.perihelion.ToString()].value;
        orbitalParams.i = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.i.ToString()].value;
        orbitalParams.lAscN = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()].value;
        orbitalParams.omega = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()].value;
        orbitalParams.nu = settings.planetBaseParamsDict[CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()].value;
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
            sunPointLight.range = Mathf.Pow(10, 5);
            sunPointLight.color = Color.white;
            sunPointLight.shadows = LightShadows.Soft;
            sunPointLight.cullingMask |= 1 << LayerMask.NameToLayer("Everything");
            sunPointLight.cullingMask &=  ~(1 << LayerMask.NameToLayer("Orbit"));
            sunPointLight.lightmapBakeType = LightmapBakeType.Baked;
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

    public Vector3d GetRelativeVelocity()
    {
        return orbitedBodyRelativeVel;
    }

    public double GetRelativeVelocityMagnitude()
    {
        return orbitedBodyRelativeVel.magnitude;
    }

    public Vector3d GetWorldPositionFromGroundStart(double latitude, double longitude)
    {
        // Substracting 180° to the longitude as the function considers the CelestialBody local +X axis as longitude 0, while longitude 0 is along local axis -X
        Vector3d sphereUnitPos = LaunchPad.LatitudeLongitude_to_3DWorldUNITPoint(latitude, longitude-180d);
        //=======
        double equaRad = settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()].value;
        double polarRad = settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()].value;
        //=======
        double geocentricRad = LaunchPad.ComputeGeocentricRadius(equaRad, polarRad, latitude) * UniCsts.pl2u;
        Vector3d worldPos = new Quaterniond(transform.rotation) * (sphereUnitPos * geocentricRad);
        return worldPos;
    }

    public void InitMeshColliders(GameObject faceGO)
    {
        MeshCollider meshCollider = (MeshCollider)UsefulFunctions.CreateAssignComponent(typeof(MeshCollider), faceGO);
        meshCollider.convex = true;

        PhysicMaterial sp_c_Material = new PhysicMaterial();
        sp_c_Material.bounciness = 0f;
        sp_c_Material.dynamicFriction = 1f;
        sp_c_Material.staticFriction = 1f;
        sp_c_Material.frictionCombine = PhysicMaterialCombine.Average;
        sp_c_Material.bounceCombine = PhysicMaterialCombine.Average;
        meshCollider.material = sp_c_Material;
    }

    public double SetDistanceScaleFactor()
    {
        if(orbitalParams.orbParamsUnits == OrbitalTypes.orbitalParamsUnits.km_degree)
        {
            distanceScaleFactor = UniCsts.m2km2pl2u; // If the orbit is defined in km_degree
        }
        else {
            distanceScaleFactor = UniCsts.m2km2au2u; // If the orbit is defined in AU_degree
        }
        return distanceScaleFactor;
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