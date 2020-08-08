using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Linq;
using System.IO;

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
    //======
    [HideInInspector] public bool isPLanetarySurfaceInitialization;
    private string selectedSpacecraftName;

    void Start()
    {
        selectedSpacecraftName = "Rocket";
        //======================================

        lastSelectedPlanetName = UniCsts.planets.Earth.ToString();

        Init_startLocInitTypeDropdown();
        startLocInitTypeDropdown.onValueChanged.AddListener(delegate { OnValueChangedInitTypeDropdown(); });

        Init_startLocPlanetSelectorDropdown();
        startLocPlanetSelectorDropdown.onValueChanged.AddListener(delegate { OnValueChangedPlanetSelectorDropdown(); });
        
        InitSimpleSpheres();
    }

    public void On_FLY_click_GatherOrbitalParams()
    {
        // Called when the 'FLY' button is pressed.
        // Gathers the initialisation data and saved the data to a file stored on the disk.
        // This file will then be read in the simulation scene and its values copied to the corresponding spacecraft scriptable object
        SaveOrbitalParamsDataToDisk();
        SaveShipSettingsToDisk();
        Debug.Log("Ship OrbitalParams & Ship Settings have been saved");
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
                isPLanetarySurfaceInitialization = false;
                break;
            
            case 1:
                section_InitOrbitGO.SetActive(false);
                section_InitPlanetarySurfaceGO.SetActive(true);
                isPLanetarySurfaceInitialization = true;
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
        double lastSelectedBodyIsRocky = UniCsts.planetsDict[lastBody][CelestialBodyParamsBase.otherParams.isRockyBody.ToString()].value;
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
                foreach(KeyValuePair<UniCsts.planets, Dictionary<string, UnitInterface>> body_KV_Pair in UniCsts.planetsDict)
                {
                    if(body_KV_Pair.Value[CelestialBodyParamsBase.otherParams.isRockyBody.ToString()].value == 1d)
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
    
    //=====================================================================
    //=====================================================================
    public void SaveOrbitalParamsDataToDisk()
    {
        // Save the needed orbitalParams data to disk
        // Saved data file has the name 'shipToLoad_orbitalParams.json'
        UIStartLoc_InitOrbit UI_initOrbit = section_InitOrbitGO.GetComponent<UIStartLoc_InitOrbit>();
        // Creating the string[] for the 'OrbitalParamsSaveData' struct
        // Refer to struct definition for the order of the variables to add to the string[]
        string[] arrayToSave = new string[OrbitalParamsSaveData.NB_PARAMS];

        arrayToSave[0] = startLocPlanetSelectorDropdown.options[startLocPlanetSelectorDropdown.value].text;
        if(!isPLanetarySurfaceInitialization)
        {
            arrayToSave[1] = UI_initOrbit.orbitDefType.value.ToString();
            arrayToSave[2] = UI_initOrbit.bodyPosType.value.ToString();
            arrayToSave[3] = UI_initOrbit.unitsDropdown.value.ToString();
            arrayToSave[4] = "0"; // Defining a real orbit, not a predicted one
            arrayToSave[5] = "0"; // For the moment, don't draw any vectors/directions
            arrayToSave[6] = "1"; // default as true for drawOrbit bool
            arrayToSave[7] = "0"; // default as false for drawDirections bool
            arrayToSave[8] = "300"; //default value for the orbitDrawingResolution
            arrayToSave[9] = UsefulFunctions.DoubleToString(UI_initOrbit.previewedOrbit.param.ra);
            arrayToSave[10] = UsefulFunctions.DoubleToString(UI_initOrbit.previewedOrbit.param.rp);
            arrayToSave[11] = UsefulFunctions.DoubleToString(UI_initOrbit.previewedOrbit.param.p);
            arrayToSave[12] = UsefulFunctions.DoubleToString(UI_initOrbit.previewedOrbit.param.e);
            arrayToSave[13] = UsefulFunctions.DoubleToString(UI_initOrbit.previewedOrbit.param.i);
            arrayToSave[14] = UsefulFunctions.DoubleToString(UI_initOrbit.previewedOrbit.param.lAscN);
            arrayToSave[15] = UsefulFunctions.DoubleToString(UI_initOrbit.previewedOrbit.param.omega);
            arrayToSave[16] = UsefulFunctions.DoubleToString(UI_initOrbit.previewedOrbit.param.nu);
        }
        else {
            for(int i=1; i<arrayToSave.Length; i++) { arrayToSave[i]="Nan"; }
        }
        OrbitalParamsSaveData orbParamsSaveData = new OrbitalParamsSaveData(arrayToSave);

        string filepath = Application.persistentDataPath + Filepaths.shipToLoad_orbitalParams;
        File.WriteAllText(filepath, JsonUtility.ToJson(orbParamsSaveData, true));
    }

    private void SaveShipSettingsToDisk()
    {
        // Save the needed SpaceshipSettings data to disk
        // Saved data file has the name 'shipToLoad_settings.json'
        UIStartLoc_InitPlanetarySurf UI_initSurface = section_InitPlanetarySurfaceGO.GetComponent<UIStartLoc_InitPlanetarySurf>();
        // Creating the string[] for the 'SpaceshipSettingsSaveData' struct
        // Refer to struct definition for the order of the variables to add to the string[]
        string[] arrayToSave = new string[SpaceshipSettingsSaveData.NB_PARAMS];
        arrayToSave[0] = "1000.0"; // Fake value for the ship mass
        
        if(isPLanetarySurfaceInitialization)
        {
            arrayToSave[1] = "1"; // True if the planetary surface was the last panel in the start location to be active
            arrayToSave[2] = UI_initSurface.currSelectedLaunchpad.Get_Lat_Long().ToString();
        }
        else {
            arrayToSave[1] = "0"; // Not a planetary init, thus an in-orbit init
            arrayToSave[2] = new Vector2(float.NaN, float.NaN).ToString();
        }
        SpaceshipSettingsSaveData shipSettingsSaveData = new SpaceshipSettingsSaveData(arrayToSave);
        UsefulFunctions.WriteToFileSpaceshipSettingsSaveData(shipSettingsSaveData);
    }


}