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
        AnimationPlayableUtilities.PlayClip(cameraAnimator, OnEnableAnim, out playableGraph);
    }

    void OnDisable()
    {   
        AnimationPlayableUtilities.PlayClip(cameraAnimator, OnDisableAnim, out playableGraph);
    }
}