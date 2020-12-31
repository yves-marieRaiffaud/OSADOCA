using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class UIMainMenu : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    public Button btn_StartLocation;
    public Button btn_Spacecraft;
    public Button btn_ControlSelection;
    public Button btn_SimSettings;
    public Button btn_Fly;

    [Header("Control Bar Script")]
    public ControlBarCheck controlBarCheckScript;

    [Header("Panel for the 4 Main Menus")]
    [Tooltip("GameObject named 'Panel_StartLoc' located under the 'Submenus_PanelContent' GameObject")]
    public GameObject panel_StartLoc;
    UIStartLoc_Panel startLocPanelScript;

    [Tooltip("GameObject named 'Panel_Spacecraft' located under the 'Submenus_PanelContent' GameObject")]
    public GameObject panel_Spacecraft;
    UI_Spaceship_Panel spaceshipPanelScript;

    [Tooltip("GameObject named 'Panel_ControlSelection' located under the 'Submenus_PanelContent' GameObject")]
    public GameObject panel_ControlSelection;
    //UI_Matlab_Panel matlabPanelScript;

    [Tooltip("GameObject named 'Panel_SimSettings' located under the 'Submenus_PanelContent' GameObject")]
    public GameObject panel_SimSettings;
    UI_SimSettings_Panel simSettingsPanelScript;

    // Int that represent which menu is currently selected/displayed
    // 0 : Start location panel
    // 1 : Spacecraft panel
    // 2 : FLY panel
    // 3 : Control Selection panel
    // 4 : Simulation Settings panel
    [HideInInspector] public int currentSelectedMenuInt; 
    [HideInInspector] public RectTransform activeContentPanel; // Panel containing all the sub panels for each menu
    //===================    
    bool flyBtnIsAlreadyReady;

    void Start()
    {
        flyBtnIsAlreadyReady = false;

        startLocPanelScript = GetComponent<UIStartLoc_Panel>();
        startLocPanelScript.panelIsFullySetUp.AddListener(HandleControlBarTriangleColor);

        spaceshipPanelScript = GetComponent<UI_Spaceship_Panel>();
        spaceshipPanelScript.panelIsFullySetUp.AddListener(HandleControlBarTriangleColor);

        //matlabPanelScript = GetComponent<UI_Matlab_Panel>();
        //matlabPanelScript.panelIsFullySetUp.AddListener(HandleControlBarTriangleColor);

        simSettingsPanelScript = GetComponent<UI_SimSettings_Panel>();
        simSettingsPanelScript.panelIsFullySetUp.AddListener(HandleControlBarTriangleColor);

        InitMainButtons();
        StartCoroutine(FLY_CheckUpdate());
        StartCoroutine(WatchUseKeyboardParamToggle());
    }
    void InitMainButtons()
    {
        btn_StartLocation.onClick.AddListener(delegate{OnMainMenuButtonClick(0);});
        btn_Spacecraft.onClick.AddListener(delegate{OnMainMenuButtonClick(1);});
        btn_ControlSelection.onClick.AddListener(delegate{OnMainMenuButtonClick(3);});
        btn_SimSettings.onClick.AddListener(delegate{OnMainMenuButtonClick(4);});
        btn_Fly.onClick.AddListener(onFLYBtnClick);

        // On Start, by default, start on the StartLocation menu
        // Forced to first call matlab panel as it will trigger the OnEnable planetUI animation, then switching to the default starting panel
        // I don't know why this behaviour is happening (animation is triggered even when 'OnenableDisable_ControlAnimation' code is commented out)
        OnMainMenuButtonClick(3);
        OnMainMenuButtonClick(0);
    }
    //---------------------------------------------
    //---------------------------------------------
    IEnumerator WatchUseKeyboardParamToggle()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.5f);
            // Forcing every panel to send their control check before actually checking
            if(simSettingsPanelScript.useKeyboardEvent != null)
                simSettingsPanelScript.useKeyboardEvent.AddListener(ToggleUseKeyboardSimSettingPanel);
        }
    }
    void ToggleUseKeyboardSimSettingPanel(bool paramBoolValue)
    {
        int val = paramBoolValue ? 0 : 1;
        /*matlabPanelScript.shipControlModeDropdown.value = val;
        controlBarCheckScript.ControlBarMatchTriangleColor(controlBarCheckScript.controlBar_Matlab_Img, controlBarCheckScript.triangle_Matlab_Img);*/
    }
    //---------------------------------------------
    //---------------------------------------------
    void onFLYBtnClick()
    {
        currentSelectedMenuInt = 2;

        bool isReady = CheckFLYReadiness();
        if(!isReady)
            return;
        // Gathering the orbitalParams/PlanetarySurface data
        UIStartLoc_Panel startLocPanelScript = gameObject.GetComponent<UIStartLoc_Panel>();
        startLocPanelScript.On_FLY_click_GatherOrbitalParams();

        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // Loading the simulation scene
    }
    IEnumerator FLY_CheckUpdate()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.5f);
            // Forcing every panel to send their control check before actually checking
            CheckFLYReadinessForCoroutine();
        }
    }
    bool CheckFLYReadinessForCoroutine()
    {
        bool startLocOK = startLocPanelScript.SendControlBarTriangleUpdate();
        bool shipOK = spaceshipPanelScript.SendControlBarTriangleUpdate();
        //bool matlabOK = matlabPanelScript.SendControlBarTriangleUpdate();
        bool simSettingsOK = simSettingsPanelScript.SendControlBarTriangleUpdate();

        bool outputValue = false;

        if(startLocOK && shipOK && /*matlabOK &&*/ simSettingsOK && !flyBtnIsAlreadyReady) {
            controlBarCheckScript.ControlBarSet_ReadyTo_FLY_Color(true);
            flyBtnIsAlreadyReady = true;
            outputValue = true;
        }
        else if((!startLocOK || !shipOK || /*!matlabOK ||*/ !simSettingsOK) && flyBtnIsAlreadyReady) {
            controlBarCheckScript.ControlBarSet_ReadyTo_FLY_Color(false);
            flyBtnIsAlreadyReady = false;
            outputValue = false;
        }
        return outputValue;
    }
    bool CheckFLYReadiness()
    {
        bool startLocOK = startLocPanelScript.SendControlBarTriangleUpdate();
        bool shipOK = spaceshipPanelScript.SendControlBarTriangleUpdate();
        //bool matlabOK = matlabPanelScript.SendControlBarTriangleUpdate();
        bool simSettingsOK = simSettingsPanelScript.SendControlBarTriangleUpdate();

        bool outputValue = false;

        if(startLocOK && shipOK && /*matlabOK &&*/ simSettingsOK)
            outputValue = true;
        else if(!startLocOK || !shipOK && /*|| !matlabOK ||*/ !simSettingsOK) {
            controlBarCheckScript.ControlBarSet_ReadyTo_FLY_Color(false);
            outputValue = false;
        }
        return outputValue;
    }
    //---------------------------------------------
    //---------------------------------------------
    void OnMainMenuButtonClick(int idxClickedBtn)
    {
        currentSelectedMenuInt = idxClickedBtn;
        HandlePanelsToggling(currentSelectedMenuInt);
        HandleControlBarLineColor(currentSelectedMenuInt);
    }
    void HandlePanelsToggling(int selectedMenuItem)
    {
        switch(selectedMenuItem)
        {
            case 0:
                // "StartLocation" panel selected
                panel_StartLoc.SetActive(true);
                panel_SimSettings.SetActive(false);
                panel_ControlSelection.SetActive(false);
                panel_Spacecraft.SetActive(false);
                break;
            case 1:
                // "Spacecraft" panel selected
                panel_StartLoc.SetActive(false);
                panel_SimSettings.SetActive(false);
                panel_ControlSelection.SetActive(false);
                panel_Spacecraft.SetActive(true);
                break;
            case 3:
                // "Control Selection" panel selected
                panel_StartLoc.SetActive(false);
                panel_SimSettings.SetActive(false);
                panel_ControlSelection.SetActive(true);
                panel_Spacecraft.SetActive(false);
                break;
            case 4:
                // "SimSettings" panel selected
                panel_StartLoc.SetActive(false);
                panel_SimSettings.SetActive(true);
                panel_ControlSelection.SetActive(false);
                panel_Spacecraft.SetActive(false);
                break;
        }
    }
    void HandleControlBarTriangleColor(int mainPanelIdentifier, int boolPanelIsSetUp)
    {
        bool panelIsSetUpBool = boolPanelIsSetUp == 1 ? true:false;
        switch(mainPanelIdentifier)
        {
            case 0:
                // 'startLocation' panel has fired its event
                controlBarCheckScript.ChangeTriangleColor(controlBarCheckScript.triangle_startLoc_Img, panelIsSetUpBool);
                break;
            case 1:
                // 'spacecraft' panel has fired its event
                controlBarCheckScript.ChangeTriangleColor(controlBarCheckScript.triangle_Spacecraft_Img, panelIsSetUpBool);
                break;
            case 2:
                // 'matlab' panel has fired its event
                controlBarCheckScript.ChangeTriangleColor(controlBarCheckScript.triangle_Matlab_Img, panelIsSetUpBool);
                break;
            case 3:
                // 'SimSettings' panel has fired its event
                controlBarCheckScript.ChangeTriangleColor(controlBarCheckScript.triangle_SimSettings_Img, panelIsSetUpBool);
                break;
        }
    }
    void HandleControlBarLineColor(int selectedMenuItem)
    {
        switch(selectedMenuItem)
        {
            case 0:
                // "StartLocation" panel selected
                controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_startLoc_Img, true);
                controlBarCheckScript.ControlBarMatchTriangleColor(controlBarCheckScript.controlBar_Spacecraft_Img, controlBarCheckScript.triangle_Spacecraft_Img);
                controlBarCheckScript.ControlBarMatchTriangleColor(controlBarCheckScript.controlBar_Matlab_Img, controlBarCheckScript.triangle_Matlab_Img);
                controlBarCheckScript.ControlBarMatchTriangleColor(controlBarCheckScript.controlBar_SimSettings_Img, controlBarCheckScript.triangle_SimSettings_Img);
                break;
            case 1:
                // "Spacecraft" panel selected
                controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_Spacecraft_Img, true);
                controlBarCheckScript.ControlBarMatchTriangleColor(controlBarCheckScript.controlBar_startLoc_Img, controlBarCheckScript.triangle_startLoc_Img);
                controlBarCheckScript.ControlBarMatchTriangleColor(controlBarCheckScript.controlBar_Matlab_Img, controlBarCheckScript.triangle_Matlab_Img);
                controlBarCheckScript.ControlBarMatchTriangleColor(controlBarCheckScript.controlBar_SimSettings_Img, controlBarCheckScript.triangle_SimSettings_Img);
                break;
            case 3:
                // "Matlab" panel selected
                controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_Matlab_Img, true);
                controlBarCheckScript.ControlBarMatchTriangleColor(controlBarCheckScript.controlBar_startLoc_Img, controlBarCheckScript.triangle_startLoc_Img);
                controlBarCheckScript.ControlBarMatchTriangleColor(controlBarCheckScript.controlBar_Spacecraft_Img, controlBarCheckScript.triangle_Spacecraft_Img);
                controlBarCheckScript.ControlBarMatchTriangleColor(controlBarCheckScript.controlBar_SimSettings_Img, controlBarCheckScript.triangle_SimSettings_Img);
                break;
            case 4:
                // "SimSettings" panel selected
                controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_SimSettings_Img, true);
                controlBarCheckScript.ControlBarMatchTriangleColor(controlBarCheckScript.controlBar_startLoc_Img, controlBarCheckScript.triangle_startLoc_Img);
                controlBarCheckScript.ControlBarMatchTriangleColor(controlBarCheckScript.controlBar_Spacecraft_Img, controlBarCheckScript.triangle_Spacecraft_Img);
                controlBarCheckScript.ControlBarMatchTriangleColor(controlBarCheckScript.controlBar_Matlab_Img, controlBarCheckScript.triangle_Matlab_Img);
                simSettingsPanelScript.UpdateParameters();
                break;
        }
    }
}