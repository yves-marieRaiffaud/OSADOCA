using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Events;

public class ToggleSwitch : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private bool _isOn = false;
    public bool isOn
    {
        get {
            return _isOn;
        }
        set {
            _isOn=value;
            Toggle(!_isOn);
            Toggle(!_isOn);
        }
    }

    [SerializeField]
    private RectTransform toggleIndicator;
    [SerializeField]
    private Image backgroundImage;

    [SerializeField]
    private Color onColor;
    [SerializeField]
    private Color offColor;
    private float offX;
    private float onX;
    [SerializeField]
    private float tweenTime = 0.25f;

    public UnityEvent onValueChanged;

    private void OnEnable() {
        Toggle(isOn);    
    }

    void Awake()
    {
        if(onValueChanged == null)
            onValueChanged = new UnityEvent();

        offX = toggleIndicator.anchoredPosition.x;
        onX = offX + 80f;
    }

    public void Toggle(bool value)
    {
        if(value != isOn)
        {
            _isOn = value;

            ToggleColor(isOn);
            MoveIndicator(isOn);

            if(onValueChanged != null)
                onValueChanged.Invoke();
        }
    }

    private void ToggleColor(bool value)
    {
        if(value)
            backgroundImage.DOColor(onColor, tweenTime);
        else
            backgroundImage.DOColor(offColor, tweenTime);
    }

    private void MoveIndicator(bool value)
    {
        if(value)
            toggleIndicator.DOAnchorPosX(onX, tweenTime);
        else
            toggleIndicator.DOAnchorPosX(offX, tweenTime);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Toggle(!isOn);
    }
}
