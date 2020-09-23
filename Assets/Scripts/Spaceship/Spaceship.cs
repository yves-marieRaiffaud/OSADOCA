using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using System.Text;
using UnityEngine.SceneManagement;
using UseFnc = UsefulFunctions;
using Matlab_Communication;

public class Spaceship : MonoBehaviour, FlyingObjCommonParams
{
    public bool spawnAsUIRocket; 
    [HideInInspector] public PropulsionManager propulsionManager;
    [HideInInspector] public SpaceshipController spaceshipController {get; private set;}
    [HideInInspector] public SpaceshipSettings settings;
    [HideInInspector] Rigidbody rb;
    //=======
    [HideInInspector] public bool foldoutFlyingObjInfoPanel;
    //=======
    [HideInInspector] public double µ;

    public GameObject _gameObject
    { 
        get {
            return this.gameObject;
        }
        set {
            _gameObject=this.gameObject;
        }
    }
    
    [HideInInspector]
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

    [HideInInspector]
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
    private Vector3d _realPosition;
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
    private Vector3d _orbitedBodyRelativeAcc;
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
    private Vector3d _orbitedBodyRelativeVelIncr;
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
    private Vector3d _orbitedBodyRelativeVel;
    public Vector3d orbitedBodyRelativeVel
    {
        get {
            return _orbitedBodyRelativeVel;
        }
        set {
            _orbitedBodyRelativeVel=value;
        }
    }
    
    public Vector3d absoluteVelocity
    {
        get {
            Vector3d speedOfOrbitedBody = Vector3d.zero;
            if(!orbitalParams.orbitedBodyName.Equals("None")) {
                speedOfOrbitedBody = orbitalParams.orbitedBody.orbitedBodyRelativeVel;
            }
            return orbitedBodyRelativeVel + speedOfOrbitedBody;
        }
    }
    public Vector3d absoluteVelocityUnityScaled
    {
        get {
            Vector3d speedOfOrbitedBody = Vector3d.zero;
            double orbitedBodySF = 0f; // Scale factor
            if(!orbitalParams.orbitedBodyName.Equals("None")) {
                speedOfOrbitedBody = orbitalParams.orbitedBody.orbitedBodyRelativeVel;
                orbitedBodySF = orbitalParams.orbitedBody.distanceScaleFactor;
            }
            return orbitedBodyRelativeVel*distanceScaleFactor + speedOfOrbitedBody*orbitedBodySF;
        }
    }

    private CelestialBodyPullForce[] _gravPullList;
    public CelestialBodyPullForce[] gravPullList
    {
        get {
            return _gravPullList;
        }
        set {
            _gravPullList=value;
        }
    }
    //===============================
    private Quaterniond previousRotation;
    private Quaterniond _deltaRotation;
    public Quaterniond deltaRotation
    {
        get {
            return _deltaRotation;
        }
    }
    //===============================
    [HideInInspector] public bool shipRBConstraints_areOn;
    [HideInInspector] private bool orbitsAreDisplayed;
    [HideInInspector] public Camera mainCamera;
    [HideInInspector] public Camera cameraBack;
    //===============================
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

    void FixedUpdate()
    {
        GetDeltaRotation();
        if(spaceshipController == null)
        {
            // Check if there has been a connection sending nozzle control data
            InitComHandlerTCPReceiver();
        }

        if(Input.GetKey(KeyCode.Space))
        {
            rb.constraints = RigidbodyConstraints.None;
            shipRBConstraints_areOn = false;
        }
    }
    //===============================
    //===============================
    void Awake()
    {
        settings = SpaceshipSettingsSaveData.LoadObjectFromJSON(Application.persistentDataPath + Filepaths.shipToLoad_settings);
        µ = UniCsts.G * settings.mass; // e^-11
        // Reading & Copying the JSON files to the right Scriptable Objects of the spaceship
        orbitalParams = OrbitalParamsSaveData.LoadObjectFromJSON(Application.persistentDataPath + Filepaths.shipToLoad_orbitalParams);

        propulsionManager = GetComponent<PropulsionManager>();
        spaceshipController = null;
        previousRotation = new Quaterniond(transform.rotation);
        orbitsAreDisplayed = false;
        rb = GetComponent<Rigidbody>();

        if(transform.Find("Main Camera") != null)
            mainCamera = transform.Find("Main Camera").GetComponent<Camera>();
        else
            Debug.LogWarning("'MainCamera' Camera component has not been found");

        if(transform.Find("CameraBack") != null)
            cameraBack = transform.Find("CameraBack").GetComponent<Camera>();
        else
            Debug.LogWarning("'CameraBack' Camera component has not been found");

        if(spawnAsUIRocket)
        {
            transform.Find("Main Camera").gameObject.SetActive(false);
            transform.Find("CameraBack").gameObject.SetActive(false);
            return;
        }

        shipRBConstraints_areOn = true;
        //==================================================
        GameObject universeGO = GameObject.Find("UniverseRunner");
        if(universeGO != null)
        {
            UniverseRunner universe = universeGO.GetComponent<UniverseRunner>();
            gravPullList = new CelestialBodyPullForce[universe.simEnv.NBODYSIM_NB_BODY.value];
        }
        InitComHandlerTCPReceiver();
    }
    //===============================
    //===============================
    private void InitComHandlerTCPReceiver()
    {
        GameObject comHandlerGO = GameObject.Find("MatlabCom_Handler");
        MatlabComHandler comHandlerInstance = comHandlerGO.GetComponent<MatlabComHandler>();
        if(comHandlerInstance.controlAlgoTCPServer != null)
            spaceshipController = new SpaceshipController(this, comHandlerInstance.controlAlgoTCPServer);
    }
    //===============================
    //===============================
    public Vector3d GetRelativeRealWorldPosition()
    {
        return new Vector3d(transform.position - orbitalParams.orbitedBody.transform.position) * UniCsts.u2pl;
    }

