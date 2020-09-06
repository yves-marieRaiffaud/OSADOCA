using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using System.Text;
using UseFnc = UsefulFunctions;

public class RCSThruster : MonoBehaviour
{
    public float RCSThrustPower;
    [Tooltip("Rigidbody on which to apply the RCS thrust")]
    public Rigidbody effectiveRB;

    [Header("Vectors to draw")]
    public bool displayThrustVector;
    public bool displayThrustAxes;
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


    // Vector pointing toward the centerline axis of the rocket cylinder == colinear with the same direction of the inward radial axis == along +X-Axis
    public Vector3 worldLocalRight {
        get {
            return transform.TransformDirection(Vector3.right);
        }
    }
    // Vector pointing upward == along the positive longitudinal axis of the rocket == along +Y-Axis
    public Vector3 worldLocalUp {
        get {
            return transform.TransformDirection(Vector3.up);
        }
    }
    // Vector pointing toward the centerline axis of the rocket cylinder == colinear with the same direction of the inward radial axis == along +Z-Axis
    public Vector3 worldLocalForward {
        get {
            return transform.TransformDirection(Vector3.forward);
        }
    }

    // Orientation and Sense form together the thrust's direction
    // enums directly mapping to their corresponding direction: respectively 'worldLocalRight', 'worldLocalUp', 'worldLocalForward'
    public enum RCSThrustOrientation { right, up, forward, NULL };
    public enum RCSThrustSense { positive, negative, NULL };

    void Start()
    {
        if(effectiveRB == null)
            Debug.LogWarningFormat("The specified Rigidbody 'effectiveRB' is null. Thrust for {0} is thus disabled.", name);
        
        thrustVectorLR_GO = UseFnc.CreateAssignGameObject("ThrustVector_LR", gameObject, typeof(LineRenderer));
        thrustVectorLR_GO.layer = Filepaths.engineLayersToInt[Filepaths.EngineLayersName.SpaceshipVectors];
        thrustVectorLR = thrustVectorLR_GO.GetComponent<LineRenderer>();
        thrustVectorLR.useWorldSpace = false;
        thrustVectorLR.sharedMaterial = Resources.Load("OrbitMaterial", typeof(Material)) as Material; // SHOULD BE CHANGED
    }

    public void FireRCS(RCSThrustOrientation orientation, RCSThrustSense sense)
    {
        if(effectiveRB == null) {
            Debug.LogWarningFormat("The specified Rigidbody 'effectiveRB' is null. Thrust for {0} is thus disabled.", name);
            return;
        }
        
        Vector3 thrust = RCSThrustPower * VectorFromOrientation(orientation) * ValueFromThrustSense(sense);
        effectiveRB.AddForceAtPosition(thrust, worldPosition, ForceMode.Force);
    }

    private Vector3 VectorFromOrientation(RCSThrustOrientation orientation)
    {
        switch(orientation) {
            case RCSThrustOrientation.right:
                return worldLocalRight;

            case RCSThrustOrientation.up:
                return worldLocalUp;
            
            case RCSThrustOrientation.forward:
                return worldLocalForward;

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

}