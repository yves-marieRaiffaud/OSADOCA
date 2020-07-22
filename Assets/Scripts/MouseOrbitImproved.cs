using UnityEngine;
using System.Collections;
 
[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbitImproved : MonoBehaviour
{
    public GameObject uiOrbitPreview;
    [HideInInspector] public bool mouseOver_uiOrbitPreview=false;
    public UIStartLoc_InitOrbit sectionInitOrbitUIPanel; // Only to access the computed Orbit and modify the width of the rendered orbit with respect to the distance of the camera 

    public Transform target;
    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
 
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
 
    public float distanceMin = 6f;
    public float distanceMax = 15f;
 
    private Rigidbody rigidbody;
 
    float x = 0.0f;
    float y = 0.0f;

    // Use this for initialization
    void Start () 
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
 
        rigidbody = GetComponent<Rigidbody>();
 
        // Make the rigid body not change rotation
        if (rigidbody != null)
        {
            rigidbody.freezeRotation = true;
        }
    }
 
    void LateUpdate () 
    {
        UpdateCamera_Rotation_Zoom();
        UpdatePreviewedOrbit_lineRendererWidth();
        UpdatePreviewedOrbit_Spacecraft();
        UpdatePreviewedOrbit_PinpointRot();
    }
 
    private void UpdateCamera_Rotation_Zoom()
    {
        if (target && mouseOver_uiOrbitPreview && Input.GetMouseButton(0)) 
        {
            x += Input.GetAxis("MouseX") * xSpeed * 0.02f;
            y -= Input.GetAxis("MouseY") * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);

            distance = Mathf.Clamp(distance, distanceMin, distanceMax);

            RaycastHit hit;
            if (Physics.Linecast (target.position, transform.position, out hit)) 
            {
                distance -=  hit.distance;
            }
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            transform.position = position;
        }
        else if(target && mouseOver_uiOrbitPreview)
        {
            // For the zoom management, no need to have the left mouse button held down
            distance = Mathf.Clamp(distance - Input.GetAxis("MouseScrollWheel")*5, distanceMin, distanceMax);

            RaycastHit hit;
            if (Physics.Linecast (target.position, transform.position, out hit)) 
            {
                distance -=  hit.distance;
            }
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = transform.rotation * negDistance + target.position;

            transform.position = position;
        }
    }

    private void UpdatePreviewedOrbit_lineRendererWidth()
    {
        if(sectionInitOrbitUIPanel.previewedOrbit != null)
        {
            sectionInitOrbitUIPanel.previewedOrbit.lineRenderer.widthMultiplier = 0.004f * distance;
        }
    }

    private void UpdatePreviewedOrbit_Spacecraft()
    {
        sectionInitOrbitUIPanel.orbitingSpacecraft.transform.LookAt(transform);

        float scaleFactor = distance * 0.015f;
        Vector3 initialScale = new Vector3(0.05f, 0.05f, 0.05f);
        sectionInitOrbitUIPanel.orbitingSpacecraft.transform.localScale = initialScale * scaleFactor;
    }

    private void UpdatePreviewedOrbit_PinpointRot()
    {
        // Update rotation of the perihelion/aphelion pinpoint, so that it always look at the camera
        sectionInitOrbitUIPanel.perihelionPinpoint.transform.LookAt(transform);
        sectionInitOrbitUIPanel.aphelionPinpoint.transform.LookAt(transform);

        float scaleFactor = distance * 0.02f;
        Vector3 initialScale = new Vector3(0.05f, 0.05f, 0.05f);
        sectionInitOrbitUIPanel.perihelionPinpoint.transform.localScale = initialScale * scaleFactor;
        sectionInitOrbitUIPanel.aphelionPinpoint.transform.localScale = initialScale * scaleFactor;
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