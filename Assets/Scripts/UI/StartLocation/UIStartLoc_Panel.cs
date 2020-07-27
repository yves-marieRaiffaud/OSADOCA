using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIStartLoc_Panel : MonoBehaviour
{
    [Header("UI GameObjects/RectTransform")]
    public RectTransform startLoc_Panel_RT;
    public GameObject section_InitPlanetarySurfaceGO;
    public GameObject section_InitOrbitGO;
    // Empty Gameobject containing every simple spheres
    public GameObject simpleSpheresFolder;
    // Dropdowns
    [Header("Dropdowns")]
    public TMPro.TMP_Dropdown startLocInitTypeDropdown;
    public TMPro.TMP_Dropdown startLocPlanetSelectorDropdown;

    private string lastSelectedPlanetName;
    //===================================================================================================
    [HideInInspector] public enum startLocInitType { inOrbit, planetarySurface };
    [HideInInspector] public Dictionary<startLocInitType, string> startLocInitTypeDict = new Dictionary<startLocInitType, string> {
        { startLocInitType.inOrbit, "In Orbit" },
        { startLocInitType.planetarySurface, "Planetary Surface" }
    };


    void Start()
    {
        lastSelectedPlanetName = UniCsts.planets.Earth.ToString();

        Init_startLocInitTypeDropdown();
        startLocInitTypeDropdown.onValueChanged.AddListener(delegate { OnValueChangedInitTypeDropdown(); });

        Init_startLocPlanetSelectorDropdown();
        startLocPlanetSelectorDropdown.onValueChanged.AddListener(delegate { OnValueChangedPlanetSelectorDropdown(); });
        
        InitSimpleSpheres();
    }

    private void InitSimpleSpheres()
    {
        // Spawn every simpleSpheres in the folder, and set the 'orbitedBody' variable
        foreach(Transform child in simpleSpheresFolder.transform)
        {
            CelestialBody celestBody = child.gameObject.GetComponent<CelestialBody>();
            celestBody.AwakeCelestialBody(UniCsts.planetsDict[celestBody.settings.chosenPredifinedPlanet]);
        }
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
        UIStartLoc_InitPlanetarySurf initPlanetary_instance = section_InitPlanetarySurfaceGO.GetComponent<UIStartLoc_InitPlanetarySurf>();
        initPlanetary_instance.currPlanetSelectedName = startLocPlanetSelectorDropdown.options[startLocPlanetSelectorDropdown.value].text;
        initPlanetary_instance.UpdatePlanetaryMap();
    }

    private void Init_startLocPlanetSelectorDropdown()
    {
        UniCsts.planets lastBody = UsefulFunctions.CastStringTo_Unicsts_Planets(lastSelectedPlanetName);
        double lastSelectedBodyIsRocky = UniCsts.planetsDict[lastBody][CelestialBodyParamsBase.otherParams.isRockyBody.ToString()];
        int dropdownIdxToSet = 0;
        int bodyIndex = 0;

        startLocPlanetSelectorDropdown.ClearOptions();

        List<string> possibleBodies = new List<string>();
        switch(startLocInitTypeDropdown.value)
        {
            case 0:
                // 'In-orbit' init
                // Taking every planets and body, its does not matter if the body is rocky or not
                foreach(UniCsts.planets body in UniCsts.planetsDict.Keys)
                {
                    string bodyName = body.ToString();
                    possibleBodies.Add(bodyName);
                    if(lastSelectedPlanetName.Equals(bodyName))
                    {
                        dropdownIdxToSet = bodyIndex;
                    }
                    bodyIndex += 1;
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
                        string bodyNameToAdd = body_KV_Pair.Key.ToString();
                        possibleBodies.Add(bodyNameToAdd);

                        if(lastSelectedBodyIsRocky == 1d && lastSelectedPlanetName.Equals(bodyNameToAdd))
                        {
                            // The corresponding planet has been found
                            dropdownIdxToSet = bodyIndex;
                        }
                        bodyIndex += 1;
                    }
                }
                break;
        }

        startLocPlanetSelectorDropdown.AddOptions(possibleBodies);
        startLocPlanetSelectorDropdown.value = dropdownIdxToSet;
        // Finally, need to update the displayed planet
        OnValueChangedPlanetSelectorDropdown();
    }

    private void OnValueChangedPlanetSelectorDropdown()
    {
        // Display the selected planet and hide the others

        // Suffix used for every simpleSphere GameObject. Suffix to add after the name of the planet starting with a capital letter
        string go_suffix = "Planet_UI";
        string dropdownText = startLocPlanetSelectorDropdown.options[startLocPlanetSelectorDropdown.value].text;
        string goToFind = dropdownText + go_suffix;

        UIStartLoc_InitOrbit initOrbit_instance = section_InitOrbitGO.GetComponent<UIStartLoc_InitOrbit>();
        UIStartLoc_InitPlanetarySurf initPlanetary_instance = section_InitPlanetarySurfaceGO.GetComponent<UIStartLoc_InitPlanetarySurf>();
        initPlanetary_instance.currPlanetSelectedName = dropdownText;
        initPlanetary_instance.UpdatePlanetaryMap();

        foreach(Transform child in simpleSpheresFolder.transform)
        {
            if(child.name.Equals(goToFind))
            {
                child.gameObject.SetActive(true);
                initOrbit_instance.orbitedBody = child.gameObject.GetComponent<CelestialBody>();
            }
            else {
                child.gameObject.SetActive(false);
            }
        }

        // Redraw the orbit to take into account the change of planet
        if(!initOrbit_instance.InputFieldsAreAllEmpty())
        {
            // Update only if some values have been entered. Avoid drawing an orbit of ra=rp=0 km when first selecting the planet
            initOrbit_instance.OnUpdateOrbit_BtnClick();
        }

        lastSelectedPlanetName = startLocPlanetSelectorDropdown.options[startLocPlanetSelectorDropdown.value].text;
    }

}