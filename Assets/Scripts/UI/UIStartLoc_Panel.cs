using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStartLoc_Panel : MonoBehaviour
{
    public RectTransform startLoc_Panel_RT;
    public GameObject section_InitPlanetarySurfaceGO;
    public GameObject section_InitOrbitGO;

    public TMPro.TMP_Dropdown startLocInitTypeDropdown;

    [HideInInspector] public enum startLocInitType { inOrbit, planetarySurface };
    [HideInInspector] public Dictionary<startLocInitType, string> startLocInitTypeDict = new Dictionary<startLocInitType, string> {
        { startLocInitType.inOrbit, "In Orbit" },
        { startLocInitType.planetarySurface, "Planetary Surface" }
    };


    void Start()
    {
        Init_startLocInitTypeDropdown();
        startLocInitTypeDropdown.onValueChanged.AddListener(delegate { OnValueChangedStartLocInitTypeDropdown(); });

        // ONLY FOR DEBUG
        // Start directly on the planetary surface init, for debug
        startLocInitTypeDropdown.value = 1;
    }

    private void Init_startLocInitTypeDropdown()
    {
        startLocInitTypeDropdown.ClearOptions();
        startLocInitTypeDropdown.AddOptions(new List<string>(startLocInitTypeDict.Values));
    }

    private void OnValueChangedStartLocInitTypeDropdown()
    {
        switch(startLocInitTypeDropdown.value)
        {
            case 0:
                section_InitOrbitGO.SetActive(true);
                section_InitPlanetarySurfaceGO.SetActive(false);
                break;
            
            case 1:
                section_InitOrbitGO.SetActive(false);
                section_InitPlanetarySurfaceGO.SetActive(true);
                break;
        }
    }

    void OnGUI()
    {

    }
    


}