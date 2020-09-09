using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using System.Text;
using UnityEngine.Events;
using UseFnc = UsefulFunctions;

public class RCSThruster : MonoBehaviour, PropulsionInterface
{
    public enum SideLocation { FrontPositive, FrontNegative, RightPositive, RightNegative };
    public SideLocation sideLocation;
    //======================
    //======================

    public float _RCSThrustPower; // public to be user-modified
    public float RCSThrustPower {
        get {
            return _RCSThrustPower;
        }
        set {
            _RCSThrustPower=value;
        }
    }

    [Tooltip("Rigidbody on which to apply the RCS thrust")]
    public Rigidbody _effectiveRB;
    public Rigidbody effectiveRB {
        get {
            return _effectiveRB;
        }
        set {
            _effectiveRB=value;
        }
    }

    [Header("Vectors to draw")]
    bool _displayThrustVector;
    public bool displayThrustVector {
        get {
            return _displayThrustVector;
        }
        set {
            _displayThrustVector=value;
        }
    }
    
    bool _displayThrustAxes;
    public bool displayThrustAxes {
        get {
            return _displayThrustAxes;
        }
        set {
            _displayThrustAxes=value;
        }
    }

    GameObject thrustVectorLR_GO;
    LineRenderer thrustVectorLR;

    /// <summary>
    /// Position of the thrust from this RCS thruster in World space
    /// </summary>
    public Vector3 worldPosition {
        get {
            return transform.position;
        }
    }
    /// <summary>
    /// Position of the thrust from this RCS thruster relative to the rigidbody transform it is applying force on
    /// </summary>
    public Vector3 localPosition {
        get {
            return effectiveRB.transform.InverseTransformPoint(transform.position);
        }
    }

    ThrustAxes _thrustAxes;
    public ThrustAxes thrustAxes {
        get {
            return _thrustAxes;
        }
    }

    // Orientation and Sense form together the thrust's direction
    // enums directly mapping to their corresponding direction: respectively 'worldLocalRight', 'worldLocalUp', 'worldLocalForward'
    public enum RCSThrustOrientation { right, up, forward, NULL };
    public enum RCSThrustSense { positive, negative, NULL };
    [HideInInspector] public Vector3 thrust;

    ThrustIsOnEvent _thrustIsOn;
    /// <summary>
    /// Event fired at LateUpdate every time the thruster is applying a non-null force on the rigidbody 'effectiveRB'. Passed argument to this event is an instance of the 'PropulsionInterface'
    /// </summary>
    public ThrustIsOnEvent thrustIsOn
    {
        get {
            return _thrustIsOn;
        }
        set {
            _thrustIsOn=value;
        }
    }

    void Awake()
    {
        if(_thrustIsOn == null)
            _thrustIsOn = new ThrustIsOnEvent();
    }

    void InitThrusterSideLocation()
    {
        // Comparing the Rocket local axes with the RCS thruster local axes
        // If the rocket local X-axis is colinear & same sense with the RCS thruster local X-axis ==> SideLocation == FrontPositive
            // Else if local X-axis is colinear with opposite sense ==> SideLocation == FrontNegative
        // If the rocket local Z-axis is colinear & same sense with the RCS thruster local X-axis ==> SideLocation == RightPositive
            // Else if local Z-axis is colinear & opposite sense ==> SideLocation == RightNegative
        Vector3 trLocalX = transform.TransformDirection(Vector3.right);
        //=======
        Vector3 rocketLocalX = effectiveRB.transform.TransformDirection(Vector3.right);
        float dotX = Vector3.Dot(trLocalX, rocketLocalX);
        if(UseFnc.FloatsAreEqual(dotX, 1f)) {
            sideLocation = SideLocation.FrontPositive;
            return;
        }
        else if(UseFnc.FloatsAreEqual(dotX, -1f)) {
            sideLocation = SideLocation.FrontNegative;
            return;
        }

        Vector3 rocketLocalZ = effectiveRB.transform.TransformDirection(Vector3.forward);
        float dotZ = Vector3.Dot(trLocalX, rocketLocalZ);
        if(UseFnc.FloatsAreEqual(dotZ, 1f)) {
            sideLocation = SideLocation.RightPositive;
            return;
        }
        else if(UseFnc.FloatsAreEqual(dotZ, -1f)) {
            sideLocation = SideLocation.RightNegative;
            return;
        }
    }

