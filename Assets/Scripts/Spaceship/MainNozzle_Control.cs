using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.VFX;
using Mathd_Lib;

public class MainNozzle_Control : MonoBehaviour
{
    // TO get access to the simEnv, and thus to the 'shipUseKeyboardControl' SimSettingBool variable
    public UniverseRunner universe;

    public float nozzleThrust_Power = 3;
    [Header("Yaw Axis Nozzle Limits")]
    public float yawAngle_min, yawAngle_max;
    [Header("Pitch Axis Limits")]
    public float pitchAngle_min, pitchAngle_max;
    //================
    private bool useKeyboardControl;
    [HideInInspector] public bool engineIsActive;
    //================
    Rigidbody nozzleRigidbody;
    ConfigurableJoint originalfreeFloatingJoint; //Configurable Joint from the Editor
    ConfigurableJoint onlinefreeFloatingJoint; //Configurable Joint that is created and updated in live in 'Update()'
    //================
    VisualEffect mainNozzleVFX;
    Spaceship spaceship;

    private float nozzleThrustValue;
    private float[] targetedAngles;

    private Vector3 thrustForce;
    public bool showThrustVector;
    [HideInInspector] public GameObject thrustLR_GO;
    //================
    void Start()
    {
        engineIsActive = false;
        nozzleThrustValue = 0f;
        showThrustVector = false;
        thrustForce = Vector3.zero;
        thrustLR_GO = null;
        targetedAngles = new float[2];
        targetedAngles[0] = targetedAngles[1] = 0f;

        spaceship = transform.parent.gameObject.GetComponent<Spaceship>();

        if(universe == null) {
            Debug.LogWarning("The 'UniverseRunner' script has not been assgined in the MainNozzle_Control script.\n The default control (using keyboard) have been assgined for the spaceship.\nIf you are in the UI main menu, DISREGARD THIS MESSAGE");
            useKeyboardControl = true;
        }
        else
            useKeyboardControl = universe.simEnv.shipUseKeyboardControl.value;

        nozzleRigidbody = GetComponent<Rigidbody>();
        originalfreeFloatingJoint = GetComponent<ConfigurableJoint>();

        SetLiveConfigurableJoint();
        mainNozzleVFX = GetComponentInChildren<VisualEffect>();
    }

    void FixedUpdate()
    {
        if(useKeyboardControl)
            Fire_Engine_KeyboardControl();
        else
            Fire_Engine_GNC_Algorithms(); // Else we are controlling the rocket via TCP/IP orders and GNC algorithms
    }

    void LateUpdate()
    {
        if(showThrustVector)
            ShowThrustVector();
    }

    public void ShowThrustVector()
    {
        // Layer 12 is the 'SpaceshipVectors' layer
        float maxVecLength = 20f;
        //float vecLength = Mathf.Clamp(thrustForce.magnitude / 100f, -maxVecLength, maxVecLength);
        Debug.Log("thrustForce = " + thrustForce);
        thrustLR_GO = UsefulFunctions.DrawVector(thrustForce, Vector3.zero, 10f, "MainNozzleThrust", 0.5f, 12, spaceship.gameObject.transform);
        thrustLR_GO.transform.position = spaceship.transform.position;
        thrustLR_GO.transform.rotation = Quaternion.identity;
    }

    private void Fire_Engine_GNC_Algorithms()
    {
        RotateNozzle();
        if(engineIsActive)
        {
            mainNozzleVFX.Play();
            // Tranforming the raw values to the desired rotation angles of the nozzle
            ApplyThrust();
        }
        else {
            mainNozzleVFX.Stop();
            engineIsActive = false;
            nozzleRigidbody.AddForce(thrustForce, ForceMode.Force);
        }
    }

    private void Fire_Engine_KeyboardControl()
    {
        float[] raw_inputs_array = getInputAxis();
        targetedAngles[0] = raw_inputs_array[0];
        targetedAngles[1] = raw_inputs_array[1];
        nozzleThrustValue = raw_inputs_array[2] * nozzleThrust_Power;

        RotateNozzle();

        if(UsefulFunctions.FloatsAreEqual(nozzleThrustValue, 0f)) {
            engineIsActive = false;
            mainNozzleVFX.Stop();
        }
        else {
            engineIsActive = true;
            mainNozzleVFX.Play();
            ApplyThrust();
        }
    }

    public IEnumerator FireEngine(float[] raw_inputs_array, float rawThrustValue)
    {
        if(spaceship != null)
        {
            if(spaceship.shipRBConstraints_areOn)
            {
                Rigidbody rb = spaceship.GetComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.None;
                spaceship.shipRBConstraints_areOn = false;
            }
        }

        nozzleThrustValue = rawThrustValue;
        targetedAngles = raw_inputs_array;

        if(nozzleThrustValue < 0.4f) {
            engineIsActive = false;
            mainNozzleVFX.Stop();
        }
        else {
            engineIsActive = true;
            mainNozzleVFX.Play();
        }
        yield return null;
    }

