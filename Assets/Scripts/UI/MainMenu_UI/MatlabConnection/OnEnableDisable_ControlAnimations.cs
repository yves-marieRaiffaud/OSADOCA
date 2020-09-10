using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Playables;
using System;

public class OnEnableDisable_ControlAnimations : MonoBehaviour
{
    public Animator cameraAnimator;
    public PlayableGraph playableGraph;
    public AnimationClip OnEnableAnim;
    public AnimationClip OnDisableAnim;

    

    void OnEnable()
    {
        AnimationPlayableUtilities.PlayClip(cameraAnimator, OnEnableAnim, out playableGraph);
    }

    void OnDisable()
    {   
        AnimationPlayableUtilities.PlayClip(cameraAnimator, OnDisableAnim, out playableGraph);
    }
}