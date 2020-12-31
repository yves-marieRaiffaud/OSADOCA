using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class ControlBarCheck : MonoBehaviour
{
    [Header("Colors for the control bar Sprites")]
    [SerializeField] Color color_isSetUp;
    [SerializeField] Color color_isNOTSetUp;

    [SerializeField] Color color_selectedPanel;
    [SerializeField] Color color_default;

    [SerializeField] Color color_OKtoFLY;

    [Header("Control Bar LINES Sprites Images")]
    public Image controlBar_startLoc_Img;
    public Image controlBar_Spacecraft_Img;
    public Image controlBar_Matlab_Img;
    public Image controlBar_SimSettings_Img;

    [Header("Control Bar TRIANGLES Sprites Images")]
    public Image triangle_startLoc_Img;
    public Image triangle_Spacecraft_Img;
    public Image triangle_Matlab_Img;
    public Image triangle_SimSettings_Img;

    [Header("Fly Text & Control bar image")]
    public Image controlBar_FLY_Img;
    public TMPro.TMP_Text fly_text;
    public Animator flyTxtAnimator;
    public AnimationClip flyTxtAnimation;
    PlayableGraph flyTxtGraph;

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

    public void ControlBarSet_ReadyTo_FLY_Color(bool isReadyToFly)
    {
        if(isReadyToFly) {
            
            fly_text.color = controlBar_FLY_Img.color = color_OKtoFLY;
            AnimationPlayableUtilities.PlayClip(flyTxtAnimator, flyTxtAnimation, out flyTxtGraph);
        }
        else
            fly_text.color = controlBar_FLY_Img.color = color_default;
    }

    public void ChangeTriangleColor(Image spriteToChange, bool triangleIsSetUp)
    {
        if(triangleIsSetUp)
            spriteToChange.color = color_isSetUp;
        else
            spriteToChange.color = color_isNOTSetUp;
    }
}