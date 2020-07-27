using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Mathd_Lib;
using UnityEngine.EventSystems;

public class UIStartLoc_InitPlanetarySurf : MonoBehaviour
{
    [HideInInspector] public string currPlanetSelectedName; // Only need the name of the planet
    public RectTransform planetMap; // Reference to the planet map to get its width/height
    public GameObject launchPadPrefab;
    //===============================================
    [Header("Launch Pad Info UI Panel")]
    public TMPro.TMP_Text lp_name_val;
    public TMPro.TMP_Text lp_country_val;
    public TMPro.TMP_Text lp_agency_val;
    public TMPro.TMP_Text lp_operationalDate_val;
    public TMPro.TMP_Text lp_longitude_val;
    public TMPro.TMP_Text lp_latitude_val;
    public TMPro.TMP_Text lp_eastwardBoost_val;
    //===============================================
    private Vector2 mapSize;
    private Vector3 mapBottomLeftCorner; // Reference point for the position of the launch pad sprites
    private Image planetMapImg;
    private GameObject[] launchPadGOs;
    private LaunchPad[] launchPadInstances;

    void Start()
    {
        UpdatePlanetaryMap();
        InitMapSizeVariables();
        Init_LaunchPad_Sprites();
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
            string pathToMap = "CelestialBodies/TextureFiles/UIPlanetary_Maps/" + currPlanetSelectedName;
            Sprite newSprite = Resources.Load<Sprite>(pathToMap);
            
            if(planetMapImg == null) {
                planetMapImg = planetMap.gameObject.GetComponent<Image>();
            }
            planetMapImg.sprite = newSprite;
            Init_LaunchPad_Sprites();
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

        Dictionary<string, Dictionary<LaunchPad.launchPadParams, string>> lpDict;
        lpDict = LaunchPadList.launchPadsDict[UsefulFunctions.CastStringTo_Unicsts_Planets(currPlanetSelectedName)];
        if(lpDict.Count == 0) {
            return;
        }

        launchPadGOs = new GameObject[lpDict.Count];
        launchPadInstances = new LaunchPad[lpDict.Count];

        // launchPad positions are calculated relative to the bottom left corner of the map
        for(int i=0; i < lpDict.Count; i++)
        {
            KeyValuePair<string, Dictionary<LaunchPad.launchPadParams, string>> pair = lpDict.ElementAt(i);
            string launchPad_name = pair.Key;
            Dictionary<LaunchPad.launchPadParams, string> lp_dict = pair.Value;

            LaunchPad launchPad = new LaunchPad(lp_dict);
            Vector2 launchPad_XY_Pos = launchPad.LatitudeLongitude_2_XY(mapSize);
            Vector3 launchPadPos = new Vector3(launchPad_XY_Pos.x, launchPad_XY_Pos.y) + mapBottomLeftCorner;

            GameObject launchPad_GO = Instantiate(launchPadPrefab, launchPadPos, Quaternion.identity);
            launchPad_GO.name = launchPad_name;
            launchPad_GO.transform.parent = planetMap.gameObject.transform;

            Button launchPad_btn = launchPad_GO.GetComponent<Button>();
            launchPad_btn.onClick.AddListener(delegate { OnLaunchPadClick(launchPad); });

            launchPadGOs[i] = launchPad_GO;
            launchPadInstances[i] = launchPad;
        }
    }

    private void OnLaunchPadClick(LaunchPad clickedLP)
    {
        lp_name_val.text = clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.name];
        lp_country_val.text = clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.country];
        lp_agency_val.text = clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.supervision];
        lp_operationalDate_val.text = clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.operationalDate];
        lp_longitude_val.text = UsefulFunctions.StringToSignificantDigits(clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.longitude], UniCsts.UI_SIGNIFICANT_DIGITS) + " °";
        lp_latitude_val.text = UsefulFunctions.StringToSignificantDigits(clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.latitude], UniCsts.UI_SIGNIFICANT_DIGITS) + " °";
        lp_eastwardBoost_val.text = UsefulFunctions.StringToSignificantDigits(clickedLP.launchPadParamsDict[LaunchPad.launchPadParams.eastwardBoost], UniCsts.UI_SIGNIFICANT_DIGITS) + " m/s";
    }
}