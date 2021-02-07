using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using ObjHand = CommonMethods.ObjectsHandling;
using Planets = CelestialBodiesConstants;
using Vectrosity;
using System.Linq;
using System;
using GT = MissionAnalysis.GroundTracks;
using MA = Mission_Analysis_Plots;
using MSDropdownNamespace;
using MathsOps = CommonMethods.MathsOps;

public class UI_MissionAnalysis_Panel : MonoBehaviour
{
    [Tooltip("UIStartLoc_Panel Script of the 'UIStartLoc_Panel'")]
    public UIStartLoc_Panel startLocPanelScript;
    [Tooltip("RectTransform of the 'Panel_InitType'")]
    public RectTransform initOrbitType_RT;
    [Tooltip("RectTransform of the 'Section_InitOrbit'")]
    public RectTransform sectionInitOrbit_RT;
    [Tooltip("RectTransform of the 'Panel_OrbitDef'")]
    public RectTransform panelOrbitDef_RT;
    [Tooltip("RectTransform of the 'Panel_OrbitPreview'")]
    public RectTransform panelOrbitPreview_RT;
    
    [Tooltip("RectTransform of the 'Panel_PlanetMap'")]
    public RectTransform panelPlanetMap_RT;
    internal Image planetMap;
    Transform planetMapRT;

    [Tooltip("Button of the 'HideShow_OritPreview_btn'")]
    public Button hideShowOrbitPreview_btn;
    TMP_Text hideShow3DOrbit_btnTxt;
    bool orbt3DIsShown=true;

    float transitionsDur = 0.5f;
    UIStartLoc_InitOrbit initOrbitScript;
    Mission_Analysis_Plots maPlots;

    [Tooltip("Prefab for the Ground Track UILineRenderer")]
    public GameObject groundTrack_prefab;
    internal List<VectorObject2D> _lrArr;
    List<MA.XYLatLongTime> _groundTracksData;
    GameObject ma_GridGO;

    // Array of Colors & current index of the last used Color
    int currGtColorIdx=0;
    Color[] gtColors = new Color[] { Color.magenta, Color.yellow, Color.green, Color.red };

    [Tooltip("TMP_Dropdown 'GroundTrackType_Dropdown' to select the GT type to plot (body-fixed or body-rotating)")]
    public TMP_Dropdown gtType_Drop;
    [Tooltip("MSDropdown 'PointsofInterest_MS_Dropdown' to select the multiple interesting points to display")]
    public MSDropdown gtPOI_MSDrop;
    [Tooltip("Prefab GameObject for the PointOfInterest for the GroundTracks")]
    public GameObject poi_prefab;

