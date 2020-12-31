using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Mathd_Lib;
using System;
using UnityEngine.Events;
using System.IO;

using MathOps = CommonMethods.MathsOps;
using ObjHand = CommonMethods.ObjectsHandling;
using Planets = CelestialBodiesConstants;
using UniCsts = UniverseConstants;

public class UIStartLoc_InitPlanetarySurf : MonoBehaviour
{
    [HideInInspector] public string currPlanetSelectedName; // Only need the name of the planet
    public RectTransform planetMap; // Reference to the planet map to get its width/height
    [Tooltip("Prefab for the hardcoded launchPads from the 'LaunchPadList.cs'")]
    public GameObject launchPadPrefab; // Prefab used for the launchPads hardcoded in the LaunchPadList.cs file
    [Tooltip("Prefab for the launchPads added by the user")] 
    public GameObject launchPadPrefabCustom; // Prefab used for custom launchPads added by the user
    //===============================================
    [Header("Launch Pad Info UI Panel")]
    [Tooltip("GameObject named 'LocationInfoContent' under the 'LocationAdditionalInfo' panel")]
    public RectTransform location_infoContent_panel;
    public TMPro.TMP_Text lp_name_val;
    public TMPro.TMP_Text lp_country_val;
    public TMPro.TMP_Text lp_agency_val;
    public TMPro.TMP_Text lp_operationalDate_val;
    public TMPro.TMP_Text lp_longitude_val;
    public TMPro.TMP_Text lp_latitude_val;
    public TMPro.TMP_Text lp_eastwardBoost_val;
    //===============================================
    [Header("UI Elements to add LaunchPads")]
    [Tooltip("GameObject named 'LocationAddNewLaunchPad' under the 'LocationAdditionalInfo' panel")]
    public RectTransform location_addNewLaunchPad_panel;
    public TMPro.TMP_InputField lp_name_input;
    public TMPro.TMP_InputField lp_country_input;
    public TMPro.TMP_InputField lp_supervision_input;
    public TMPro.TMP_InputField lp_operationDate_input;
    public TMPro.TMP_Text lp_longitude_NEWLP_val;
    public TMPro.TMP_Text lp_latitude_NEWLP_val;
    public TMPro.TMP_Text lp_eastwardBoost_NEWLP_val;
    public Button lp_Cancel_btn;
    public Button lp_save_btn;
    //===============================================
    private Vector2 mapSize;
    private Vector3 mapBottomLeftCorner; // Reference point for the position of the launch pad sprites
    private Image planetMapImg;
    private List<GameObject> launchPadGOs;
    private List<LaunchPad> launchPadInstances;
    //===============================================
    // Bool to indicate if the right mouse button is held down within the map rect Transform to add a new launch pad
    // This value is modified by the 'UI_planetMap_PointerEvents' script attached to the 'Panel_PlanetMapSelection' UI GameObject
    [HideInInspector] public bool isAddingNewLaunchPad;
    private bool launchPadAlreadyInCreationAndMoving;
    private RectTransform currActiveUIInfoPanel; // The active panel of UI additional info. Either 'location_addNewLaunchPad_panel' or 'location_infoContent_panel'
    //===============================================
    public LaunchPad currSelectedLaunchpad;
    bool hasALaunchPadSelected;
    public StartLocPanelIsSetUp panelIsFullySetUp;

    void Start()
    {
        if(panelIsFullySetUp == null)
            panelIsFullySetUp = new StartLocPanelIsSetUp();

        hasALaunchPadSelected = false;
        UpdatePlanetaryMap();
        InitMapSizeVariables();
        Init_LaunchPad_Sprites();

        SelectLastLaunchPadInList();
    }
    public void UpdatePlanetaryMap()
    {
        Planets.planets currPlanet = ObjHand.Str_2_Planet(currPlanetSelectedName);
        double currBodyIsRocky = Planets.planetsDict[currPlanet][CelestialBodyParamsBase.otherParams.isRockyBody.ToString()].val;

        if(currBodyIsRocky == 1d)
        {
            // planet is rocky
            string pathToMap = Filepaths.RSC_UIPlanetaryMaps + currPlanetSelectedName;
            Sprite newSprite = Resources.Load<Sprite>(pathToMap);
            
            if(planetMapImg == null) {
                planetMapImg = planetMap.gameObject.GetComponent<Image>();
            }
            planetMapImg.sprite = newSprite;
            Init_LaunchPad_Sprites();
            launchPadGOs.Clear();
            launchPadInstances.Clear();
        }
        launchPadAlreadyInCreationAndMoving = false;
        SelectLastLaunchPadInList();
    }
    
