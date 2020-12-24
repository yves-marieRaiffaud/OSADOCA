using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Image_OrbitPreview_Script : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Camera that renderers the simpleSpheres: 'UI_OrbitPreviewCam' GameObject")]
    public GameObject uiOrbitPreviewCam;
    private UI_OrbitPreview_MouseOrbit cameraManagerInstance;

    void Start()
    {
        cameraManagerInstance = uiOrbitPreviewCam.GetComponent<UI_OrbitPreview_MouseOrbit>();
        if(cameraManagerInstance == null)
            Debug.LogError("Could not find any Component of type 'MouseOrbitImproved' attached to the specified OrbitPreview UI element");
        cameraManagerInstance.mouseOver_uiOrbitPreview = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        cameraManagerInstance.mouseOver_uiOrbitPreview = true;
    }
 
    public void OnPointerExit(PointerEventData eventData)
    {
        if(!Input.GetMouseButton(0))
            cameraManagerInstance.mouseOver_uiOrbitPreview = false;
    }
}