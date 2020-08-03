using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_planetMap_PointerEvents : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // This script is attached to the GameObject 'PlanetMap' that draws the map of the selected planet

    [Tooltip("GameObject named 'Section_InitPlanetarySurface' that contains the 'UIStartLoc_PlanetaryInit' script")]
    public GameObject panelPlanetMapSelection;
    private UIStartLoc_InitPlanetarySurf uiStartLocplanetaryInitScript;

    void Start()
    {
        uiStartLocplanetaryInitScript = panelPlanetMapSelection.GetComponent<UIStartLoc_InitPlanetarySurf>();
        if(uiStartLocplanetaryInitScript == null)
        {
            Debug.LogError("Could not find any Component of type 'UIStartLoc_InitPlanetarySurf' attached to the specified 'Section_InitPlanetarySurface' panel GameObject");
        }
        uiStartLocplanetaryInitScript.isAddingNewLaunchPad = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        uiStartLocplanetaryInitScript.isAddingNewLaunchPad = true;
    }
 
    public void OnPointerExit(PointerEventData eventData)
    {
        if(!Input.GetMouseButton(1))
        {
            uiStartLocplanetaryInitScript.isAddingNewLaunchPad = false;
        }
    }
}