    void Start()
    {
        if(effectiveRB == null)
            Debug.LogWarningFormat("The specified Rigidbody 'effectiveRB' is null. Thrust for {0} is thus disabled.", name);
        
        InitThrusterSideLocation();
        _thrustAxes = new ThrustAxes(transform, sideLocation);
        
        thrustVectorLR_GO = UseFnc.CreateAssignGameObject("ThrustVector_LR", gameObject, typeof(LineRenderer), false);
        thrustVectorLR_GO.transform.localPosition = Vector3.zero;
        thrustVectorLR_GO.tag = "SpaceshipVectors";
        thrustVectorLR_GO.layer = Filepaths.engineLayersToInt[Filepaths.EngineLayersName.SpaceshipVectors];
        thrustVectorLR = thrustVectorLR_GO.GetComponent<LineRenderer>();
        thrustVectorLR.useWorldSpace = true; // Using worldSpace as we are passing the 'world****ThrustAxis' Vector3 to the 'DrawThrustVector' method
        thrustVectorLR.sharedMaterial = Resources.Load("OrbitMaterial", typeof(Material)) as Material; // SHOULD BE CHANGED
    }

    public void FireRCS(RCSThrustOrientation orientation, RCSThrustSense sense)
    {
        if(effectiveRB == null) {
            Debug.LogWarningFormat("The specified Rigidbody 'effectiveRB' is null. Thrust for {0} is thus disabled.", name);
            return;
        }
        
        thrust = RCSThrustPower * VectorFromOrientation(orientation) * ValueFromThrustSense(sense);
        Debug.Log("thrust = " + thrust);
        effectiveRB.AddForceAtPosition(thrust, worldPosition, ForceMode.Force);
    }

    void LateUpdate()
    {
        /*if((thrust.x != 0f || thrust.y != 0f || thrust.z != 0f) && thrust.magnitude != 0f) {
            if(displayThrustVector && !thrustVectorLR_GO.activeSelf)
                thrustVectorLR_GO.SetActive(true);
            else if(!displayThrustAxes && thrustVectorLR_GO.activeSelf)
                thrustVectorLR_GO.SetActive(false);
                
            if(_thrustIsOn != null)
                _thrustIsOn.Invoke(this as PropulsionInterface);
        }*/
    }

    private Vector3 VectorFromOrientation(RCSThrustOrientation orientation)
    {
        float mul = (sideLocation.Equals(SideLocation.FrontNegative) || sideLocation.Equals(SideLocation.RightNegative)) ? -1f : 1f;
        switch(orientation) {
            case RCSThrustOrientation.right:
                return mul * thrustAxes.worldRightThrustAxis;

            case RCSThrustOrientation.up:
                return thrustAxes.worldUpThrustAxis;
            
            case RCSThrustOrientation.forward:
                return mul * thrustAxes.worldForwardThrustAxis;

            case RCSThrustOrientation.NULL:
                return Vector3.zero;
            
            default:
                return Vector3.zero;
        }
    }

    private float ValueFromThrustSense(RCSThrustSense sense)
    {
        switch(sense) {
            case RCSThrustSense.positive:
                return 1f;

            case RCSThrustSense.negative:
                return -1f;
            
            case RCSThrustSense.NULL:
                return 0f;

            default:
                return 0f;
        }
    }

    public bool ThrustAxis_AlignedWith_Direction(Vector3 directionToCheck)
    {
        Debug.Log(name + ":PossibleLocalThrustAxRocketFrame: " + string.Join("\n", thrustAxes.PossibleLocalThrustAxRocketFrame()));
        foreach(Vector3 thrustAxis in thrustAxes.PossibleLocalThrustAxRocketFrame()) {
            Debug.Log("directionToCheck: " + directionToCheck + "; thrustAxis: " + thrustAxis + "; dot" + (Vector3.Dot(directionToCheck, thrustAxis)));
            if(UseFnc.FloatsAreEqual(Vector3.Dot(directionToCheck, thrustAxis), 1f, 0.01f))
                return true;
        }
        return false;
    }


    public void DrawThrustVector()
    {
        UsefulFunctions.DrawVector(thrustVectorLR_GO, thrustVectorLR, thrust, effectiveRB.transform.position, thrust.magnitude);
    }

}

public struct ThrustAxes {
    Transform transform;
    RCSThruster.SideLocation sideLocation;
    /// <summary>
    /// Vector; in WORLD SPACE; pointing toward the centerline axis of the rocket cylinder == colinear with the same direction of the inward radial axis == along +X-Axis
    /// </summary>
    public Vector3 worldRightThrustAxis {
        get {
            return transform.TransformDirection(localRightThrustAxis);
        }
    }
    /// <summary>
    /// Vector; in WORLD SPACE; pointing upward == along the positive longitudinal axis of the rocket == along +Y-Axis
    /// </summary>
    public Vector3 worldUpThrustAxis {
        get {
            return transform.TransformDirection(localUpThrustAxis);
        }
    }
    /// <summary>
    /// Vector; in WORLD SPACE; pointing toward the centerline axis of the rocket cylinder == colinear with the same direction of the inward radial axis == along +Z-Axis
    /// </summary>
    public Vector3 worldForwardThrustAxis {
        get {
            return transform.TransformDirection(localForwardThrustAxis);
        }
    }

