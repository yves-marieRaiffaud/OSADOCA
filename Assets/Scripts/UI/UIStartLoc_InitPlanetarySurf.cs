using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Mathd_Lib;
using UnityEngine.EventSystems;

public class UIStartLoc_InitPlanetarySurf : MonoBehaviour
{
    public CelestialBody orbitedBody;
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
    private GameObject[] launchPadGOs;
    private LaunchPad[] launchPadInstances;

    void Start()
    {
        InitMapSizeVariables();
        Init_LaunchPad_Sprites();

        // By default click on the Kourou launch pad
        Button defaultSelected  = launchPadGOs[0].GetComponent<Button>();
        defaultSelected.Select();
        OnLaunchPadClick(launchPadInstances[0]);
    }

    private void InitMapSizeVariables()
    {
        Vector2 planetMapPanelSize = planetMap.parent.GetComponent<RectTransform>().sizeDelta;
        float mapWidth = planetMapPanelSize.x - planetMap.offsetMin.x - planetMap.offsetMax.x;
        float mapHeight = planetMapPanelSize.y - planetMap.offsetMin.y - planetMap.offsetMax.y;
        mapSize = new Vector2(mapWidth, mapHeight);

        mapBottomLeftCorner = new Vector3(planetMap.parent.position.x - mapSize.x/2, planetMap.parent.position.y - mapSize.y/2);
    }

    private void Init_LaunchPad_Sprites()
    {
        launchPadGOs = new GameObject[LaunchPadList.earth_launchPads.Count];
        launchPadInstances = new LaunchPad[LaunchPadList.earth_launchPads.Count];

        // launchPad positions are calculated relative to the bottom left corner of the map
        for(int i=0; i < LaunchPadList.earth_launchPads.Count; i++)
        {
            KeyValuePair<string, Dictionary<LaunchPad.launchPadParams, string>> pair = LaunchPadList.earth_launchPads.ElementAt(i);
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