using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlBarCheck : MonoBehaviour
{
    [Header("Colors for the control bar Sprites")]
    [SerializeField] Color color_isSetUp;
    [SerializeField] Color color_isNOTSetUp;

    [SerializeField] Color color_selectedPanel;
    [SerializeField] Color color_default;

    [Header("Control Bar LINES Sprites Images")]
    public Image controlBar_startLoc_Img;
    public Image controlBar_Spacecraft_Img;
    public Image controlBar_FLY_Img;
    public Image controlBar_Matlab_Img;
    public Image controlBar_SimSettings_Img;

    [Header("Control Bar TRIANGLES Sprites Images")]
    public Image triangle_startLoc_Img;
    public Image triangle_Spacecraft_Img;
    public Image triangle_Matlab_Img;
    public Image triangle_SimSettings_Img;

    public void ChangeControlBarColor(Image spriteToChange, bool isSelected)
    {
        if(isSelected)
            spriteToChange.color = color_selectedPanel;
        else
            spriteToChange.color = color_default;
    }

    public void ControlBarMatchTriangleColor(Image spriteToChange, Image triangleToMatchColor)
    {
        spriteToChange.color = triangleToMatchColor.color;
    }

    public void ChangeTriangleColor(Image spriteToChange, bool triangleIsSetUp)
    {
        if(triangleIsSetUp)
            spriteToChange.color = color_isSetUp;
        else
            spriteToChange.color = color_isNOTSetUp;
    }
}
