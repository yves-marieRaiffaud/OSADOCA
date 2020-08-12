using UnityEngine;
using System.Collections;
 
[AddComponentMenu("Camera-Control/Mouse Orbit with zoom for Simulation Rockt Spacecraft")]
public class SimRocketCam_MouseOrbit : MonoBehaviour
{
    public Transform cameraBackTR; // Camera rendering the celestialBodies and the outer space
    public Transform target;
    private float targetLocalScale; // Need rocket local scale to scale the computed distance

    private float distance;
    public float xSpeed = 80.0f;
    public float ySpeed = 80.0f;
 
    public float yMinLimit = -90f;
    public float yMaxLimit = 90f;
 
    public float distanceMin = 2f;
    public float distanceMax = 300f;
 
    private Rigidbody rb;
 
    float x = 0.0f;
    float y = 0.0f;

    private Camera mainCameraForRocket; // Needed to modify its Far clipping plane value when zooming in/out
    private Camera cameraBackOuterSpace;
    private float farClipPlaneMargin;

    void Start () 
    {
        targetLocalScale = 3f / (target.localScale.x + target.localScale.y + target.localScale.z);
        distance = cameraBackTR.position.magnitude/targetLocalScale;
        //farClipPlaneMargin = (float)UniCsts.u2pl/2f;
        farClipPlaneMargin = 10f;//(float)UniCsts.u2pl/2f / targetLocalScale;
        //========
        mainCameraForRocket = gameObject.GetComponent<Camera>();
        cameraBackOuterSpace = cameraBackTR.gameObject.GetComponent<Camera>();
        rb = GetComponent<Rigidbody>();
        //========
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        //========
        // Make the rigid body not change rotation
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }
 
    void LateUpdate () 
    {
        farClipPlaneMargin = Mathf.Max(farClipPlaneMargin, (float)UniCsts.u2pl/2f/targetLocalScale);
        //========
        UpdateCamera_Rotation_Zoom();
        //cameraBackOuterSpace.nearClipPlane = mainCameraForRocket.farClipPlane;
        cameraBackTR.transform.position = transform.position;
        cameraBackTR.transform.rotation = transform.rotation;
    }
 
    private void UpdateCamera_Rotation_Zoom()
    {
        if (target && Input.GetMouseButton(0)) 
        {
            x += Input.GetAxis("MouseX") * xSpeed * 0.02f;
            y -= Input.GetAxis("MouseY") * ySpeed * 0.02f;
            //========
            y = ClampAngle(y, yMinLimit, yMaxLimit);
            //========
            Quaternion rotation = Quaternion.Euler(y, x, 0);
            distance = Mathf.Clamp(distance, distanceMin, distanceMax);
            //========
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance/targetLocalScale);
            Vector3 position = rotation * negDistance + target.position;
            //========
            transform.rotation = rotation;
            transform.position = position;
        }
        else if(target)
        {
            // For the zoom management, no need to have the left mouse button held down
            distance = Mathf.Clamp(distance - Input.GetAxis("MouseScrollWheel")*5, distanceMin, distanceMax);
            //========
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance/targetLocalScale);
            Vector3 position = transform.rotation * negDistance + target.position;
            //========
            transform.position = position;
        }
        mainCameraForRocket.farClipPlane = distance/targetLocalScale + farClipPlaneMargin;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}