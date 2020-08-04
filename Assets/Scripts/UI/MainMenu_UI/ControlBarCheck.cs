using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlBarCheck : MonoBehaviour
{
    [Header("Colors for the control bar Sprites")]
    public Color color_isSetUp;
    public Color color_isNOTSetUp;
    public Color color_selectedPanel;
    public Color color_default;

    [Header("Control Bar Sprites Images")]
    public Image controlBar_startLoc_Img;
    public Image controlBar_Spacecraft_Img;
    public Image controlBar_FLY_Img;
    public Image controlBar_Matlab_Img;
    public Image controlBar_SimSettings_Img;

    public void ChangeControlBarColor(Image spriteToChange, Color newColor)
    {
        spriteToChange.color = newColor;
    }
}