    public Slider nbOrbits_slider;
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    void Start()
    {
        initOrbitScript = sectionInitOrbit_RT.GetComponent<UIStartLoc_InitOrbit>();
        planetMapRT = panelPlanetMap_RT.Find("PlanetMap");
        planetMap = planetMapRT.GetComponent<Image>();
        ma_GridGO = planetMap.transform.Find("Grid").gameObject;

        SetUp_GT_Type_Dropdown_Opts();
        SetUp_GT_POI_MSDropdown_Opts();
        gtPOI_MSDrop.OnValueChanged.AddListener(On_POI_Dropdown_ValueChanged);

        maPlots = new Mission_Analysis_Plots(this, initOrbitScript);
        initOrbitScript.panelIsFullySetUp.AddListener(OnOrbitDef_UpdateClick);

        hideShow3DOrbit_btnTxt = hideShowOrbitPreview_btn.GetComponentInChildren<TMP_Text>();

        // Adding callback when another planet is selected from the dropdown ==> update the planetary map of the mission analysis panel
        startLocPanelScript.onPlanetSelectionValueChange.AddListener(Update_Planetary_Map);

        _lrArr = new List<VectorObject2D>();
        _groundTracksData = new List<MA.XYLatLongTime>();
    }
    void SetUp_GT_Type_Dropdown_Opts()
    {
        if(gtType_Drop == null)
            Debug.LogError("Error while trying to setup the 'GroundTrackType_Dropdown': object is null");
        gtType_Drop.AddOptions(GT.gtType_Names.Select(f => f.Value).ToList());
    }
    void SetUp_GT_POI_MSDropdown_Opts()
    {
        if(gtPOI_MSDrop== null)
            Debug.LogError("Error while trying to setup the 'PointsOfInterest_MS_Dropdown': object is null");
        List<string> opts_string = GT.gtPOI_Names.Select(f => f.Value).ToList();
        List<stringBoolStruct> opts = new List<stringBoolStruct>();
        foreach(string strOpt in opts_string)
            opts.Add(new stringBoolStruct(strOpt, false));
        gtPOI_MSDrop.SetOptions(opts);
    }
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    void On_POI_Dropdown_ValueChanged()
    {
        Update_POI();
    }
    void Update_POI()
    {
        GT.GroundTrack_POI selectedPOI = (GT.GroundTrack_POI) gtPOI_MSDrop.Options_To_FlagedEnum_Int<GT.GroundTrack_POI>(GT.gtPOI_Names);
        if(selectedPOI.HasFlag(GT.GroundTrack_POI.apocentre))
            Add_POI("Apocentre", Mathf.PI);
        if(selectedPOI.HasFlag(GT.GroundTrack_POI.pericentre))
            Add_POI("Pericentre", 0f);
        if(selectedPOI.HasFlag(GT.GroundTrack_POI.ascendingNode))
            Add_POI("AscendingNode", (float)-initOrbitScript.previewedOrbit.param.omega);
        if(selectedPOI.HasFlag(GT.GroundTrack_POI.descendingNode))
            Add_POI("DescendingNode", (float)-initOrbitScript.previewedOrbit.param.omega + Mathf.PI);
    }
    void Add_POI(string goName, float trueAnomaly)
    {
        float e = (float) initOrbitScript.previewedOrbit.param.e; // no dim
        float n = (float) initOrbitScript.previewedOrbit.param.n; // rad/s
        float T = (float) initOrbitScript.previewedOrbit.param.period; // seconds
        // Corresponding Mean Anomaly of the apocentre
        float apocentre_M = (float) Kepler.OrbitalConversions.OrbitPosition.nu_2_M(trueAnomaly, e).Item1;
        apocentre_M = MathsOps.ClampAngle(apocentre_M, 0f, 2f*Mathf.PI);
        float apocentre_time = apocentre_M/n;

        // Looping through all times modulo the orbital period
        List<MA.XYLatLongTime> apocentreArr = new List<MA.XYLatLongTime>();
        for(int orbitNb=1; orbitNb<= (int)nbOrbits_slider.value; orbitNb++)
        {
            MA.XYLatLongTime closest = _groundTracksData.Select(w => new { w, distance = Math.Abs(w.time - orbitNb*apocentre_time) })
            .OrderBy(p => p.distance)
            .Select(p => p.w).First();
            apocentreArr.Add(closest);
        }
        Debug.Log("apocentreArr = " + string.Join("\n", apocentreArr));
        foreach(MA.XYLatLongTime foundData in apocentreArr)
        {
            // Spawning new Sprite
            GameObject poiGO = GameObject.Instantiate(poi_prefab, Vector3.zero, Quaternion.identity, planetMapRT);
            poiGO.SetActive(true);
            poiGO.name = goName;
            PointOfInterest poi = poiGO.GetComponent<PointOfInterest>();
            RectTransform poiGoRT = poiGO.GetComponent<RectTransform>();
            poiGoRT.offsetMin = Vector2.zero;
            poiGoRT.offsetMax = Vector2.zero;
            poiGoRT.sizeDelta = new Vector2(20f, 20f);

            poi.SetPosition(foundData.x-planetMap.rectTransform.rect.width/2f, foundData.y-planetMap.rectTransform.rect.height/2f);
        }
    }
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    void Create_GroundTrack_LineRenderer_Obj(string goName)
    {
        if(groundTrack_prefab == null)
            Debug.LogError("Error while trying to create a UILineRenderer GroundTrack Object: 'groundTrack_prefab' GameObject is null");

        // Spawning the UILineRenderer Prefab
        GameObject go = GameObject.Instantiate(groundTrack_prefab, Vector3.zero, Quaternion.identity, planetMapRT);
        go.name = goName;
        RectTransform goRT = go.GetComponent<RectTransform>();
        goRT.offsetMin = Vector2.zero;
        goRT.offsetMax = Vector2.zero;

        // Adding it to the List<UILineRender>
        VectorObject2D object2D = go.GetComponent<VectorObject2D>();
        _lrArr.Add(object2D);
        _lrArr[_lrArr.Count-1].gameObject.SetActive(false);
    }
    void Clear_GroundTracks_Plots()
    {
        // Clearing the List<UILineRenderer> containing the Ground Tracks
        _lrArr.Clear();
        _groundTracksData.Clear();

        // Clear the GroundTracks UILineRenderer except for the Grid GameObject
        for(int childIdx=0; childIdx<planetMap.transform.childCount; childIdx++) {
            if(planetMap.transform.GetChild(childIdx).name != ma_GridGO.name) {
                GameObject.DestroyImmediate(planetMap.transform.GetChild(childIdx).gameObject);
                childIdx--;
            }
        }
    }
    void Add_New_Ground_Track(string vectorLineGOName, bool bodyRotatingPlot)
    {
        if(planetMap.transform.Find(vectorLineGOName) == null)
            Create_GroundTrack_LineRenderer_Obj(vectorLineGOName);
        Compute_GroundTracks_Points(bodyRotatingPlot);
    }
    void Compute_GroundTracks_Points(bool bodyRotatingPlot)
    {
        // Plotting only 1 orbit for body-fixed plot
        int nbOrbits = bodyRotatingPlot ? (int) nbOrbits_slider.value : 1;
        List<MA.XYLatLongTime> pts;
        for(int i=0; i<nbOrbits ; i++)
        {
            // Computing the Ground Track X & Y Values
            pts = maPlots.Create_GroundTracks_Data(bodyRotatingPlot);
            if(i > 0) {
                int lastIdx = _lrArr[_lrArr.Count-1].vectorLine.points2.Count-1;
                pts = maPlots.OffsetOrbit_fromLastOrbit_Point(_groundTracksData[_groundTracksData.Count-1], pts);
            }
            _groundTracksData.AddRange(pts);
            _lrArr[_lrArr.Count-1].vectorLine.points2.AddRange(pts.ConvertAll<Vector2>(MA.XYLatLongTime.Retrieve_XY_From_Struct));
        }
        _lrArr[_lrArr.Count-1].gameObject.SetActive(true);
        _lrArr[_lrArr.Count-1].vectorLine.active = true;
        _lrArr[_lrArr.Count-1].vectorLine.color = Get_Next_GroundTrack_Color();
        _lrArr[_lrArr.Count-1].vectorLine.Draw();
    }
    Color Get_Next_GroundTrack_Color()
    {
        Color outColor = gtColors[currGtColorIdx];
        currGtColorIdx = currGtColorIdx == gtColors.Length-1 ? 0 : currGtColorIdx+1;
        return outColor;
    }
    
