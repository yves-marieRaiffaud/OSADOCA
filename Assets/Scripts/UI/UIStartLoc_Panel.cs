using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIStartLoc_Panel : MonoBehaviour
{
    public RectTransform startLoc_Panel_RT;
    public GameObject section_InitPlanetarySurfaceGO;
    public GameObject section_InitOrbitGO;

    public TMPro.TMP_Dropdown startLocInitTypeDropdown;
    public TMPro.TMP_Dropdown startLocPlanetSelectorDropdown;

    [HideInInspector] public enum startLocInitType { inOrbit, planetarySurface };
    [HideInInspector] public Dictionary<startLocInitType, string> startLocInitTypeDict = new Dictionary<startLocInitType, string> {
        { startLocInitType.inOrbit, "In Orbit" },
        { startLocInitType.planetarySurface, "Planetary Surface" }
    };


    void Start()
    {
        Init_startLocInitTypeDropdown();
        startLocInitTypeDropdown.onValueChanged.AddListener(delegate { OnValueChangedInitTypeDropdown(); });

        Init_startLocPlanetSelectorDropdown();
        startLocPlanetSelectorDropdown.onValueChanged.AddListener(delegate { OnValueChangedPlanetSelectorDropdown(); });

        // ONLY FOR DEBUG
        // Start directly on the planetary surface init, for debug
        //startLocInitTypeDropdown.value = 1;
    }

    private void Init_startLocInitTypeDropdown()
    {
        startLocInitTypeDropdown.ClearOptions();
        startLocInitTypeDropdown.AddOptions(new List<string>(startLocInitTypeDict.Values));
    }

    private void OnValueChangedInitTypeDropdown()
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
        // Need to update the dropdown of the possible start planets
        Init_startLocPlanetSelectorDropdown();
    }

    private void Init_startLocPlanetSelectorDropdown()
    {
        startLocPlanetSelectorDropdown.ClearOptions();

        List<string> possibleBodies = new List<string>();
        switch(startLocInitTypeDropdown.value)
        {
            case 0:
                // 'In-orbit' init
                // Taking every planets and body, its does not matter if the body is rocky or not
                foreach(UniCsts.planets body in UniCsts.planetsDict.Keys)
                {
                    possibleBodies.Add(body.ToString());
                }
                break;

            case 1:
                // 'Planetary surface' init
                // Taking only the rocky bodies for planetary surface init
                foreach(KeyValuePair<UniCsts.planets, Dictionary<string, double>> body_KV_Pair in UniCsts.planetsDict)
                {
                    if(body_KV_Pair.Value[CelestialBodyParamsBase.otherParams.isRockyBody.ToString()] == 1d)
                    {
                        // Body is rocky
                        possibleBodies.Add(body_KV_Pair.Key.ToString());
                    }
                }
                break;
        }

        startLocPlanetSelectorDropdown.AddOptions(possibleBodies);
    }

    private void OnValueChangedPlanetSelectorDropdown()
    {
        // do something
    }

}