    /// <summary>
    /// Vector; in LOCAL SPACE; pointing toward the centerline axis of the rocket cylinder == colinear with the same direction of the inward radial axis == along +X-Axis
    /// </summary>
    public Vector3 localRightThrustAxis {
        get {
            return Vector3.right;
        }
    }
    /// <summary>
    /// Vector; in LOCAL SPACE; pointing upward == along the positive longitudinal axis of the rocket == along +Y-Axis
    /// </summary>
    public Vector3 localUpThrustAxis {
        get {
            return Vector3.up;
        }
    }
    /// <summary>
    /// Vector; in LOCAL SPACE; pointing toward the centerline axis of the rocket cylinder == colinear with the same direction of the inward radial axis == along +Z-Axis
    /// </summary>
    public Vector3 localForwardThrustAxis {
        get {
            return Vector3.forward;
        }
    }

    public ThrustAxes(Transform _transform, RCSThruster.SideLocation _sideLocation) {
        transform = _transform;
        sideLocation = _sideLocation;
    }

    public Vector3[] LocalThrustAxes() {
        Vector3[] localTRAxes = new Vector3[3];
        localTRAxes[0] = localRightThrustAxis;
        localTRAxes[1] = localUpThrustAxis;
        localTRAxes[2] = localForwardThrustAxis;
        return localTRAxes;
    }

    public Vector3[] WorldThrustAxes() {
        Vector3[] worldTRAxes = new Vector3[3];
        worldTRAxes[0] = worldRightThrustAxis;
        worldTRAxes[1] = worldUpThrustAxis;
        worldTRAxes[2] = worldForwardThrustAxis;
        return worldTRAxes;
    }

    public Vector3[] PossibleLocalThrustAxes() {
        Vector3[] localTRAxes = new Vector3[5];
        localTRAxes[0] = -localRightThrustAxis;
        localTRAxes[1] = -localUpThrustAxis;
        localTRAxes[2] = localUpThrustAxis;
        localTRAxes[3] = -localForwardThrustAxis;
        localTRAxes[4] = localForwardThrustAxis;
        return localTRAxes;
    }

    public Vector3[] PossibleLocalThrustAxRocketFrame() {
        Vector3[] localTRAxes = new Vector3[5];
        // The hereunder vectors are expressed in the rocket frame
        switch(sideLocation)
        {
            case RCSThruster.SideLocation.FrontPositive:
                localTRAxes[0] = Vector3.left;
                localTRAxes[1] = Vector3.up;
                localTRAxes[2] = Vector3.down;
                localTRAxes[3] = Vector3.back;
                localTRAxes[4] = Vector3.forward;
                break;
            case RCSThruster.SideLocation.FrontNegative:
                localTRAxes[0] = Vector3.right;
                localTRAxes[1] = Vector3.up;
                localTRAxes[2] = Vector3.down;
                localTRAxes[3] = Vector3.forward;
                localTRAxes[4] = Vector3.back;
                break;
            case RCSThruster.SideLocation.RightPositive:
                localTRAxes[0] = Vector3.back;
                localTRAxes[1] = Vector3.up;
                localTRAxes[2] = Vector3.down;
                localTRAxes[3] = Vector3.left;
                localTRAxes[4] = Vector3.right;
                break;
            case RCSThruster.SideLocation.RightNegative:
                localTRAxes[0] = Vector3.forward;
                localTRAxes[1] = Vector3.up;
                localTRAxes[2] = Vector3.down;
                localTRAxes[3] = Vector3.right;
                localTRAxes[4] = Vector3.left;
                break;
        }
        return localTRAxes;
    }

    public Vector3[] PossibleWorldThrustAxes() {
        Vector3[] localTRAxes = new Vector3[5];
        localTRAxes[0] = -worldRightThrustAxis;
        localTRAxes[1] = -worldUpThrustAxis;
        localTRAxes[2] = worldUpThrustAxis;
        localTRAxes[3] = -worldForwardThrustAxis;
        localTRAxes[4] = worldForwardThrustAxis;
        return localTRAxes;
    }
};