    void Draw_Selected_GroundTracks()
    {
        // Removing every previous ground tracks
        Clear_GroundTracks_Plots();

        string gtName;
        switch(gtType_Drop.value)
        {
            case 0:
                // Plot only Body-Fixed Ground Track
                gtName = GT.gtType_Names[GT.GroundTrackType.bodyFixed];
                Add_New_Ground_Track(gtName, false);
                break;
            case 1:
                // Plot only Body-Rotating Ground Track
                gtName = GT.gtType_Names[GT.GroundTrackType.bodyRotating];
                Add_New_Ground_Track(gtName, true);
                break;
            case 2:
                // Plot both Body-Fixed & Body-Rotating Ground Tracks
                gtName = GT.gtType_Names[GT.GroundTrackType.bodyFixed];
                Add_New_Ground_Track(gtName, false);
                gtName = GT.gtType_Names[GT.GroundTrackType.bodyRotating];
                Add_New_Ground_Track(gtName, true);
                break;
        }
        Update_POI();
    }
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    void OnOrbitDef_UpdateClick(int panelIdentifier, int isSetupBool)
    {
        if(panelIdentifier != 0 || isSetupBool != 1)
            return;
        Draw_Selected_GroundTracks();
    }
    void Check_Plot_DataLines()
    {
        if(initOrbitScript == null || maPlots == null)
            return;
        if(initOrbitScript.ORBITAL_PARAMS_VALID)
            Draw_Selected_GroundTracks();
    }
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    public void On_HideShowOrbitPreview_btnClick()
    {
        if(panelOrbitPreview_RT.gameObject.activeSelf) {
            orbt3DIsShown = false;
            panelOrbitPreview_RT.gameObject.SetActive(false);
            hideShow3DOrbit_btnTxt.text = "Show 3D orbit";
            panelPlanetMap_RT.DOLocalMove(new Vector3(130,-10.5f,0f), transitionsDur);
            panelPlanetMap_RT.DOScale(new Vector3(1.5f, 1.5f), transitionsDur);
        }
        else {
            orbt3DIsShown = true;
            hideShow3DOrbit_btnTxt.text = "Hide 3D orbit";
            panelOrbitPreview_RT.gameObject.SetActive(true);
            panelPlanetMap_RT.DOLocalMove(new Vector3(490f, -128f, 0f), transitionsDur);
            panelPlanetMap_RT.DOScale(new Vector3(1f, 1f), transitionsDur);
        }
    }
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    public void TogglePanel()
    {
        if(gameObject.activeSelf)
            OnDisable();
        else
            OnEnable();
    }
    void OnEnable()
    {
        gameObject.SetActive(true);
        initOrbitType_RT.gameObject.SetActive(false);
        sectionInitOrbit_RT.gameObject.SetActive(false);
        panelPlanetMap_RT.gameObject.SetActive(true);

        panelOrbitDef_RT.SetParent(transform);
        panelOrbitPreview_RT.SetParent(transform);
        OnEnable_Anims();
        // Has finished to init the panel, can get to work
        Check_Plot_DataLines();
    }
    void OnEnable_Anims()
    {
        panelOrbitDef_RT.DOLocalMoveX(-780f, transitionsDur, false);
        panelOrbitPreview_RT.DOScale(new Vector3(2f/3f, 2f/3f), transitionsDur);
        panelOrbitPreview_RT.DOLocalMove(new Vector3(-294f,-128f,0f), transitionsDur);
        panelPlanetMap_RT.DOLocalMove(new Vector3(490f, -128f, 0f), transitionsDur);
    }

