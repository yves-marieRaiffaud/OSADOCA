using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mathd_Lib;

public class UI_Spaceship_Panel : MonoBehaviour
{
    [Tooltip("GameObejct named 'Panel_Spacecraft' under the 'Panel_Submenus' gameobject")]
    public GameObject panelSpacecraft;
    //=====================================
    //=====================================
    //UI_Spaceship_Builder shipBuilder;

    [HideInInspector] public MainPanelIsSetUp panelIsFullySetUp;
    //==================
    //==================
    void OnEnable()
    {
        //shipBuilder = panelSpacecraft.GetComponent<UI_Spaceship_Builder>();
    }

    void Start()
    {
        if(panelIsFullySetUp == null)
            panelIsFullySetUp = new MainPanelIsSetUp();
        
        // As the panel is not implemented, sending 1 as bool value (arg2)
        if(panelIsFullySetUp != null)
            panelIsFullySetUp.Invoke(1, 1);
    }

    public bool SendControlBarTriangleUpdate()
    {
        if(panelIsFullySetUp != null)
            panelIsFullySetUp.Invoke(1, 1);
        return true;
    }
}