using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using System.Text;
using UseFnc = UsefulFunctions;
using Matlab_Communication;

public class Spaceship : MonoBehaviour, FlyingObjCommonParams
{
    [HideInInspector] public SpaceshipController spaceshipController {get; private set;}
    [HideInInspector] public SpaceshipSettings settings;
    //=======
    [HideInInspector] public bool foldoutFlyingObjInfoPanel;
    //=======
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
    void Awake()
    {
        // FOR DEBUG PURPOSES
        if(GameObject.Find("DEBUG") == null) { return; }

        settings = SpaceshipSettingsSaveData.LoadObjectFromJSON(Application.persistentDataPath + Filepaths.shipToLoad_settings);

        DebugGameObject debugGO = GameObject.Find("DEBUG").GetComponent<DebugGameObject>();
        
        GameObject universeGO = GameObject.Find("UniverseRunner");
        if(universeGO != null && orbitalParams == null && !debugGO.loadShipDataFromUIDiskFile)
        {
            // Will load only the orbitalParams from JSON disk File
            orbitalParams = Resources.Load<OrbitalParams>(Filepaths.DEBUG_shipOrbitalParams_0 + gameObject.name + Filepaths.DEBUG_shipOrbitalParams_2);
            if(orbitalParams.orbitedBodyName.Equals("None"))
            {
                orbitalParams.orbitedBody = null;
            }
        }
        else
        {
            // Will load only the orbitalParams from JSON disk File
            DEBUG_LOAD_ORBITALPARAMS_TO_SCRIPTABLE_OBJ();
            orbitalParams.orbitedBody = GameObject.Find(orbitalParams.orbitedBodyName).GetComponent<CelestialBody>();
        }

        if(universeGO != null)
        {
            UniverseRunner universe = universeGO.GetComponent<UniverseRunner>();
            gravPullList = new CelestialBodyPullForce[universe.simEnv.NBODYSIM_NB_BODY.value];
        }
        InitComHandlerTCPReceiver();
    }

    private void InitComHandlerTCPReceiver()
    {
        GameObject comHandlerGO = GameObject.Find("MatlabCom_Handler");
        MatlabComHandler comHandlerInstance = comHandlerGO.GetComponent<MatlabComHandler>();
        if(comHandlerInstance.controlAlgoTCPServer != null)
            spaceshipController = new SpaceshipController(this, comHandlerInstance.controlAlgoTCPServer);
    }

    public void InitializeOrbitalPredictor()
    {
        predictor = new OrbitalPredictor(this, orbitalParams.orbitedBody.GetComponent<CelestialBody>(), orbit);
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

    public double Get_R()
    {
        // Returns the distance in real world (in km) from the spaceship position to the centre of the orbited body
        double distance = Vector3d.Distance(new Vector3d(gameObject.transform.position), new Vector3d(orbitalParams.orbitedBody.transform.position));
        distance *= UniCsts.u2pl; // km
        return distance; // distance in km
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
        return new Vector3d(transform.position - orbitalParams.orbitedBody.transform.position).magnitude;
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
        double latitude = GetProjectedLatitudeFromShipPos(polarRad);
        //=============
        return LaunchPad.ComputeGeocentricRadius(equaRad, polarRad, latitude);
    }

    public Pressure GetOrbitedBodyAtmospherePressure()
    {
        double currAltitude = GetShipAltitude();
        //=============
        Pressure surfacePressure = orbitalParams.orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.biomeParams.surfPressure.ToString()] as Pressure;
        Pressure currentPressure = UniCsts.planetsFncsDict[UseFnc.CastStringTo_Unicsts_Planets(orbitalParams.orbitedBodyName)][PlanetsFunctions.chosenFunction.pressureEvolution](surfacePressure, currAltitude);
        return currentPressure;
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