    public void Init_LaunchPad_Sprites()
    {
        ClearAllInfoValues();
        RemoveAllLaunchPads();

        LoadDefaultLaunchPads();
        LoadCustomLaunchPads();
    }
    public void ClearAllInfoValues() {
        lp_name_val.text = "";
        lp_country_val.text = "";
        lp_agency_val.text = "";
        lp_operationalDate_val.text = "";
        lp_longitude_val.text = "";
        lp_latitude_val.text = "";
        lp_eastwardBoost_val.text = "";
    }
    private void RemoveAllLaunchPads() {
        foreach(Transform child in planetMap.transform)
            Destroy(child.gameObject);
    }
    void LoadDefaultLaunchPads()
    {
        // Load hardcoded locations
        Dictionary<string, Dictionary<LaunchPad.launchPadParams, string>> lpDict;
        lpDict = LaunchPadList.launchPadsDict[ObjHand.Str_2_Planet(currPlanetSelectedName)];
        if(lpDict.Count == 0)
            return;

        launchPadGOs = new List<GameObject>();
        launchPadInstances = new List<LaunchPad>();

        // launchPad positions are calculated relative to the bottom left corner of the map
        for(int i=0; i < lpDict.Count; i++) {
            KeyValuePair<string, Dictionary<LaunchPad.launchPadParams, string>> pair = lpDict.ElementAt(i);
            string launchPad_name = pair.Key;
            Dictionary<LaunchPad.launchPadParams, string> lp_dict = pair.Value;
            AddLaunchPad(lp_dict, false);
        }
    }
    void LoadCustomLaunchPads()
    {
        LaunchPad[] readLaunchPads = LaunchPad.ReadCustomLaunchPadsFromJSON();
        if(readLaunchPads == null)
            return;
        foreach(LaunchPad lp in readLaunchPads) {
            if(lp.planet.Equals(currPlanetSelectedName)) {
                lp.RebuildDict();
                AddLaunchPad(lp, true);
            }
        }
    }
    
    void AddLaunchPad(LaunchPad launchPad, bool addAsCustomLaunchPad)
    {
        Vector2 launchPad_XY_Pos = launchPad.LatitudeLongitude_2_XY(mapSize);
        Vector3 launchPadPos = new Vector3(launchPad_XY_Pos.x, launchPad_XY_Pos.y) + mapBottomLeftCorner;

        GameObject launchPad_GO;
        if(addAsCustomLaunchPad)
            launchPad_GO = Instantiate(launchPadPrefabCustom, launchPadPos, Quaternion.identity);
        else
            launchPad_GO = Instantiate(launchPadPrefab, launchPadPos, Quaternion.identity);
        
        launchPad_GO.name = launchPad.name;
        launchPad_GO.transform.parent = planetMap.gameObject.transform;

        Button launchPad_btn = launchPad_GO.GetComponent<Button>();
        launchPad_btn.onClick.AddListener(delegate { OnLaunchPadClick(launchPad); });

        launchPadGOs.Add(launchPad_GO);
        launchPadInstances.Add(launchPad);
    }
    void AddLaunchPad(Dictionary<LaunchPad.launchPadParams, string> lp_dict, bool addAsCustomLaunchPad)
    {
        LaunchPad launchPad = new LaunchPad(lp_dict);
        Vector2 launchPad_XY_Pos = launchPad.LatitudeLongitude_2_XY(mapSize);
        Vector3 launchPadPos = new Vector3(launchPad_XY_Pos.x, launchPad_XY_Pos.y) + mapBottomLeftCorner;

        GameObject launchPad_GO;
        if(addAsCustomLaunchPad)
            launchPad_GO = Instantiate(launchPadPrefabCustom, launchPadPos, Quaternion.identity);
        else
            launchPad_GO = Instantiate(launchPadPrefab, launchPadPos, Quaternion.identity);
        
        launchPad_GO.name = lp_dict[LaunchPad.launchPadParams.name];
        launchPad_GO.transform.SetParent(planetMap.gameObject.transform, true);

        Button launchPad_btn = launchPad_GO.GetComponent<Button>();
        launchPad_btn.onClick.AddListener(delegate { OnLaunchPadClick(launchPad); });

        launchPadGOs.Add(launchPad_GO);
        launchPadInstances.Add(launchPad);
    }
    
