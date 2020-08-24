using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.VFX;

public class MainNozzle_Control : MonoBehaviour
{
    float nozzleThrust = 1;
    float nozzleLowerBound = -10;
    float nozzleUpperBound = 10;
    //================
    public bool useKeyboardControl;
    [HideInInspector] public bool engineIsActive;
    //================
    Rigidbody nozzleRigidbody;
    ConfigurableJoint originalfreeFloatingJoint; //Configurable Joint from the Editor
    ConfigurableJoint onlinefreeFloatingJoint; //Configurable Joint that is created and updated in live in 'Update()'
    float yawAngle_min, yawAngle_max;
    float pitchAngle_min, pitchAngle_max;
    //================
    VisualEffect mainNozzleVFX;
    //================
    void Start()
    {
        engineIsActive = false;
        nozzleRigidbody = GetComponent<Rigidbody>();
        originalfreeFloatingJoint = GetComponent<ConfigurableJoint>();

        SetLiveConfigurableJoint();

        if(transform.parent.Find("MainNozzleVFX") != null)
            mainNozzleVFX = transform.parent.Find("MainNozzleVFX").GetComponent<VisualEffect>();
        else
            mainNozzleVFX = null;
    }

    void FixedUpdate()
    {
        if(useKeyboardControl)
        {
            // Reading raw values from the controller
            float[] raw_inputs_array = getInputAxis();
            float rawThrustValue = raw_inputs_array[2];
            if(!UsefulFunctions.FloatsAreEqual(rawThrustValue, 0f) && mainNozzleVFX != null)
            {
                engineIsActive = true;
            }
            if(UsefulFunctions.FloatsAreEqual(rawThrustValue, 0f) && mainNozzleVFX != null)
            {
                engineIsActive = false;
            }
            FireEngine(raw_inputs_array, rawThrustValue);
        }
        if(engineIsActive)
        {
            mainNozzleVFX.Play();
        }
        else {
            mainNozzleVFX.Stop();
            engineIsActive = false;
        }
    }

    public IEnumerator FireEngine(float[] raw_inputs_array, float rawThrustValue)
    {
        if(UsefulFunctions.FloatsAreEqual(rawThrustValue, 0f)) {
            engineIsActive = false;
            yield return null;
        }
        
        // Tranforming the raw values to the desired rotation angles of the nozzle
        Quaternion targetQuaternion = computeYawPitchAngle(raw_inputs_array[0], raw_inputs_array[1]);
        RotateNozzle(targetQuaternion);
        nozzleRigidbody.AddForce(transform.up * nozzleThrust * rawThrustValue, ForceMode.Force);
        engineIsActive = false;
        yield return null;
    }

    private void SetLiveConfigurableJoint()
    {
        // Copying the values from the Joint in the Editor, in order to keep the user-input values
        onlinefreeFloatingJoint = originalfreeFloatingJoint;

        // Common attributes for every Soft Join Limits
        float contactDistance = 100;
        SoftJointLimit angularXLimits = new SoftJointLimit();
        angularXLimits.contactDistance = contactDistance;
        angularXLimits.bounciness = 0;

        // Setting the nozzle lower angular limit along the X-Axis/Yaw Angle
        angularXLimits.limit = nozzleLowerBound;
        onlinefreeFloatingJoint.lowAngularXLimit = angularXLimits;

        // Setting the nozzle upper angular limit along the X-Axis/Yaw Angle
        angularXLimits.limit = nozzleUpperBound;
        onlinefreeFloatingJoint.highAngularXLimit = angularXLimits;

        // Setting the nozzle angular limit along the Y-Axis/Pitch Angle
        onlinefreeFloatingJoint.angularYLimit = angularXLimits;
        onlinefreeFloatingJoint.projectionAngle = nozzleUpperBound;

        // Retrieving the min/max angles along the Yaw/X and Pitch/Y Axis
        yawAngle_min = onlinefreeFloatingJoint.lowAngularXLimit.limit;
        yawAngle_max = onlinefreeFloatingJoint.highAngularXLimit.limit;
        pitchAngle_max = onlinefreeFloatingJoint.angularYLimit.limit;
        pitchAngle_min = -pitchAngle_max;
    }

    private float[] getInputAxis()
    {
        float[] outputArray = new float[3];
        outputArray[0] = CrossPlatformInputManager.GetAxis("YawAxis");
        outputArray[1] = CrossPlatformInputManager.GetAxis("PitchAxis");
        outputArray[2] = CrossPlatformInputManager.GetAxis("VerticalThrust");
        return outputArray;
    }

    private Quaternion computeYawPitchAngle(float rawYawAngle, float rawPitchAngle)
    {
        float desiredYawAngle = UsefulFunctions.mapInRange(new Vector2(-1, 1), new Vector2(yawAngle_min, yawAngle_max), rawYawAngle);
        float desiredPitchAngle = UsefulFunctions.mapInRange(new Vector2(-1, 1), new Vector2(pitchAngle_min, pitchAngle_max), rawPitchAngle);

        // Rounding small values to zero (can be seen as precision)
        if(UsefulFunctions.isInRange(desiredYawAngle, -0.1f, 0.1f))
        {
            desiredYawAngle = 0f;
        }
        if(UsefulFunctions.isInRange(desiredPitchAngle, -0.1f, 0.1f))
        {
            desiredPitchAngle = 0f;
        }
        return Quaternion.Euler(desiredYawAngle, desiredPitchAngle, 0f);
    }

    private void RotateNozzle(Quaternion targetQuaternion)
    {
        // Seeting the target Quaternion of the Configurable Joint. Can be used with either the SLERP motor or the X/YZ motor
        onlinefreeFloatingJoint.targetRotation = targetQuaternion;
    }

}