    public void DEBUG_LOAD_ORBITALPARAMS_TO_SCRIPTABLE_OBJ()
    {
        if(GameObject.Find("DEBUG") != null)
        {
            DebugGameObject scriptInstance = GameObject.Find("DEBUG").GetComponent<DebugGameObject>();
            bool needToLoadJSONFiles = scriptInstance.loadShipDataFromUIDiskFile;
            if(needToLoadJSONFiles)
            {
                // Reading & Copying the JSON files to the right Scriptable Objects of the spaceship
                orbitalParams = OrbitalParamsSaveData.LoadObjectFromJSON(Application.persistentDataPath + Filepaths.shipToLoad_orbitalParams);
            }
        }
        // don't need to do anything if we want to use the Scriptables objects set up in the Unity Editor Inspector
    }
    //===============================
    //===============================
    public Vector3d Get_RadialVec()
    {
        // Returns the radial vector, from the orbitedBody centre to this gameObject position, in KM
        if(orbitalParams.orbitedBody == null)
            Debug.LogError("'orbitedBody' is null");

        Vector3d pullinBodyPos = new Vector3d(orbitalParams.orbitedBody.transform.position);
        Vector3d r = new Vector3d(transform.position) - pullinBodyPos;
        double scalingFactor = UniCsts.u2au * UniCsts.au2km; // km, for planets

        if(orbitalParams.orbParamsUnits == OrbitalTypes.orbitalParamsUnits.km_degree &&
            orbitalParams.orbitedBodyName == orbitalParams.orbitedBody.name)
        {
            scalingFactor = UniCsts.u2pl; // km, for spaceships or interplanetary orbits of spaceships
        }

        r *= scalingFactor; // km
        return r;
    }
    
    public double Get_R()
    {
        // Returns the distance in real world (in km) from the spaceship position to the centre of the orbited body
        return Get_R(new Vector3d(gameObject.transform.position));
    }

    public double Get_R(Vector3d shipPosition)
    {
        // Returns the distance in real world (in km) from the spaceship position to the centre of the orbited body
        double distance = Vector3d.Distance(shipPosition, new Vector3d(orbitalParams.orbitedBody.transform.position));
        distance *= UniCsts.u2pl; // Either km or AU
        return distance; // distance in km
    }

