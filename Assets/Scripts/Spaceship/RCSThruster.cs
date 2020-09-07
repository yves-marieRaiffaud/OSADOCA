using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using System.Text;
using UnityEngine.Events;
using UseFnc = UsefulFunctions;

public class RCSThruster : MonoBehaviour, PropulsionInterface
{
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

    /// <summary>
    /// Vector; in WORLD SPACE; pointing toward the centerline axis of the rocket cylinder == colinear with the same direction of the inward radial axis == along +X-Axis
    /// </summary>
    public Vector3 worldRightThrustAxis {
        get {
            return transform.TransformDirection(Vector3.right);
        }
    }
    /// <summary>
    /// Vector; in WORLD SPACE; pointing upward == along the positive longitudinal axis of the rocket == along +Y-Axis
    /// </summary>
    public Vector3 worldUpThrustAxis {
        get {
            return transform.TransformDirection(Vector3.up);
        }
    }
    /// <summary>
    /// Vector; in WORLD SPACE; pointing toward the centerline axis of the rocket cylinder == colinear with the same direction of the inward radial axis == along +Z-Axis
    /// </summary>
    public Vector3 worldForwardThrustAxis {
        get {
            return transform.TransformDirection(Vector3.forward);
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


    void Start()
    {
        if(effectiveRB == null)
            Debug.LogWarningFormat("The specified Rigidbody 'effectiveRB' is null. Thrust for {0} is thus disabled.", name);
        
        thrustVectorLR_GO = UseFnc.CreateAssignGameObject("ThrustVector_LR", gameObject, typeof(LineRenderer), false);
        thrustVectorLR_GO.transform.localPosition = Vector3.zero;
        thrustVectorLR_GO.tag = "SpaceshipVectors";
        thrustVectorLR_GO.layer = Filepaths.engineLayersToInt[Filepaths.EngineLayersName.SpaceshipVectors];
        thrustVectorLR = thrustVectorLR_GO.GetComponent<LineRenderer>();
        thrustVectorLR.useWorldSpace = true; // Using worldSpace as we are passing the 'world****ThrustAxis' Vector3 to the 'DrawThrustVector' method
        thrustVectorLR.sharedMaterial = Resources.Load("OrbitMaterial", typeof(Material)) as Material; // SHOULD BE CHANGED

        if(_thrustIsOn == null)
            _thrustIsOn = new ThrustIsOnEvent();
    }

    public void FireRCS(RCSThrustOrientation orientation, RCSThrustSense sense)
    {
        if(effectiveRB == null) {
            Debug.LogWarningFormat("The specified Rigidbody 'effectiveRB' is null. Thrust for {0} is thus disabled.", name);
            return;
        }
        
        thrust = RCSThrustPower * VectorFromOrientation(orientation) * ValueFromThrustSense(sense);
        effectiveRB.AddForceAtPosition(thrust, worldPosition, ForceMode.Force);
    }

    void LateUpdate()
    {
        if((thrust.x != 0f || thrust.y != 0f || thrust.z != 0f) && thrust.magnitude != 0f) {
            if(displayThrustVector && !thrustVectorLR_GO.activeSelf)
                thrustVectorLR_GO.SetActive(true);
            else if(!displayThrustAxes && thrustVectorLR_GO.activeSelf)
                thrustVectorLR_GO.SetActive(false);
                
            if(_thrustIsOn != null)
                _thrustIsOn.Invoke(this as PropulsionInterface);
        }
    }

    private Vector3 VectorFromOrientation(RCSThrustOrientation orientation)
    {
        switch(orientation) {
            case RCSThrustOrientation.right:
                return worldRightThrustAxis;

            case RCSThrustOrientation.up:
                return worldUpThrustAxis;
            
            case RCSThrustOrientation.forward:
                return worldForwardThrustAxis;

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


    public void DrawThrustVector()
    {
        //Debug.Log("thrust.magnitude = " + thrust.magnitude);
        UsefulFunctions.DrawVector(thrustVectorLR_GO, thrustVectorLR, thrust, effectiveRB.transform.position, thrust.magnitude);
    }

}