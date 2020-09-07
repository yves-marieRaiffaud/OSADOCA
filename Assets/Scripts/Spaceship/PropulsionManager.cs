using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using System.Text;
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
        if(Input.GetKey(KeyCode.Z))
            FireRCS_Translate(RCSTranslationDirection.Upward);

        else if(Input.GetKey(KeyCode.S))
            FireRCS_Translate(RCSTranslationDirection.Downward);
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
            
            default:
                orientation = RCSThruster.RCSThrustOrientation.NULL;
                sense = RCSThruster.RCSThrustSense.NULL;
                break;

        }

        foreach(RCSThruster rcs in rcsThrusters)
            rcs.FireRCS(orientation, sense);
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
        Debug.Log("propulsionObj = " + propulsionObj);
        propulsionObj.DrawThrustVector();
    }
}