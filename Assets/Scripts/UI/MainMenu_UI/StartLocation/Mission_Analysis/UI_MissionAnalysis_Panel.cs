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
    internal List<UIExt.UILineRenderer> _lrArr;

    public Slider nbOrbits_slider;

    void Start()
    {
        initOrbitScript = sectionInitOrbit_RT.GetComponent<UIStartLoc_InitOrbit>();
        planetMap = panelPlanetMap_RT.Find("PlanetMap").GetComponent<Image>();
        maPlots = new Mission_Analysis_Plots(this, initOrbitScript);
        initOrbitScript.panelIsFullySetUp.AddListener(OnOrbitDef_UpdateClick);

        hideShow3DOrbit_btnTxt = hideShowOrbitPreview_btn.GetComponentInChildren<TMP_Text>();

        // Adding callback when another planet is selected from the dropdown ==> update the planetary map of the mission analysis panel
        startLocPanelScript.onPlanetSelectionValueChange.AddListener(Update_Planetary_Map);

        _lrArr = new List<UIExt.UILineRenderer>();

        /*Create_GroundTrack_LineRenderer_Obj("DebugLine");
        _lrArr[_lrArr.Count-1].gameObject.SetActive(true);
        List<Vector2> debugPts = new List<Vector2>();
        debugPts.Add(new Vector2(13.3f,0f));
        debugPts.Add(new Vector2(planetMap.rectTransform.rect.width,0f));
        debugPts.Add(new Vector2(planetMap.rectTransform.rect.width,planetMap.rectTransform.rect.height));
        debugPts.Add(new Vector2(0f,planetMap.rectTransform.rect.height));
        debugPts.Add(new Vector2(0f,0f));
        _lrArr[_lrArr.Count-1].Points = debugPts.ToArray();*/

    }
    void Create_GroundTrack_LineRenderer_Obj(string goName)
    {
        if(groundTrack_prefab == null)
            Debug.LogError("Error while trying to create a UILineRenderer GroundTrack Object: 'groundTrack_prefab' GameObject is null");
        GameObject go = GameObject.Instantiate(groundTrack_prefab, Vector3.zero, Quaternion.identity, panelPlanetMap_RT.Find("PlanetMap"));
        go.name = goName;
        RectTransform goRT = go.GetComponent<RectTransform>();
        goRT.offsetMin = Vector2.zero;
        goRT.offsetMax = Vector2.zero;
        _lrArr.Add(go.GetComponent<UIExt.UILineRenderer>());
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

    void OnOrbitDef_UpdateClick(int panelIdentifier, int isSetupBool)
    {
        if(panelIdentifier != 0 && isSetupBool != 1)
            return;
        
        _lrArr.Clear();
        for(int childIdx=0; childIdx<panelPlanetMap_RT.Find("PlanetMap").childCount; childIdx++)
            GameObject.Destroy(panelPlanetMap_RT.Find("PlanetMap").GetChild(childIdx).gameObject);

        for(int i=0; i<(int)nbOrbits_slider.value; i++)
        {
            Create_GroundTrack_LineRenderer_Obj(i.ToString());
            Vector2[] pts;
            pts = maPlots.Create_GroundTracks_Data(true);
            _lrArr[_lrArr.Count-1].gameObject.SetActive(true);
            if(i > 0) {
                int lastIdx = _lrArr[_lrArr.Count-2].Points.Length-1;
                pts = maPlots.OffsetOrbit_fromLastOrbit_Point(_lrArr[_lrArr.Count-2].Points[lastIdx], pts);
            }
            _lrArr[_lrArr.Count-1].Points = pts;
        }
    }
    void Check_Plot_DataLines()
    {
        if(initOrbitScript == null || maPlots == null)
            return;
        if(initOrbitScript.ORBITAL_PARAMS_VALID)
            maPlots.Create_GroundTracks_Data(true);
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