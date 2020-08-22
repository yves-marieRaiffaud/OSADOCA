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
    UI_Spaceship_Builder shipBuilder;
    //==================
    //==================
    void OnEnable()
    {
        shipBuilder = panelSpacecraft.GetComponent<UI_Spaceship_Builder>();
    }
}