    public double GetPivotPointAltitude()
    {
        // Returns the altitude in real world (in km) from the spaceship position to the surface of the orbited body
        return Get_R() - orbitalParams.orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()].value;
    }

    public double GetPivotPointAltitude(Vector3d shipPosition)
    {
        // Returns the altitude in real world (in km) from the spaceship position to the surface of the orbited body
        return Get_R(shipPosition) - orbitalParams.orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()].value;
    }

    public double GetUnityAltitude(Vector3d shipPosition)
    {
        return (shipPosition - new Vector3d(orbitalParams.orbitedBody.transform.position)).magnitude;
    }

    public double GetUnityAltitude()
    {
        return GetUnityAltitude(new Vector3d(transform.position));
    }

    public double GetShipAltitude()
    {
        // Returns the altitude in km of the SHIP's CENTER POINT
        //==============================================
        double pivotAltitude = GetUnityAltitude();
        //=======
        BoxCollider shipCollider = gameObject.GetComponent<BoxCollider>();
        Vector3 centerPos = shipCollider.bounds.center;
        double centerAltitude = GetUnityAltitude(new Vector3d(centerPos));
        //=======
        double geocentricRad = GetGeocentricRadiusFromShipPos();
        double altitudeVal = (centerAltitude*UniCsts.u2pl + (pivotAltitude - centerAltitude)*UniCsts.u2sh) - geocentricRad;
        //=======
        return altitudeVal;
    }

    public double GetProjectedLatitudeFromShipPos(double polarRadius)
    {
        // Returns the calculated/estimated latitude in degree
        Vector3d shipPosInPlanetAxis = new Vector3d(orbitalParams.orbitedBody.transform.InverseTransformPoint(transform.position));
        //=====
        double ratioAboveNullLong = shipPosInPlanetAxis.y * UniCsts.u2pl / polarRadius;
        ratioAboveNullLong = Mathd.Clamp(ratioAboveNullLong, -1d, 1d);
        return 90d - Mathd.Acos(ratioAboveNullLong)*UniCsts.rad2deg;
    }

    public double GetGeocentricRadiusFromShipPos()
    {
        // Returns the Geocentric Radius in km of the current projected position of the spaceship on the CelestialBody it is orbiting 
        double equaRad = orbitalParams.orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()].value;
        double polarRad = orbitalParams.orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()].value;
        double latitude = GetProjectedLatitudeFromShipPos(polarRad); // in degree
        //=============
        return LaunchPad.ComputeGeocentricRadius(equaRad, polarRad, latitude*UniCsts.deg2rad);
    }
    //===============================
    //===============================
    public static double GetProjectedLatitudeFromShipPos(double polarRadius, CelestialBody body, Vector3 shipPos)
    {
        // Returns the calculated/estimated latitude in degree
        Vector3d shipPosInPlanetAxis = new Vector3d(body.transform.InverseTransformPoint(shipPos));
        //=====
        double ratioAboveNullLong = shipPosInPlanetAxis.y * UniCsts.u2pl / polarRadius;
        ratioAboveNullLong = Mathd.Clamp(ratioAboveNullLong, -1d, 1d);
        return 90d - Mathd.Acos(ratioAboveNullLong)*UniCsts.rad2deg;
    }

    public static double GetGeocentricRadiusFromShipPos(CelestialBody body, Vector3 shipPos)
    {
        // Returns the Geocentric Radius in km of the current projected position of the spaceship on the CelestialBody it is orbiting 
        double equaRad = body.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()].value;
        double polarRad = body.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()].value;
        double latitude = GetProjectedLatitudeFromShipPos(polarRad, body, shipPos);
        //=============
        return LaunchPad.ComputeGeocentricRadius(equaRad, polarRad, latitude*UniCsts.deg2rad);
    }

    public Vector3d GetEasternDirection(double longitude)
    {
        // Returns a vector3d of the normalized eastern direction at equator latitude at the passed longitude
        // The returned vector3d is in WORLD SPACE (the direction vector is in world space)
        // To be multiplied by the spaceship launchPad's eastward boost to init its ground velocity
        Quaterniond planetRot = new Quaterniond(orbitalParams.orbitedBody.transform.rotation);
        Vector3d spherePos = (planetRot*LaunchPad.LatitudeLongitude_to_3DWorldUNITPoint(0d-90d, longitude-180d)).normalized;
        Vector3d localUp = new Vector3d(orbitalParams.orbitedBody.transform.TransformDirection(Vector3.up)).normalized;
        return Vector3d.Cross(spherePos, localUp).normalized;
    }

    public Pressure GetOrbitedBodyAtmospherePressure()
    {
        double currAltitude = GetShipAltitude();
        //=============
        Pressure surfacePressure = orbitalParams.orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.biomeParams.surfPressure.ToString()] as Pressure;
        Pressure currentPressure = UniCsts.planetsFncsDict[UseFnc.CastStringTo_Unicsts_Planets(orbitalParams.orbitedBodyName)][PlanetsFunctions.chosenFunction.pressureEvolution](surfacePressure, currAltitude);
        return currentPressure;
    }
    //===============================
    //===============================
    public double SetDistanceScaleFactor()
    {
        if(orbitalParams.orbParamsUnits == OrbitalTypes.orbitalParamsUnits.km_degree)
            distanceScaleFactor = UniCsts.m2km2pl2u; // If the orbit is defined in km_degree
        else
            distanceScaleFactor = UniCsts.m2km2au2u; // If the orbit is defined in AU_degree
        return distanceScaleFactor;
    }

    public Quaterniond GetDeltaRotation()
    {
        // Returns the increment of rotation of the rocket
        // Simulates what the gyrscopes of the IMU would measure
        //_deltaRotation = new Quaterniond(transform.rotation) - previousRotation;
        _deltaRotation = new Quaterniond(transform.rotation) * Quaterniond.Conjugate(previousRotation);
        previousRotation = new Quaterniond(transform.rotation);
        return _deltaRotation;
    }

    public IEnumerator DeltaRotation_Coroutine(float comHandlerSenderSimEnvFrequency)
    {
        while (true)
        {
            GetDeltaRotation();
            yield return new WaitForSeconds(comHandlerSenderSimEnvFrequency);
        }
    }

    public void UnlockRigibodyROtations()
    {
        rb.constraints = RigidbodyConstraints.None;
    }
}
