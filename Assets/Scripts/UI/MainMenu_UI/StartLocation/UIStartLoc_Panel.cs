using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using MathOps = CommonMethods.MathsOps;
using ObjHand = CommonMethods.ObjectsHandling;

public class UIStartLoc_Panel : MonoBehaviour
{
    [Header("UI GameObjects/RectTransform")]
    public GameObject section_InitPlanetarySurfaceGO;
    UIStartLoc_InitPlanetarySurf UI_initPlanetaryScript;

    public GameObject section_InitOrbitGO;
    UIStartLoc_InitOrbit UI_initOrbitScript;

    // Empty Gameobject containing every simple spheres
    public GameObject simpleSpheresFolder;
    // Dropdowns
    [Header("Dropdowns")]
    public TMPro.TMP_Dropdown startLocInitTypeDropdown;
    public TMPro.TMP_Dropdown startLocPlanetSelectorDropdown;
    OnPlanetSelectValueChange _onPlanetSelectionValueChange;
    public OnPlanetSelectValueChange onPlanetSelectionValueChange
    {
        get {
            return _onPlanetSelectionValueChange;
        }
    }
    //----------------------
    //----------------------
    [HideInInspector] public MainPanelIsSetUp panelIsFullySetUp;
    private string lastSelectedPlanetName;
    //----------------------
    //----------------------
    [HideInInspector] public enum startLocInitType { inOrbit, planetarySurface };
    [HideInInspector] public Dictionary<startLocInitType, string> startLocInitTypeDict = new Dictionary<startLocInitType, string> {
        { startLocInitType.inOrbit, "In Orbit" },
        { startLocInitType.planetarySurface, "Planetary Surface" }
    };
    //----------------------
    //----------------------
    [HideInInspector] public bool isPLanetarySurfaceInitialization;
    [HideInInspector] public bool hasFinishedStart;
    //----------------------
    //----------------------
    void Start()
    {
        hasFinishedStart = false;
        if(panelIsFullySetUp == null)
            panelIsFullySetUp = new MainPanelIsSetUp();
        if(_onPlanetSelectionValueChange == null)
            _onPlanetSelectionValueChange = new OnPlanetSelectValueChange();

        // By default, sending 0 to indicate that things need to be configured by the user
        if(panelIsFullySetUp != null)
                panelIsFullySetUp.Invoke(0, 0);

        UI_initOrbitScript = section_InitOrbitGO.GetComponent<UIStartLoc_InitOrbit>();
        UI_initOrbitScript.panelIsFullySetUp.AddListener(OnPanelFullySetUp_Event);

        UI_initPlanetaryScript = section_InitPlanetarySurfaceGO.GetComponent<UIStartLoc_InitPlanetarySurf>();
        UI_initPlanetaryScript.panelIsFullySetUp.AddListener(OnPanelFullySetUp_Event);

        // Init start location panel with Earth as default planet
        lastSelectedPlanetName = CelestialBodiesConstants.planets.Earth.ToString();

        Init_startLocInitTypeDropdown();
        startLocInitTypeDropdown.onValueChanged.AddListener(delegate { OnValueChangedInitTypeDropdown(); });

        Init_startLocPlanetSelectorDropdown();
        startLocPlanetSelectorDropdown.onValueChanged.AddListener(delegate { OnValueChangedPlanetSelectorDropdown(); });

        InitSimpleSpheres();

        hasFinishedStart = true;
    }
    void InitSimpleSpheres()
    {
        // Spawn every simpleSpheres in the folder, and set the 'orbitedBody' variable
        foreach(Transform child in simpleSpheresFolder.transform) {
            CelestialBody celestBody = child.gameObject.GetComponent<CelestialBody>();
            celestBody.Init_CelestialBodySettings();
        }
    }
    //-----------------------------------------------------------
    //-----------------------------------------------------------
    void Init_startLocInitTypeDropdown()
    {
        startLocInitTypeDropdown.ClearOptions();
        startLocInitTypeDropdown.AddOptions(new List<string>(startLocInitTypeDict.Values));
    }
    void OnValueChangedInitTypeDropdown()
    {
        switch(startLocInitTypeDropdown.value)
        {
            case 0:
                // 'In Orbit' initialisation is selected
                section_InitOrbitGO.SetActive(true);
                section_InitPlanetarySurfaceGO.SetActive(false);
                isPLanetarySurfaceInitialization = false;
                break;
            
            case 1:
                // 'Planetary surface' initialisation is selected
                section_InitOrbitGO.SetActive(false);
                section_InitPlanetarySurfaceGO.SetActive(true);
                isPLanetarySurfaceInitialization = true;
                break;
        }
        // Need to update the dropdown of the possible start planets
        Init_startLocPlanetSelectorDropdown();
        UI_initPlanetaryScript.currPlanetSelectedName = startLocPlanetSelectorDropdown.options[startLocPlanetSelectorDropdown.value].text;
        UI_initPlanetaryScript.UpdatePlanetaryMap();

        SendControlBarTriangleUpdate();
    }
    void Init_startLocPlanetSelectorDropdown()
    {
        CelestialBodiesConstants.planets lastBody = ObjHand.Str_2_Planet(lastSelectedPlanetName);
        double lastSelectedBodyIsRocky = CelestialBodiesConstants.planetsDict[lastBody][CelestialBodyParamsBase.otherParams.isRockyBody.ToString()].val;
        int dropdownIdxToSet = 0;
        int bodyIndex = 0;

        startLocPlanetSelectorDropdown.ClearOptions();

        List<string> possibleBodies = new List<string>();
        switch(startLocInitTypeDropdown.value)
        {
            case 0:
                // 'In-orbit' init
                // Taking every planets and body, its does not matter if the body is rocky or not
                foreach(CelestialBodiesConstants.planets body in CelestialBodiesConstants.planetsDict.Keys) {
                    string bodyName = body.ToString();
                    possibleBodies.Add(bodyName);
                    if(lastSelectedPlanetName.Equals(bodyName))
                        dropdownIdxToSet = bodyIndex;
                    bodyIndex += 1;
                }
                break;
            case 1:
                // 'Planetary surface' init
                // Taking only the rocky bodies for planetary surface init
                foreach(KeyValuePair<CelestialBodiesConstants.planets, Dictionary<string, UnitInterface>> body_KV_Pair in CelestialBodiesConstants.planetsDict) {
                    if(body_KV_Pair.Value[CelestialBodyParamsBase.otherParams.isRockyBody.ToString()].val == 1d) {
                        // Body is rocky
                        string bodyNameToAdd = body_KV_Pair.Key.ToString();
                        possibleBodies.Add(bodyNameToAdd);

                        if(lastSelectedBodyIsRocky == 1d && lastSelectedPlanetName.Equals(bodyNameToAdd))
                            dropdownIdxToSet = bodyIndex; // The corresponding planet has been found
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
    void OnValueChangedPlanetSelectorDropdown()
    {
        // Display the selected planet and hide the others
        // Suffix used for every simpleSphere GameObject. Suffix to add after the name of the planet starting with a capital letter
        string dropdownText = startLocPlanetSelectorDropdown.options[startLocPlanetSelectorDropdown.value].text;
        string goToFind = dropdownText;

        UI_initPlanetaryScript.currPlanetSelectedName = dropdownText;
        UI_initPlanetaryScript.UpdatePlanetaryMap();
        if(_onPlanetSelectionValueChange != null)
            _onPlanetSelectionValueChange.Invoke(dropdownText);

        foreach(Transform child in simpleSpheresFolder.transform)
        {
            if(child.name.Equals(goToFind)) {
                child.gameObject.SetActive(true);
                UI_initOrbitScript.orbitedBody = child.gameObject.GetComponent<CelestialBody>();
            }
            else {
                child.gameObject.SetActive(false);
            }
        }

        UI_initOrbitScript.UpdateApsidesNames();
        // Redraw the orbit to take into account the change of planet
        if(!UI_initOrbitScript.InputFieldsAreAllEmpty())
            // Update only if some values have been entered. Avoid drawing an orbit of ra=rp=0 km when first selecting the planet
            UI_initOrbitScript.OnUpdateOrbit_BtnClick();

        lastSelectedPlanetName = startLocPlanetSelectorDropdown.options[startLocPlanetSelectorDropdown.value].text;
    }
    //-----------------------------------------------------------
    //-----------------------------------------------------------
    void OnPanelFullySetUp_Event(int panelIdentifier, int boolPanelIsSetUp)
    {
        if((panelIdentifier == 0 && section_InitOrbitGO.activeSelf) || (panelIdentifier == 1 && section_InitPlanetarySurfaceGO.activeSelf)) {
            // Sending 0 as the 'startLocation' panel identifier
            if(panelIsFullySetUp != null)
                panelIsFullySetUp.Invoke(0, boolPanelIsSetUp);
        }
    }
    //-----------------------------------------------------------
    //-----------------------------------------------------------
    // PUBLIC METHODS CALLED FROM THE 'UIMainMenu' Script
    public void On_FLY_click_GatherOrbitalParams()
    {
        // Called when the 'FLY' button is pressed.
        // Gathers the initialisation data and saved the data to a file stored on the disk.
        // This file will then be read in the simulation scene and its values copied to the corresponding spacecraft scriptable object
        SaveOrbitalParamsDataToDisk();
        SaveShipSettingsToDisk();
        Debug.Log("Ship OrbitalParams & Ship Settings have been saved");
    }
    public bool SendControlBarTriangleUpdate()
    {
        if(section_InitOrbitGO.activeSelf)
            return UI_initOrbitScript.TriggerPanelIsSetBoolEvent();

        else if(section_InitPlanetarySurfaceGO.activeSelf)
            return UI_initPlanetaryScript.TriggerPanelIsSetBoolEvent();
        else
            return false;
    }
    //-----------------------------------------------------------
    //-----------------------------------------------------------
    public void SaveOrbitalParamsDataToDisk()
    {
        // Save the needed orbitalParams data to disk
        // Saved data file has the name 'shipToLoad_orbitalParams.json'

        // Creating the string[] for the 'OrbitalParamsSaveData' struct
        // Refer to struct definition for the order of the variables to add to the string[]
        string[] arrayToSave = new string[OrbitalParamsSaveData.NB_PARAMS];

        arrayToSave[0] = startLocPlanetSelectorDropdown.options[startLocPlanetSelectorDropdown.value].text;
        if(!isPLanetarySurfaceInitialization) {
            arrayToSave[1] = UI_initOrbitScript.orbitDefType.value.ToString();
            arrayToSave[2] = UI_initOrbitScript.bodyPosType.value.ToString();
            arrayToSave[3] = UI_initOrbitScript.unitsDropdown.value.ToString();
            arrayToSave[4] = "0"; // Defining a real orbit, not a predicted one
            arrayToSave[5] = "0"; // For the moment, don't draw any vectors/directions
            arrayToSave[6] = "1"; // default as true for drawOrbit bool
            arrayToSave[7] = "0"; // default as false for drawDirections bool
            arrayToSave[8] = "300"; //default value for the orbitDrawingResolution
            arrayToSave[9] = MathOps.DoubleToString(UI_initOrbitScript.previewedOrbit.param.ra);
            arrayToSave[10] = MathOps.DoubleToString(UI_initOrbitScript.previewedOrbit.param.rp);
            arrayToSave[11] = MathOps.DoubleToString(UI_initOrbitScript.previewedOrbit.param.p);
            arrayToSave[12] = MathOps.DoubleToString(UI_initOrbitScript.previewedOrbit.param.e);
            arrayToSave[13] = MathOps.DoubleToString(UI_initOrbitScript.previewedOrbit.param.i);
            arrayToSave[14] = MathOps.DoubleToString(UI_initOrbitScript.previewedOrbit.param.lAscN);
            arrayToSave[15] = MathOps.DoubleToString(UI_initOrbitScript.previewedOrbit.param.omega);
            arrayToSave[16] = MathOps.DoubleToString(UI_initOrbitScript.previewedOrbit.param.nu);
        }
        else {
            arrayToSave[1] = "0";
            arrayToSave[2] = "0";
            arrayToSave[3] = "0";
            arrayToSave[4] = "0"; // Defining a real orbit, not a predicted one
            arrayToSave[5] = "0"; // For the moment, don't draw any vectors/directions
            arrayToSave[6] = "0"; // default as true for drawOrbit bool
            arrayToSave[7] = "0"; // default as false for drawDirections bool
            arrayToSave[8] = "300"; //default value for the orbitDrawingResolution
            arrayToSave[9] = "0";
            arrayToSave[10] = "0";
            arrayToSave[11] = "0";
            arrayToSave[12] = "0";
            arrayToSave[13] = "0";
            arrayToSave[14] = "0";
            arrayToSave[15] = "0";
            arrayToSave[16] = "0";
        }
        OrbitalParamsSaveData orbParamsSaveData = new OrbitalParamsSaveData(arrayToSave);

        string filepath = Application.persistentDataPath + Filepaths.shipToLoad_orbitalParams;
        File.WriteAllText(filepath, JsonUtility.ToJson(orbParamsSaveData, true));
    }
    void SaveShipSettingsToDisk()
    {
        // Save the needed SpaceshipSettings data to disk
        // Saved data file has the name 'shipToLoad_settings.json'
        
        // Creating the string[] for the 'SpaceshipSettingsSaveData' struct
        // Refer to struct definition for the order of the variables to add to the string[]
        /*string[] arrayToSave = new string[SpaceshipSettingsSaveData.NB_PARAMS];
        arrayToSave[0] = "1000.0"; // Fake value for the ship mass
        
        if(isPLanetarySurfaceInitialization)
        {
            arrayToSave[1] = "1"; // True if the planetary surface was the last panel in the start location to be active
            arrayToSave[2] = UI_initPlanetaryScript.currSelectedLaunchpad.Get_Lat_Long().ToString();
        }
        else {
            arrayToSave[1] = "0"; // Not a planetary init, thus an in-orbit init
            arrayToSave[2] = new Vector2(float.NaN, float.NaN).ToString();
        }
        SpaceshipSettingsSaveData shipSettingsSaveData = new SpaceshipSettingsSaveData(arrayToSave);
        UsefulFunctions.WriteToFileSpaceshipSettingsSaveData(shipSettingsSaveData);*/
    }
}