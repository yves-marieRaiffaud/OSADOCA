using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIMainMenu : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    public Button btn_StartLocation;
    public Button btn_Spacecraft;
    public Button btn_Matlab;
    public Button btn_SimSettings;
    public Button btn_Fly;

    [Header("Control Bar Script")]
    public ControlBarCheck controlBarCheckScript;

    // Int that represent which menu is currently selected/displayed
    // 0 : Start location panel
    // 1 : Spacecraft panel
    // 2 : FLY panel
    // 3 : Matlab panel
    // 4 : Simulation Settings panel
    [HideInInspector] public int currentSelectedMenuInt; 
    [HideInInspector] public RectTransform activeContentPanel; // Panel containing all the sub panels for each menu
    //===================

    void Start()
    {
        InitMainButtons();
    }

    private void InitMainButtons()
    {
        btn_StartLocation.onClick.AddListener(onStartLocationBtnClick);
        btn_Spacecraft.onClick.AddListener(onSpacecraftBtnClick);
        btn_Matlab.onClick.AddListener(onMatlabBtnClick);
        btn_SimSettings.onClick.AddListener(onSimSettingsBtnClick);
        btn_Fly.onClick.AddListener(onFLYBtnClick);

        // On Start, by default, start on the StartLocation menu
        onStartLocationBtnClick();
    }

    private void onStartLocationBtnClick()
    {
        currentSelectedMenuInt = 0;
        
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_startLoc_Img, controlBarCheckScript.color_selectedPanel);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_Spacecraft_Img, controlBarCheckScript.color_isSetUp);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_FLY_Img, controlBarCheckScript.color_isNOTSetUp);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_Matlab_Img, controlBarCheckScript.color_isSetUp);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_SimSettings_Img, controlBarCheckScript.color_isNOTSetUp);

        Debug.Log("Click on start location btn");
    }

    private void onSpacecraftBtnClick()
    {
        currentSelectedMenuInt = 1;

        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_startLoc_Img, controlBarCheckScript.color_default);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_Spacecraft_Img, controlBarCheckScript.color_selectedPanel);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_FLY_Img, controlBarCheckScript.color_default);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_Matlab_Img, controlBarCheckScript.color_default);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_SimSettings_Img, controlBarCheckScript.color_default);

        Debug.Log("Click on spacecraft btn");
    }

    private void onMatlabBtnClick()
    {
        currentSelectedMenuInt = 3;

        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_startLoc_Img, controlBarCheckScript.color_default);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_Spacecraft_Img, controlBarCheckScript.color_default);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_FLY_Img, controlBarCheckScript.color_default);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_Matlab_Img, controlBarCheckScript.color_selectedPanel);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_SimSettings_Img, controlBarCheckScript.color_default);

        Debug.Log("Click on Matlab btn");
    }

    private void onSimSettingsBtnClick()
    {
        currentSelectedMenuInt = 4;

        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_startLoc_Img, controlBarCheckScript.color_default);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_Spacecraft_Img, controlBarCheckScript.color_default);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_FLY_Img, controlBarCheckScript.color_default);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_Matlab_Img, controlBarCheckScript.color_default);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_SimSettings_Img, controlBarCheckScript.color_selectedPanel);

        Debug.Log("Click on sim settings btn");
    }

    private void onFLYBtnClick()
    {
        currentSelectedMenuInt = 2;

        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_startLoc_Img, controlBarCheckScript.color_default);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_Spacecraft_Img, controlBarCheckScript.color_default);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_FLY_Img, controlBarCheckScript.color_selectedPanel);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_Matlab_Img, controlBarCheckScript.color_default);
        controlBarCheckScript.ChangeControlBarColor(controlBarCheckScript.controlBar_SimSettings_Img, controlBarCheckScript.color_default);

        Debug.Log("Click on FLY btn");

        DebugGameObject scriptInst = GameObject.Find("DEBUG").GetComponent<DebugGameObject>();
        if(scriptInst.loadShipDataFromUIDiskFile)
        {
            // Gathering the orbitalParams/PlanetarySurface data
            UIStartLoc_Panel startLocPanelScript = gameObject.GetComponent<UIStartLoc_Panel>();
            startLocPanelScript.On_FLY_click_GatherOrbitalParams();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // Loading the simulation scene
    }

    void OnGUI()
    {

    }
}