    void SelectLastLaunchPadInList()
    {
        // By default, display the last launchPad in the array
        if(launchPadGOs.Count > 0 && launchPadInstances.Count > 0) {
            launchPadGOs.ElementAt(launchPadGOs.Count-1).GetComponent<Button>().Select();
            OnLaunchPadClick(launchPadInstances.ElementAt(launchPadInstances.Count-1));
        }
    }

    private void InitMapSizeVariables()
    {
        Vector2 planetMapPanelSize = planetMap.parent.GetComponent<RectTransform>().sizeDelta;
        float mapWidth = planetMapPanelSize.x - planetMap.offsetMin.x - planetMap.offsetMax.x;
        float mapHeight = planetMapPanelSize.y - planetMap.offsetMin.y - planetMap.offsetMax.y;
        mapSize = new Vector2(mapWidth, mapHeight);

        mapBottomLeftCorner = new Vector3(planetMap.parent.position.x - mapSize.x/2, planetMap.parent.position.y - mapSize.y/2);
    }

    void OnLaunchPadClick(LaunchPad clickedLP)
    {
        currSelectedLaunchpad = clickedLP;
        hasALaunchPadSelected = true;

        lp_name_val.text = clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.name];
        lp_country_val.text = clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.country];
        lp_agency_val.text = clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.supervision];
        lp_operationalDate_val.text = clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.operationalDate];
        lp_longitude_val.text = MathOps.StringToSignificantDigits(clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.longitude], UniCsts.UI_SIGNIFICANT_DIGITS) + " °";
        lp_latitude_val.text = MathOps.StringToSignificantDigits(clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.latitude], UniCsts.UI_SIGNIFICANT_DIGITS) + " °";
        lp_eastwardBoost_val.text = MathOps.StringToSignificantDigits(clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.eastwardBoost], UniCsts.UI_SIGNIFICANT_DIGITS) + " m/s";
    
        // Sending 1 as the 'initPlanetary' identifier and 1 as the panel IS set-up
        if(panelIsFullySetUp != null)
            panelIsFullySetUp.Invoke(1, 1);
    }
    public bool TriggerPanelIsSetBoolEvent()
    {
        if(panelIsFullySetUp != null && hasALaunchPadSelected && !launchPadAlreadyInCreationAndMoving)
            panelIsFullySetUp.Invoke(1, Convert.ToInt32(hasALaunchPadSelected));
        return hasALaunchPadSelected;
    }
    //=============================================================================================
    //=============================================================================================
    //=============================================================================================
    void Update()
    {
        if(isAddingNewLaunchPad && Input.GetMouseButton(1))
        {
            // Pointer is withing the planetMap rectTransform & the right mouse button is held down
            // Adding a new sprite launchPad
            ClearAllInfoValues();
            if(!launchPadAlreadyInCreationAndMoving)
            {
                CreateNewEmptyLaunchPad();
                launchPadAlreadyInCreationAndMoving = true;
                // Enabling/disaling the UI panels
                location_infoContent_panel.gameObject.SetActive(false);
                location_addNewLaunchPad_panel.gameObject.SetActive(true);
                currActiveUIInfoPanel = location_addNewLaunchPad_panel;
                hasALaunchPadSelected = true;
            }
            else {
                // The launchPad is still movable by the user
                hasALaunchPadSelected = false;
                if(launchPadGOs.Count < 1)
                    return;
                UpdateLaunchPadInCreationPosition();
                UpdateLaunchPadInCreationLatitudeLongitude();
                UpdateLaunchPadInCreationInfoFields();
            }
        }
        else if(isAddingNewLaunchPad && !Input.GetMouseButton(1)) {
            // Right mouse button has been released but still withing the rectTransform
            launchPadAlreadyInCreationAndMoving = false;
        }
    }
    void CreateNewEmptyLaunchPad()
    {
        LaunchPad launchPad = new LaunchPad(LaunchPad.GetEmptyLaunchPadDict(currPlanetSelectedName));
        lp_name_input.text = LaunchPad.newLaunchPadDefaultName;

        GameObject launchPad_GO = Instantiate(launchPadPrefabCustom, new Vector3(Input.mousePosition.x, Input.mousePosition.y), Quaternion.identity);
        launchPad_GO.name = launchPad.launchPadParamsDict[LaunchPad.launchPadParams.name];
        launchPad_GO.transform.parent = planetMap.gameObject.transform;

        Button launchPad_btn = launchPad_GO.GetComponent<Button>();
        launchPad_btn.onClick.AddListener(delegate { OnLaunchPadClick(launchPad); });

        launchPadGOs.Add(launchPad_GO);
        launchPadInstances.Add(launchPad);

        UpdateLaunchPadInCreationLatitudeLongitude();
        launchPad.ComputeEastwardBoost();
    }

    public void AddNewLaunchPad_OnCancelClick()
    {
        launchPadInstances.RemoveAt(launchPadInstances.Count-1);
        GameObject.Destroy(launchPadGOs.ElementAt(launchPadGOs.Count-1));
        launchPadGOs.RemoveAt(launchPadGOs.Count-1);

        
        location_infoContent_panel.gameObject.SetActive(true);
        location_addNewLaunchPad_panel.gameObject.SetActive(false);
        currActiveUIInfoPanel = location_infoContent_panel;
        
        if(launchPadGOs.Count > 0) {
            launchPadGOs.ElementAt(launchPadGOs.Count-1).GetComponent<Button>().Select();
            OnLaunchPadClick(launchPadInstances.ElementAt(launchPadInstances.Count-1));
        }
    }
    public void AddNewLaunchPad_OnSaveClick()
    {
        LaunchPad lp = launchPadInstances.ElementAt(launchPadInstances.Count-1);
        lp.launchPadParamsDict[LaunchPad.launchPadParams.name] = lp_name_input.text;
        lp.launchPadParamsDict[LaunchPad.launchPadParams.country] = lp_country_input.text;
        lp.launchPadParamsDict[LaunchPad.launchPadParams.supervision] = lp_supervision_input.text;
        lp.launchPadParamsDict[LaunchPad.launchPadParams.operationalDate] = lp_operationDate_input.text;
        lp.InitVariables();

        bool ok_to_writeToJSON = AddNewLaunchPadCheckBeforeSaving(lp.name);
        if(!ok_to_writeToJSON) {
            Debug.Log("A launchPad with the same name and for planet" + currPlanetSelectedName + " has already been defined. Change the launchPad's name or the planet");
            return;
        }
        string filepath = AddLaunchPadDataToJSON(lp);

        string lpName = lp.launchPadParamsDict[LaunchPad.launchPadParams.name];
        Debug.Log(lpName + " launchPad succesfully saved at path " + filepath);

        location_infoContent_panel.gameObject.SetActive(true);
        location_addNewLaunchPad_panel.gameObject.SetActive(false);
        currActiveUIInfoPanel = location_infoContent_panel;

        launchPadGOs.ElementAt(launchPadGOs.Count-1).GetComponent<Button>().Select();
        OnLaunchPadClick(launchPadInstances.ElementAt(launchPadInstances.Count-1));
    }

    bool AddNewLaunchPadCheckBeforeSaving(string launchPadNameToCheck)
    {
        foreach(Planets.planets planetToCheck in Enum.GetValues(typeof(Planets.planets))) {
            // Checking custom LaunchPads and default ones at the same time
            // Lots of extra work as for each planet, it will check every custom LaunchPads (regardless of the planet)
            (LaunchPad,bool) output = LaunchPad.GetLaunchPadFromName(launchPadNameToCheck, planetToCheck, true);
            if(output.Item2)
                return false;
        }
        return true;
    }
    string AddLaunchPadDataToJSON(LaunchPad lauchPadToAdd)
    {
        LaunchPad[] arrayToWrite;
        LaunchPad[] prevLPs = LaunchPad.ReadCustomLaunchPadsFromJSON();
        if(prevLPs == null)
            arrayToWrite = new LaunchPad[1]; // There is only the new data to append to a new file
        else {
            arrayToWrite = new LaunchPad[prevLPs.Length + 1];
            // Copying previous launchPads to the array that will be written
            for(int i = 0; i < prevLPs.Length; i++)
                arrayToWrite[i] = prevLPs[i];
        }
        // Adding the new launchPad to the array
        arrayToWrite[arrayToWrite.Length-1] = lauchPadToAdd;
        string filepath = Application.persistentDataPath + Filepaths.userAdded_launchPads;
        File.WriteAllText(filepath, JsonHelper.ToJson<LaunchPad>(arrayToWrite, true));
        return filepath;
    }

    void UpdateLaunchPadInCreationInfoFields()
    {
        LaunchPad launchPad = launchPadInstances.ElementAt(launchPadInstances.Count-1);
        lp_longitude_NEWLP_val.text = MathOps.DoubleToSignificantDigits(launchPad.longitude, UniCsts.UI_SIGNIFICANT_DIGITS);
        lp_latitude_NEWLP_val.text = MathOps.DoubleToSignificantDigits(launchPad.latitude, UniCsts.UI_SIGNIFICANT_DIGITS);
        if(lp_latitude_NEWLP_val.text.Equals("90") || lp_latitude_NEWLP_val.text.Equals("-90"))
            lp_eastwardBoost_NEWLP_val.text = "0";
        else
            lp_eastwardBoost_NEWLP_val.text = MathOps.DoubleToSignificantDigits(launchPad.eastwardBoost, UniCsts.UI_SIGNIFICANT_DIGITS);
    }
    void UpdateLaunchPadInCreationPosition()
    {
        // The launchPad being created is the last element added to the List
        Vector3 newPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
        newPosition.x = Mathf.Clamp(newPosition.x, planetMap.transform.position.x - planetMap.rect.width/2f, planetMap.transform.position.x + planetMap.rect.width/2f);
        newPosition.y = Mathf.Clamp(newPosition.y, planetMap.transform.position.y - planetMap.rect.height/2f, planetMap.transform.position.y + planetMap.rect.height/2f);

        launchPadGOs.ElementAt(launchPadGOs.Count-1).transform.position = newPosition;
    }
    void UpdateLaunchPadInCreationLatitudeLongitude()
    {
        float mouseX = launchPadGOs.ElementAt(launchPadGOs.Count-1).transform.position.x - planetMap.position.x;
        float mouseY = launchPadGOs.ElementAt(launchPadGOs.Count-1).transform.position.y - planetMap.position.y;

        Vector2 mousePos = new Vector2(mouseX, mouseY);
        Vector2 mapSize = new Vector2(planetMap.rect.width, planetMap.rect.height);
        Vector2d latLong = LaunchPad.XY_2_LatitudeLongitude(mousePos, mapSize);

        LaunchPad launchPad = launchPadInstances.ElementAt(launchPadInstances.Count-1);
        launchPad.launchPadParamsDict[LaunchPad.launchPadParams.latitude] = MathOps.DoubleToString(latLong.x);
        launchPad.launchPadParamsDict[LaunchPad.launchPadParams.longitude] = MathOps.DoubleToString(latLong.y);
        launchPad.ComputeEastwardBoost();
        launchPad.InitVariables();
    }
}