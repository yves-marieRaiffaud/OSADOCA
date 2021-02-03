using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using Mathd_Lib;
using ObjHand = CommonMethods.ObjectsHandling;
using UI_Fncs = CommonMethods.UI_Methods;
using UniCsts = UniverseConstants;
using Planets = CelestialBodiesConstants;
using UnityEngine.Events;

using Vectrosity;
using UIExt = UnityEngine.UI.Extensions;

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
    GameObject ma_GridGO;

    public Slider nbOrbits_slider;

    void Start()
    {
        initOrbitScript = sectionInitOrbit_RT.GetComponent<UIStartLoc_InitOrbit>();
        planetMap = panelPlanetMap_RT.Find("PlanetMap").GetComponent<Image>();
        ma_GridGO = planetMap.transform.Find("Grid").gameObject;
        maPlots = new Mission_Analysis_Plots(this, initOrbitScript);
        initOrbitScript.panelIsFullySetUp.AddListener(OnOrbitDef_UpdateClick);

        hideShow3DOrbit_btnTxt = hideShowOrbitPreview_btn.GetComponentInChildren<TMP_Text>();

        // Adding callback when another planet is selected from the dropdown ==> update the planetary map of the mission analysis panel
        startLocPanelScript.onPlanetSelectionValueChange.AddListener(Update_Planetary_Map);

        _lrArr = new List<VectorObject2D>();
    }
    void Create_GroundTrack_LineRenderer_Obj(string goName)
    {
        if(groundTrack_prefab == null)
            Debug.LogError("Error while trying to create a UILineRenderer GroundTrack Object: 'groundTrack_prefab' GameObject is null");

        // Spawning the UILineRenderer Prefab
        GameObject go = GameObject.Instantiate(groundTrack_prefab, Vector3.zero, Quaternion.identity, panelPlanetMap_RT.Find("PlanetMap"));
        go.name = goName;
        RectTransform goRT = go.GetComponent<RectTransform>();
        goRT.offsetMin = Vector2.zero;
        goRT.offsetMax = Vector2.zero;

        // Adding it to the List<UILineRender>
        VectorObject2D object2D = go.GetComponent<VectorObject2D>();
        _lrArr.Add(object2D);
        _lrArr[_lrArr.Count-1].gameObject.SetActive(false);
    }

    void Update_Planetary_Map(string newPlanetName)
    {
        Planets.planets currPlanet = ObjHand.Str_2_Planet(newPlanetName);
        string pathToMap = Filepaths.RSC_UIPlanetaryMaps + newPlanetName;
        Sprite newSprite = Resources.Load<Sprite>(pathToMap);
        planetMap.sprite = newSprite;
    }

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

    void Clear_GroundTracks_Plots()
    {
        // Clearing the List<UILineRenderer> containing the Ground Tracks
        _lrArr.Clear();

        // Clear the GroundTracks UILineRenderer except for the Grid GameObject
        for(int childIdx=0; childIdx<planetMap.transform.childCount; childIdx++) {
            if(planetMap.transform.GetChild(childIdx) != ma_GridGO.transform)
                GameObject.DestroyImmediate(planetMap.transform.GetChild(childIdx).gameObject);
        }
    }

    void OnOrbitDef_UpdateClick(int panelIdentifier, int isSetupBool)
    {
        if(panelIdentifier != 0 && isSetupBool != 1)
            return;

        Clear_GroundTracks_Plots();
        // Spawning a new UILineRenderer if needed
        if(planetMap.transform.Find("RotatingBody_GT") == null)
            Create_GroundTrack_LineRenderer_Obj("RotatingBody_GT");
        Draw_RotatingBody_GroundTracks();
    }

    void Draw_RotatingBody_GroundTracks()
    {
        int nbOrbits = (int) nbOrbits_slider.value;
        for(int i=0; i<nbOrbits ; i++)
        {
            List<Vector2> pts;
            // Computing the Ground Track X & Y Values
            pts = maPlots.Create_GroundTracks_Data(true);
            if(i > 0) {
                int lastIdx = _lrArr[_lrArr.Count-1].vectorLine.points2.Count-1;
                pts = maPlots.OffsetOrbit_fromLastOrbit_Point(_lrArr[_lrArr.Count-1].vectorLine.points2[lastIdx], pts);
            }
            _lrArr[_lrArr.Count-1].vectorLine.points2.AddRange(pts);
        }
        _lrArr[_lrArr.Count-1].gameObject.SetActive(true);
        _lrArr[_lrArr.Count-1].vectorLine.active = true;
        _lrArr[_lrArr.Count-1].vectorLine.color = Color.green;
        _lrArr[_lrArr.Count-1].vectorLine.Draw();
    }

    void Check_Plot_DataLines()
    {
        if(initOrbitScript == null || maPlots == null)
            return;
        if(initOrbitScript.ORBITAL_PARAMS_VALID) {
            Clear_GroundTracks_Plots();
            // Spawning a new UILineRenderer if needed
            if(planetMap.transform.Find("RotatingBody_GT") == null)
                Create_GroundTrack_LineRenderer_Obj("RotatingBody_GT");
            Draw_RotatingBody_GroundTracks();
        }
    }


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
}