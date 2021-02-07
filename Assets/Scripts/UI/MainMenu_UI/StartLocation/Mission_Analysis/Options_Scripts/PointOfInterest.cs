using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;

public class PointOfInterest : MonoBehaviour
{
    RectTransform _rt;
    Button _btn;
    Image _spriteImg;

    [SerializeField] Color _spriteColor=Color.red;
    public Color spriteColor
    {
        get {
            return _spriteColor;
        }
        set {
            _spriteColor=value;
            SetColor(_spriteColor);
        }
    }

    [SerializeField] bool _isActive=false;
    public bool isActive
    {
        get {
            return _isActive;
        }
        set {
            _isActive=value;
            SetActiveState(_isActive);
        }
    }

    [SerializeField] UnityEvent _OnClick;
    public UnityEvent OnClick
    {
        get {
            return _OnClick;
        }
        set {
            _OnClick=value;
        }
    }
    [SerializeField] UnityEvent _OnEnter;
    public UnityEvent OnEnter
    {
        get {
            return _OnEnter;
        }
        set {
            _OnEnter=value;
        }
    }
    [SerializeField] UnityEvent _OnExit;
    public UnityEvent OnExit
    {
        get {
            return _OnExit;
        }
        set {
            _OnExit=value;
        }
    }

    void Awake()
    {
        if(!TryGetComponent<RectTransform>(out _rt))
            Debug.Log("Error while awaking 'PointOfInterest': can't find its RectTransform component.");
        if(!TryGetComponent<Button>(out _btn))
            Debug.Log("Error while awaking 'PointOfInterest': can't find its Button component.");
        if(!TryGetComponent<Image>(out _spriteImg))
            Debug.Log("Error while awaking 'PointOfInterest': can't find its Image component.");

        if(_OnClick == null)
            _OnClick = new UnityEvent();
        if(_OnEnter == null)
            _OnEnter = new UnityEvent();
        if(_OnExit == null)
            _OnExit = new UnityEvent();

        _btn.onClick.AddListener(OnPOIClick);
        // Apply properties
        SetActiveState(_isActive);
        SetColor(_spriteColor);
    }

    void SetColor(Color newSpriteColor)
    {
        _spriteImg.color = newSpriteColor;
    }

    void OnPOIClick()
    {
        if(_OnClick != null)
            _OnClick.Invoke();
    }

    public void SetPosition(float posX, float posY)
    {
        _rt.localPosition = new Vector3(posX, posY, 0f);
    }

    public void SetActiveState(bool setToActive)
    {
        gameObject.SetActive(setToActive);
    }
    public void ToggleState()
    {
        SetActiveState(_isActive = !_isActive);
    }
}