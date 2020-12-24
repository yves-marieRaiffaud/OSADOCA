using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomShaped_Buttons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ButtonTooltip tooltip;

    void Start()
    {
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }

    public void OnPointerEnter(PointerEventData data)
    {
        if(tooltip != null)
            tooltip.ShowTooltip();
        else
            Debug.Log("tooltip is null");
    }

    public void OnPointerExit(PointerEventData data)
    {
        if(tooltip != null)
            tooltip.HideTooltip();
        else
            Debug.Log("tooltip is null");
    }
}