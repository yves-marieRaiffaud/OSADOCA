using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Mathd_Lib;
using System;
using UnityEngine.EventSystems;
using System.IO;

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

    void Start()
    {
        UpdatePlanetaryMap();
        InitMapSizeVariables();
        Init_LaunchPad_Sprites();

        // By default, display the last launchPad in the array
        if(launchPadGOs.Count > 0 && launchPadInstances.Count > 0)
        {
            launchPadGOs.ElementAt(launchPadGOs.Count-1).GetComponent<Button>().Select();
            OnLaunchPadClick(launchPadInstances.ElementAt(launchPadInstances.Count-1));
        }
    }

    public void ClearAllInfoValues()
    {
        lp_name_val.text = "";
        lp_country_val.text = "";
        lp_agency_val.text = "";
        lp_operationalDate_val.text = "";
        lp_longitude_val.text = "";
        lp_latitude_val.text = "";
        lp_eastwardBoost_val.text = "";
    }

    public void UpdatePlanetaryMap()
    {
        UniCsts.planets currPlanet = UsefulFunctions.CastStringTo_Unicsts_Planets(currPlanetSelectedName);
        double currBodyIsRocky = UniCsts.planetsDict[currPlanet][CelestialBodyParamsBase.otherParams.isRockyBody.ToString()];

        if(currBodyIsRocky == 1d)
        {
            // planet is rocky
            string pathToMap = Filepaths.DEBUG_UIPlanetaryMaps + currPlanetSelectedName;
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
    }

    private void InitMapSizeVariables()
    {
        Vector2 planetMapPanelSize = planetMap.parent.GetComponent<RectTransform>().sizeDelta;
        float mapWidth = planetMapPanelSize.x - planetMap.offsetMin.x - planetMap.offsetMax.x;
        float mapHeight = planetMapPanelSize.y - planetMap.offsetMin.y - planetMap.offsetMax.y;
        mapSize = new Vector2(mapWidth, mapHeight);

        mapBottomLeftCorner = new Vector3(planetMap.parent.position.x - mapSize.x/2, planetMap.parent.position.y - mapSize.y/2);
    }

    private void RemoveAllLaunchPads()
    {
        foreach(Transform child in planetMap.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void Init_LaunchPad_Sprites()
    {
        ClearAllInfoValues();
        RemoveAllLaunchPads();

        LoadDefaultLaunchPads();
        LoadCustomLaunchPads();
    }
    //=================================================================
    //=================================================================
    //=================================================================
    private void LoadDefaultLaunchPads()
    {
        // Load hardcoded locations
        Dictionary<string, Dictionary<LaunchPad.launchPadParams, string>> lpDict;
        lpDict = LaunchPadList.launchPadsDict[UsefulFunctions.CastStringTo_Unicsts_Planets(currPlanetSelectedName)];
        if(lpDict.Count == 0) {
            return;
        }

        launchPadGOs = new List<GameObject>();
        launchPadInstances = new List<LaunchPad>();

        // launchPad positions are calculated relative to the bottom left corner of the map
        for(int i=0; i < lpDict.Count; i++)
        {
            KeyValuePair<string, Dictionary<LaunchPad.launchPadParams, string>> pair = lpDict.ElementAt(i);
            string launchPad_name = pair.Key;
            Dictionary<LaunchPad.launchPadParams, string> lp_dict = pair.Value;
            AddLaunchPad(lp_dict, false);
        }
    }

    private void LoadCustomLaunchPads()
    {
        // Load launchPads that have been added by the user
        string filepath = Application.persistentDataPath + Filepaths.userAdded_launchPads;
        if(!File.Exists(filepath)) { return; }

        string[] loadedData = File.ReadAllLines(filepath);
        int nbLinesBetweenPropertiesName = (1 + 1 + 2 + Enum.GetNames(typeof(LaunchPad.launchPadParams)).Length);
        int nbPropertiesInFile = Mathf.FloorToInt(loadedData.Length / nbLinesBetweenPropertiesName);

        int lineIdxOfNextLPProperty = 2; // Properties starts at line 3, so index is 2
        for(int count = 0; count < nbPropertiesInFile; count++)
        {
            string[] refPlanetLine = loadedData[lineIdxOfNextLPProperty + 3].Split(':');
            string refPlanetName = refPlanetLine[1].Substring(2, refPlanetLine[1].Length-4);
            if(!refPlanetName.Equals(currPlanetSelectedName))
            {
                lineIdxOfNextLPProperty += nbLinesBetweenPropertiesName;
                continue;
            }

            string propertyLine = loadedData[lineIdxOfNextLPProperty];
            Dictionary<LaunchPad.launchPadParams, string> lp_dict = LaunchPad.GetEmptyLaunchPadDict(currPlanetSelectedName);

            // property[0] is the key with '\t' in front
            // property[1] is the value with the ',' if it's not the last param in the class instance
            string[] stringArr = loadedData[lineIdxOfNextLPProperty + 2].Split(':');
            string stringVal = stringArr[1].Substring(2, stringArr[1].Length-3); // removing the comma and the '"' character
            lp_dict[LaunchPad.launchPadParams.isCustomLP] = stringVal;

            //stringArr = loadedData[lineIdxOfNextLPProperty + 3].Split(':');
            //stringVal = stringArr[1].Substring(2, stringArr[1].Length-4); // removing the comma
            lp_dict[LaunchPad.launchPadParams.refPlanet] = refPlanetName;

            stringArr = loadedData[lineIdxOfNextLPProperty + 4].Split(':');
            stringVal = stringArr[1].Substring(2, stringArr[1].Length-4); // removing the comma
            lp_dict[LaunchPad.launchPadParams.name] = stringVal;

            stringArr = loadedData[lineIdxOfNextLPProperty + 5].Split(':');
            stringVal = stringArr[1].Substring(2, stringArr[1].Length-4); // removing the comma
            lp_dict[LaunchPad.launchPadParams.country] = stringVal;

            stringArr = loadedData[lineIdxOfNextLPProperty + 6].Split(':');
            stringVal = stringArr[1].Substring(2, stringArr[1].Length-4); // removing the comma
            lp_dict[LaunchPad.launchPadParams.operationalDate] = stringVal;

            stringArr = loadedData[lineIdxOfNextLPProperty + 7].Split(':');
            stringVal = stringArr[1].Substring(2, stringArr[1].Length-4); // removing the comma
            lp_dict[LaunchPad.launchPadParams.supervision] = stringVal;

            stringArr = loadedData[lineIdxOfNextLPProperty + 8].Split(':');
            stringVal = stringArr[1].Substring(2, stringArr[1].Length-4); // removing the comma
            lp_dict[LaunchPad.launchPadParams.latitude] = stringVal;

            stringArr = loadedData[lineIdxOfNextLPProperty + 9].Split(':');
            stringVal = stringArr[1].Substring(2, stringArr[1].Length-4); // removing the comma
            lp_dict[LaunchPad.launchPadParams.longitude] = stringVal;

            stringArr = loadedData[lineIdxOfNextLPProperty + 10].Split(':');
            stringVal = stringArr[1].Substring(2, stringArr[1].Length-3); // there is no commo here
            lp_dict[LaunchPad.launchPadParams.eastwardBoost] = stringVal;

            AddLaunchPad(lp_dict, true);

            lineIdxOfNextLPProperty += nbLinesBetweenPropertiesName;
        }
    }

    private void OnLaunchPadClick(LaunchPad clickedLP)
    {
        currSelectedLaunchpad = clickedLP;

        lp_name_val.text = clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.name];
        lp_country_val.text = clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.country];
        lp_agency_val.text = clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.supervision];
        lp_operationalDate_val.text = clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.operationalDate];
        lp_longitude_val.text = UsefulFunctions.StringToSignificantDigits(clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.longitude], UniCsts.UI_SIGNIFICANT_DIGITS) + " °";
        lp_latitude_val.text = UsefulFunctions.StringToSignificantDigits(clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.latitude], UniCsts.UI_SIGNIFICANT_DIGITS) + " °";
        lp_eastwardBoost_val.text = UsefulFunctions.StringToSignificantDigits(clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.eastwardBoost], UniCsts.UI_SIGNIFICANT_DIGITS) + " m/s";
    }

    private void AddLaunchPad(Dictionary<LaunchPad.launchPadParams, string> lp_dict, bool addAsCustomLaunchPad)
    {
        LaunchPad launchPad = new LaunchPad(lp_dict);
        Vector2 launchPad_XY_Pos = launchPad.LatitudeLongitude_2_XY(mapSize);
        Vector3 launchPadPos = new Vector3(launchPad_XY_Pos.x, launchPad_XY_Pos.y) + mapBottomLeftCorner;

        GameObject launchPad_GO;
        if(addAsCustomLaunchPad)
        {
            launchPad_GO = Instantiate(launchPadPrefabCustom, launchPadPos, Quaternion.identity);
        }
        else {
            launchPad_GO = Instantiate(launchPadPrefab, launchPadPos, Quaternion.identity);
        }
        
        launchPad_GO.name = lp_dict[LaunchPad.launchPadParams.name];
        launchPad_GO.transform.parent = planetMap.gameObject.transform;

        Button launchPad_btn = launchPad_GO.GetComponent<Button>();
        launchPad_btn.onClick.AddListener(delegate { OnLaunchPadClick(launchPad); });

        launchPadGOs.Add(launchPad_GO);
        launchPadInstances.Add(launchPad);
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
            }
            else {
                // The launchPad is still movable by the user
                if(launchPadGOs.Count < 1) { return; }
                UpdateLaunchPadInCreationPosition();
                UpdateLaunchPadInCreationLatitudeLongitude();
                UpdateLaunchPadInCreationInfoFields();
            }
        }
        else if(isAddingNewLaunchPad && !Input.GetMouseButton(1))
        {
            // Right mouse button has been released but still withing the rectTransform
            launchPadAlreadyInCreationAndMoving = false;
        }
    }

    public void AddNewLaunchPad_OnCancelClick()
    {
        launchPadInstances.RemoveAt(launchPadInstances.Count-1);
        GameObject.Destroy(launchPadGOs.ElementAt(launchPadGOs.Count-1));
        launchPadGOs.RemoveAt(launchPadGOs.Count-1);

        
        location_infoContent_panel.gameObject.SetActive(true);
        location_addNewLaunchPad_panel.gameObject.SetActive(false);
        currActiveUIInfoPanel = location_infoContent_panel;
        
        if(launchPadGOs.Count > 0)
        {
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

        bool ok_to_writeToJSON = AddNewLaunchPadCheckBeforeSaving(lp.GetJSONLaunchPadPropertyName());
        if(!ok_to_writeToJSON)
        {
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

    private bool AddNewLaunchPadCheckBeforeSaving(string launchPadNameToCheck)
    {
        string filepath = Application.persistentDataPath + Filepaths.userAdded_launchPads;
        if(!File.Exists(filepath)) { return true; }

        string[] loadedData = File.ReadAllLines(filepath);
        int nbLinesBetweenPropertiesName = (1 + 1 + 2 + Enum.GetNames(typeof(LaunchPad.launchPadParams)).Length);
        int nbPropertiesInFile = Mathf.FloorToInt(loadedData.Length / nbLinesBetweenPropertiesName);

        int lineIdxOfNextLPProperty = 2; // Properties starts at line 3, so index is 2
        for(int count = 0; count < nbPropertiesInFile; count++)
        {
            string line = loadedData[lineIdxOfNextLPProperty];
            string[] property = line.Split('\"');
            // property[0] is the '\t' (tab) character
            // property[1] is the desired propertyName
            // property[2] is the ':' after the property name
            if(property.Length > 1)
            {
                if(launchPadNameToCheck.Equals(property[1]))
                {
                    return false;
                }
            }
            lineIdxOfNextLPProperty += nbLinesBetweenPropertiesName;
        }
        return true;
    }

    private string AddLaunchPadDataToJSON(LaunchPad lauchPadToAdd)
    {
        string filepath = Application.persistentDataPath + Filepaths.userAdded_launchPads;

        string[] previousTxtNoEndBracket = new string[] { "{\n" };
        string previousText;
        if(!File.Exists(filepath))
        {
            FileStream fs = File.Create(filepath);
            fs.Close();
            previousText = string.Join("", previousTxtNoEndBracket);
        }
        else {
            string[] tmp = File.ReadAllLines(filepath);
            previousTxtNoEndBracket = UsefulFunctions.GetSubArray(tmp, 0, tmp.Length-2);
            previousText = string.Join("\n", previousTxtNoEndBracket) + ",\n";
        }

        string newLPTxt = lauchPadToAdd.LaunchPadDataToString();

        string newJSONTxt = previousText + newLPTxt + "\n}";
        File.WriteAllText(filepath, newJSONTxt);
        return filepath;
    }

    private void UpdateLaunchPadInCreationInfoFields()
    {
        Dictionary<LaunchPad.launchPadParams, string> lp_paramDict = launchPadInstances.ElementAt(launchPadInstances.Count-1).launchPadParamsDict;
        lp_longitude_NEWLP_val.text = UsefulFunctions.StringToSignificantDigits(lp_paramDict[LaunchPad.launchPadParams.longitude], UniCsts.UI_SIGNIFICANT_DIGITS);
        lp_latitude_NEWLP_val.text = UsefulFunctions.StringToSignificantDigits(lp_paramDict[LaunchPad.launchPadParams.latitude], UniCsts.UI_SIGNIFICANT_DIGITS);
        if(lp_latitude_NEWLP_val.text.Equals("90") || lp_latitude_NEWLP_val.text.Equals("-90"))
        {
            lp_eastwardBoost_NEWLP_val.text = "0";
        }
        else {
            lp_eastwardBoost_NEWLP_val.text = UsefulFunctions.StringToSignificantDigits(lp_paramDict[LaunchPad.launchPadParams.eastwardBoost], UniCsts.UI_SIGNIFICANT_DIGITS);
        }
    }

    private void UpdateLaunchPadInCreationPosition()
    {
        // The launchPad being created is the last element added to the List
        Vector3 newPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
        newPosition.x = Mathf.Clamp(newPosition.x, planetMap.transform.position.x - planetMap.rect.width/2f, planetMap.transform.position.x + planetMap.rect.width/2f);
        newPosition.y = Mathf.Clamp(newPosition.y, planetMap.transform.position.y - planetMap.rect.height/2f, planetMap.transform.position.y + planetMap.rect.height/2f);

        launchPadGOs.ElementAt(launchPadGOs.Count-1).transform.position = newPosition;
    }

    private void UpdateLaunchPadInCreationLatitudeLongitude()
    {
        float mouseX = launchPadGOs.ElementAt(launchPadGOs.Count-1).transform.position.x - planetMap.position.x;
        float mouseY = launchPadGOs.ElementAt(launchPadGOs.Count-1).transform.position.y - planetMap.position.y;

        Vector2 mousePos = new Vector2(mouseX, mouseY);
        Vector2 mapSize = new Vector2(planetMap.rect.width, planetMap.rect.height);
        Vector2d latLong = LaunchPad.XY_2_LatitudeLongitude(mousePos, mapSize);

        LaunchPad launchPad = launchPadInstances.ElementAt(launchPadInstances.Count-1);
        launchPad.launchPadParamsDict[LaunchPad.launchPadParams.latitude] = UsefulFunctions.DoubleToString(latLong.x);
        launchPad.launchPadParamsDict[LaunchPad.launchPadParams.longitude] = UsefulFunctions.DoubleToString(latLong.y);
        launchPad.ComputeEastwardBoost();
    }

    private void CreateNewEmptyLaunchPad()
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

}