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
    }

    private void onStartLocationBtnClick()
    {
        Debug.Log("Click on start location btn");
    }

    private void onSpacecraftBtnClick()
    {
        Debug.Log("Click on spacecraft btn");
    }

    private void onMatlabBtnClick()
    {
        Debug.Log("Click on Matlab btn");
    }

    private void onSimSettingsBtnClick()
    {
        Debug.Log("Click on sim settings btn");
    }

    private void onFLYBtnClick()
    {
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