    void OnDisable()
    {
        if(!orbt3DIsShown)
            On_HideShowOrbitPreview_btnClick();

        OnDisable_Anims();
        panelOrbitDef_RT.SetParent(sectionInitOrbit_RT);
        panelOrbitDef_RT.SetAsFirstSibling();
        
        sectionInitOrbit_RT.gameObject.SetActive(true);
        initOrbitType_RT.gameObject.SetActive(true);
        panelPlanetMap_RT.gameObject.SetActive(false);

        panelOrbitPreview_RT.SetParent(sectionInitOrbit_RT);

        transform.SetAsLastSibling();
        gameObject.SetActive(false);
    }
    void OnDisable_Anims()
    {
        panelOrbitDef_RT.DOLocalMoveX(-344f, transitionsDur, false);
        panelOrbitPreview_RT.DOScale(new Vector3(1f, 1f), transitionsDur);
        panelOrbitPreview_RT.DOLocalMove(new Vector3(300f,-85.3f,0f), transitionsDur);
    }
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    void Update_Planetary_Map(string newPlanetName)
    {
        Planets.planets currPlanet = ObjHand.Str_2_Planet(newPlanetName);
        string pathToMap = Filepaths.RSC_UIPlanetaryMaps + newPlanetName;
        Sprite newSprite = Resources.Load<Sprite>(pathToMap);
        planetMap.sprite = newSprite;
    }
}