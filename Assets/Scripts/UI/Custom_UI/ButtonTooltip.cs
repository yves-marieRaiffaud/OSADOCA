using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTooltip : MonoBehaviour
{
    private Camera uiCamera;
    public string buttonTooltip;
    private Text tooltipText;
    private RectTransform backgroundRT;

    private void Start()
    {
        backgroundRT = transform.Find("Background").GetComponent<RectTransform>();
        tooltipText = transform.Find("TooltipTxt").GetComponent<Text>();
        HideTooltip();
    }

    void Update()
    {
        if(gameObject.activeSelf) {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, uiCamera, out localPoint);
            transform.localPosition = localPoint;
        }
    }

    public void ShowTooltip()
    {
        gameObject.SetActive(true);

        tooltipText.text = buttonTooltip;
        Vector2 backgroundSize = new Vector2(tooltipText.preferredWidth + 4f*2f, tooltipText.preferredHeight + 4f*2f);
        backgroundRT.sizeDelta = backgroundSize;
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}