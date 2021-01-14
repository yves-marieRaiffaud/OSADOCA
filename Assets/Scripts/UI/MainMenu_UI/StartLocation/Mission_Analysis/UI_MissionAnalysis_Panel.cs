using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Mathd_Lib;
using ObjHand = CommonMethods.ObjectsHandling;
using UI_Fncs = CommonMethods.UI_Methods;
using UniCsts = UniverseConstants;
using UnityEngine.Events;

public class UI_MissionAnalysis_Panel : MonoBehaviour
{
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

    float transitionsDur = 0.5f;

    public void TogglePanel()
    {
        if(gameObject.activeSelf)
            OnDisable();
        else
            OnEnable();
    }

    void OnEnable_Anims()
    {
        panelOrbitDef_RT.DOLocalMoveX(-705f, transitionsDur, false);
        panelOrbitPreview_RT.DOScale(new Vector3(0.5f, 0.5f), transitionsDur);
        panelOrbitPreview_RT.DOLocalMove(new Vector3(-294f,-185.4f,0f), transitionsDur);
    }
    void OnDisable_Anims()
    {
        panelOrbitDef_RT.DOLocalMoveX(-344f, transitionsDur, false);
        panelOrbitPreview_RT.DOScale(new Vector3(1f, 1f), transitionsDur);
        panelOrbitPreview_RT.DOLocalMove(new Vector3(300f,-85.3f,0f), transitionsDur);
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
    }
    void OnDisable()
    {
        OnDisable_Anims();

        sectionInitOrbit_RT.gameObject.SetActive(true);
        initOrbitType_RT.gameObject.SetActive(true);
        panelPlanetMap_RT.gameObject.SetActive(false);

        panelOrbitDef_RT.SetParent(sectionInitOrbit_RT);
        panelOrbitDef_RT.SetAsFirstSibling();

        panelOrbitPreview_RT.SetParent(sectionInitOrbit_RT);

        transform.SetAsLastSibling();
        gameObject.SetActive(false);
    }
}