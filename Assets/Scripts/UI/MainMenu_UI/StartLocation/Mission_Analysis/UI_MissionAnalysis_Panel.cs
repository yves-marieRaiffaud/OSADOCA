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
    Image planetMap;

    [Tooltip("Button of the 'HideShow_OritPreview_btn'")]
    public Button hideShowOrbitPreview_btn;
    TMP_Text hideShow3DOrbit_btnTxt;
    bool orbt3DIsShown=true;

    float transitionsDur = 0.5f;
    UIStartLoc_InitOrbit initOrbitScript;
    Mission_Analysis_Plots maPlots;

    internal UIExt.UILineRenderer _lr;

    void Start()
    {
        initOrbitScript = sectionInitOrbit_RT.GetComponent<UIStartLoc_InitOrbit>();
        maPlots = new Mission_Analysis_Plots(this, initOrbitScript);
        initOrbitScript.panelIsFullySetUp.AddListener(OnOrbitDef_UpdateClick);

        hideShow3DOrbit_btnTxt = hideShowOrbitPreview_btn.GetComponentInChildren<TMP_Text>();

        planetMap = panelPlanetMap_RT.Find("PlanetMap").GetComponent<Image>();
        // Adding callback when another planet is selected from the dropdown ==> update the planetary map of the mission analysis panel
        startLocPanelScript.onPlanetSelectionValueChange.AddListener(Update_Planetary_Map);

        if(_lr == null) {
            GameObject go = ObjHand.CreateAssignGameObject("GroundTrack", panelPlanetMap_RT.Find("PlanetMap").gameObject);
            _lr = (UIExt.UILineRenderer) ObjHand.CreateAssignComponent(typeof(UIExt.UILineRenderer), go);
            _lr.gameObject.SetActive(false);
        }
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
        maPlots.Create_GroundTracks_Data();
    }
    void Check_Plot_DataLines()
    {
        if(initOrbitScript == null || maPlots == null)
            return;
        if(initOrbitScript.ORBITAL_PARAMS_VALID)
            maPlots.Create_GroundTracks_Data();
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