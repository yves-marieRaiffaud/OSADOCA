using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    public Button btn_StartLocation;
    public Button btn_Spacecraft;
    public Button btn_Matlab;
    public Button btn_SimSettings;

    public RectTransform activeContentPanel; // Panel containing all the sub panels for each menu

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
    }

    private void onStartLocationBtnClick()
    {
        Debug.Log("Click on start location btn");
        // Create the dropdown UI item to select the type of location start
        //RectTransform typeOfLocationStartPanel = UsefulFunctions.CreateAssignUIPanel<RectTransform>("typeOfLocationStartPanel", contentPanel.gameObject,
        //                                        UsefulFunctions.UIPanelReturnType.RectTransform);
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

    void OnGUI()
    {

    }
}