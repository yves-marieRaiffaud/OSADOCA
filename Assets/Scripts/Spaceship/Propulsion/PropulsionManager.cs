using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using System.Text;
using System.Linq;
using UseFnc = UsefulFunctions;

public class PropulsionManager : MonoBehaviour
{
    [HideInInspector] public enum RCSDriveType { Rotate, Translate };
    // Upward: +Y-Axis of the rocket. Downward: -Y-Axis of the rocket
    // Right: +X-Axis of the rocket. Left: -X-Axis of the rocket
    // Forward: +Z-Axis of the rocket. Backward: -Z-Axis of the rocket
    [HideInInspector] public enum RCSTranslationDirection { Upward, Downward, Right, Left, Forward, Backward };

    [HideInInspector] public enum RCSRotationSign { Clockwise, Countercloskwise };
    // Roll: rotation along the Y-Axis, aka the longitudinal axis of the rocket
    // Yaw: rotation along the Z-Axis, rotation left-right
    // Pitch: rotation along the X-Axis, rotation back-forward/up-down
    [HideInInspector] public enum RCSRotationAxis { Roll, Yaw, Pitch };

    public GameObject RCS_Folder;
    List<RCSThruster> rcsThrusters;

    Rigidbody shipRB;
    bool drawAllThrustForces;
    //==========
    void Start()
    {
        drawAllThrustForces = false;
        shipRB = transform.GetComponentInParent<Rigidbody>();
        if(RCS_Folder == null)
            return;

        rcsThrusters = new List<RCSThruster>();
        for(int i=0; i<RCS_Folder.transform.childCount; i++) {
            RCSThruster thruster = RCS_Folder.transform.GetChild(i).GetComponent<RCSThruster>();
            rcsThrusters.Add(thruster);
            thruster.thrustIsOn.AddListener(delegate{
                DrawThrustForce(thruster as PropulsionInterface);
            });
        }
    }


    void FixedUpdate()
    {
        // Forward Backward translation
        if(Input.GetKey(KeyCode.A))
            FireRCS_Translate(RCSTranslationDirection.Forward);
        else if(Input.GetKey(KeyCode.W))
            FireRCS_Translate(RCSTranslationDirection.Backward);

        // Up Down translation
        if(Input.GetKey(KeyCode.Z))
            FireRCS_Translate(RCSTranslationDirection.Upward);
        else if(Input.GetKey(KeyCode.S))
            FireRCS_Translate(RCSTranslationDirection.Downward);

        // Left Right translation
        if(Input.GetKey(KeyCode.Q))
            FireRCS_Translate(RCSTranslationDirection.Left);
        else if(Input.GetKey(KeyCode.D))
            FireRCS_Translate(RCSTranslationDirection.Right);
    }

    void FireRCS_Translate(RCSTranslationDirection translationDirection)
    {
        RCSThruster.RCSThrustOrientation orientation;
        RCSThruster.RCSThrustSense sense;
        switch(translationDirection) {
            case(RCSTranslationDirection.Upward):
                orientation = RCSThruster.RCSThrustOrientation.up;
                sense = RCSThruster.RCSThrustSense.positive;
                break;
            
            case(RCSTranslationDirection.Downward):
                orientation = RCSThruster.RCSThrustOrientation.up;
                sense = RCSThruster.RCSThrustSense.negative;
                break;
            
            case(RCSTranslationDirection.Right):
                orientation = RCSThruster.RCSThrustOrientation.right;
                sense = RCSThruster.RCSThrustSense.positive;
                break;
            
            case(RCSTranslationDirection.Left):
                orientation = RCSThruster.RCSThrustOrientation.right;
                sense = RCSThruster.RCSThrustSense.negative;
                break;
            
            case(RCSTranslationDirection.Forward):
                orientation = RCSThruster.RCSThrustOrientation.forward;
                sense = RCSThruster.RCSThrustSense.positive;
                break;
            
            case(RCSTranslationDirection.Backward):
                orientation = RCSThruster.RCSThrustOrientation.forward;
                sense = RCSThruster.RCSThrustSense.negative;
                break;
            
            default:
                orientation = RCSThruster.RCSThrustOrientation.NULL;
                sense = RCSThruster.RCSThrustSense.NULL;
                break;
        }

        // Check if every rcs must be fired in the same way. If so, we can define a single orientation and sense for every RCS to fire
        if(translationDirection == RCSTranslationDirection.Upward || translationDirection == RCSTranslationDirection.Downward) {
            foreach(RCSThruster rcs in rcsThrusters)
                rcs.FireRCS(orientation, sense);
            return;
        }

        // Else we need to find which RCS should/can be fired, depending on the translationDirection and each RCS thrust axes
        Vector3 desiredThrustAxis = MapTranslationDirectionToShipThrustAxis(translationDirection);
        List<RCSThruster> rcsToFire = Get_RCSThrusters_ThatCanFireInAxis(desiredThrustAxis);
        Debug.Log("desiredThrustAxis: " + desiredThrustAxis + "; firing: " + string.Join("-", rcsToFire.Select(i => i.name)));
        foreach(RCSThruster rcs in rcsToFire) {
            rcs.FireRCS(orientation, sense);
        }
        Debug.Log("====");
    }

    private List<RCSThruster> Get_RCSThrusters_ThatCanFireInAxis(Vector3 desiredThrustAxis)
    {
        // 'desiredThrustAxis' should be in LOCAL SPACE of the rocket
        List<RCSThruster> outputList = new List<RCSThruster>();
        foreach(RCSThruster rcs in rcsThrusters) {
            if(rcs.ThrustAxis_AlignedWith_Direction(desiredThrustAxis))
                outputList.Add(rcs);
        }
        return outputList;
    }

    private Vector3 MapTranslationDirectionToShipThrustAxis(RCSTranslationDirection translationDirection)
    {
        // Returns the thrust axis in the spaceship's LOCAL SPACE corresponding to the passed 'translationDirection' enum
        Vector3 outputThrustAxis;
        switch(translationDirection) {
            case(RCSTranslationDirection.Upward):
                outputThrustAxis = Vector3.up;
                break;
            
            case(RCSTranslationDirection.Downward):
                outputThrustAxis = Vector3.down;
                break;
            
            case(RCSTranslationDirection.Forward):
                outputThrustAxis = Vector3.back;
                break;
            
            case(RCSTranslationDirection.Backward):
                outputThrustAxis = Vector3.forward;
                break;
            
            case(RCSTranslationDirection.Right):
                outputThrustAxis = Vector3.left;
                break;
            
            case(RCSTranslationDirection.Left):
                outputThrustAxis = Vector3.right;
                break;
            
            default:
                outputThrustAxis = Vector3.zero;
                break;
        }
        return outputThrustAxis;
    }

    public void Toggle_DrawAllThrustForce()
    {
        drawAllThrustForces = !drawAllThrustForces;

        foreach(RCSThruster rcs in rcsThrusters) {
            if(drawAllThrustForces)
            {
                if(!rcs.gameObject.activeSelf)
                    rcs.gameObject.SetActive(true);
                rcs.DrawThrustVector();
            }
            else
                rcs.gameObject.SetActive(false);
        }
            
    }

    public void DrawThrustForce(PropulsionInterface propulsionObj)
    {
        propulsionObj.DrawThrustVector();
    }
}