    private void SetLiveConfigurableJoint()
    {
        // Copying the values from the Joint in the Editor, in order to keep the user-input values
        onlinefreeFloatingJoint = originalfreeFloatingJoint;
        // Local Scale of the spaceship, to scale the anchors position
        Vector3 ls = spaceship.transform.localScale;
        float lsFactor = (ls.x + ls.y + ls.z) / 3f;

        onlinefreeFloatingJoint.anchor = new Vector3(0f, 0f*lsFactor, 0f);
        onlinefreeFloatingJoint.axis = Vector3.right;
        onlinefreeFloatingJoint.autoConfigureConnectedAnchor = false;
        onlinefreeFloatingJoint.connectedAnchor = Vector3.zero;
        onlinefreeFloatingJoint.secondaryAxis = Vector3.forward;

        onlinefreeFloatingJoint.xMotion = ConfigurableJointMotion.Locked;
        onlinefreeFloatingJoint.yMotion = ConfigurableJointMotion.Locked;
        onlinefreeFloatingJoint.zMotion = ConfigurableJointMotion.Locked;
        onlinefreeFloatingJoint.angularXMotion = ConfigurableJointMotion.Limited;
        onlinefreeFloatingJoint.angularYMotion = ConfigurableJointMotion.Limited;
        onlinefreeFloatingJoint.angularZMotion = ConfigurableJointMotion.Locked;

        SoftJointLimitSpring linearLimitSpring = new SoftJointLimitSpring();
        linearLimitSpring.spring = 0f;
        linearLimitSpring.damper = 0f;
        onlinefreeFloatingJoint.linearLimitSpring = linearLimitSpring;

        SoftJointLimit linearLimit = new SoftJointLimit();
        linearLimit.limit = 0f;
        linearLimit.bounciness = 0f;
        linearLimit.contactDistance = 10f*lsFactor;
        onlinefreeFloatingJoint.linearLimit = linearLimit;

        onlinefreeFloatingJoint.angularXLimitSpring = linearLimitSpring;

        SoftJointLimit angularLimits = new SoftJointLimit();
        angularLimits.bounciness = 0f;
        angularLimits.contactDistance = 10f*lsFactor;

        angularLimits.limit = yawAngle_min;
        onlinefreeFloatingJoint.lowAngularXLimit = angularLimits;

        angularLimits.limit = yawAngle_max;
        onlinefreeFloatingJoint.highAngularXLimit = angularLimits;

        onlinefreeFloatingJoint.angularYZLimitSpring = linearLimitSpring;

        angularLimits.limit = Mathf.Abs(pitchAngle_max);
        onlinefreeFloatingJoint.angularYLimit = angularLimits;

        angularLimits.limit = 0f;
        onlinefreeFloatingJoint.angularZLimit = angularLimits;

        onlinefreeFloatingJoint.targetPosition = Vector3.zero;
        onlinefreeFloatingJoint.targetVelocity = Vector3.zero;

        JointDrive jointDrive = new JointDrive();
        jointDrive.positionSpring = 0f;
        jointDrive.positionDamper = 0f;
        jointDrive.maximumForce = Mathf.Infinity;

        onlinefreeFloatingJoint.xDrive = jointDrive;
        onlinefreeFloatingJoint.yDrive = jointDrive;
        onlinefreeFloatingJoint.zDrive = jointDrive;

        onlinefreeFloatingJoint.targetRotation = Quaternion.identity;
        onlinefreeFloatingJoint.targetAngularVelocity = Vector3.zero;

        onlinefreeFloatingJoint.rotationDriveMode = RotationDriveMode.Slerp;

        onlinefreeFloatingJoint.angularXDrive = jointDrive;
        onlinefreeFloatingJoint.angularYZDrive = jointDrive;

        JointDrive slerpDrive = new JointDrive();
        slerpDrive.positionSpring = 1e7f;
        slerpDrive.positionDamper = 1e6f;
        slerpDrive.maximumForce = Mathf.Infinity;

        onlinefreeFloatingJoint.slerpDrive = slerpDrive;

        onlinefreeFloatingJoint.projectionMode = JointProjectionMode.None;
        onlinefreeFloatingJoint.projectionDistance = 0.01f;
        onlinefreeFloatingJoint.projectionAngle = 1f;

        onlinefreeFloatingJoint.configuredInWorldSpace = false;
        onlinefreeFloatingJoint.swapBodies = false;

        onlinefreeFloatingJoint.breakForce = Mathf.Infinity;
        onlinefreeFloatingJoint.breakTorque = Mathf.Infinity;

        onlinefreeFloatingJoint.enableCollision = false;
        onlinefreeFloatingJoint.enablePreprocessing = true;

        onlinefreeFloatingJoint.massScale = 1f;
        onlinefreeFloatingJoint.connectedMassScale = 1f;
    }

    private float[] getInputAxis()
    {
        float[] outputArray = new float[3];
        outputArray[0] = CrossPlatformInputManager.GetAxis("Horizontal");
        outputArray[1] = CrossPlatformInputManager.GetAxis("Vertical");
        outputArray[2] = CrossPlatformInputManager.GetAxis("VerticalThrust");
        return outputArray;
    }

    private Quaternion computeYawPitchAngle(float rawYawAngle, float rawPitchAngle)
    {
        float desiredYawAngle = UsefulFunctions.mapInRange(new Vector2(-1, 1), new Vector2(yawAngle_min, yawAngle_max), rawYawAngle);
        float desiredPitchAngle = UsefulFunctions.mapInRange(new Vector2(-1, 1), new Vector2(pitchAngle_min, pitchAngle_max), rawPitchAngle);
        return Quaternion.Euler(desiredYawAngle, desiredPitchAngle, 0f);
    }

    private void ApplyThrust()
    {
        thrustForce += transform.up * nozzleThrustValue;
        //Vector3d thrustAcc = new Vector3d(thrustForce) / 1d /*spaceship.mass*/;
        nozzleRigidbody.AddForce(thrustForce, ForceMode.Force);
        //spaceship.orbitedBodyRelativeAcc += thrustAcc;
    }

    private void RotateNozzle()
    {
        Quaternion targetQuaternion = computeYawPitchAngle(targetedAngles[0], targetedAngles[1]);
        // Seeting the target Quaternion of the Configurable Joint. Can be used with either the SLERP motor or the X/YZ motor
        originalfreeFloatingJoint.targetRotation = targetQuaternion;
    }

}
