using UnityEngine;
using UnityEngine.Playables;

public class OnEnableDisable_ControlAnimations : MonoBehaviour
{
    public Animator cameraAnimator;
    PlayableGraph playableGraph;
    public AnimationClip OnEnableAnim;
    public AnimationClip OnDisableAnim;

    void OnEnable()
    {
        if(cameraAnimator != null && OnEnableAnim != null)
            AnimationPlayableUtilities.PlayClip(cameraAnimator, OnEnableAnim, out playableGraph);
    }

    void OnDisable()
    {   
        if(cameraAnimator != null && OnDisableAnim != null)
            AnimationPlayableUtilities.PlayClip(cameraAnimator, OnDisableAnim, out playableGraph);
    }
}