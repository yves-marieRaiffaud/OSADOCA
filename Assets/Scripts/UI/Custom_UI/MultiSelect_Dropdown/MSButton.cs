using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace MSDropdownNamespace
{
    public class MSButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
    {
        public Image targetGraphic;
        //========
        public Color normalColor;
        public Color pressedColor;
        public Color highlightedColor;
        public Color selectedColor; 
        public Color disabledColor;
        //========
        UnityEvent _OnClick;
        public UnityEvent OnClick
        {
            get {
                return _OnClick;
            }
        }
        UnityEvent _OnEnter;
        public UnityEvent OnEnter
        {
            get {
                return _OnEnter;
            }
        }
        UnityEvent _OnExit;
        public UnityEvent OnExit
        {
            get {
                return _OnExit;
            }
        }
        UnityEvent _OnPressed;
        public UnityEvent OnPressed
        {
            get {
                return _OnPressed;
            }
        }
        UnityEvent _OnReleased;
        public UnityEvent OnReleased
        {
            get {
                return _OnReleased;
            }
        }
        //========
        public bool isEnabled { get; set; }

        public bool isSelected { get; set; }
        private Color currentColor;
        //========
        public void UpdateButton(bool newIsSelected)
        {
            isSelected = newIsSelected;
            if(isSelected && targetGraphic != null)
                targetGraphic.color = currentColor = selectedColor;
            else if(!isSelected && targetGraphic != null)
                targetGraphic.color = currentColor = normalColor;
            if(isEnabled && targetGraphic != null)
                targetGraphic.color = currentColor = disabledColor;
            CheckTargetGraphic();
        }

        internal void Awake()
        {
            if(_OnClick == null)
                _OnClick = new UnityEvent();
            if(_OnEnter == null)
                _OnEnter = new UnityEvent();
            if(_OnExit == null)
                _OnExit = new UnityEvent();
            if(_OnPressed == null)
                _OnPressed = new UnityEvent();
            if(_OnReleased == null)
                _OnReleased = new UnityEvent();
        }
        public void Enable()
        {
            isEnabled = true;
            if(targetGraphic != null)
                targetGraphic.color = currentColor;
            CheckTargetGraphic();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(_OnClick != null)
                _OnClick.Invoke();

            isSelected = !isSelected;
            if(targetGraphic != null && isSelected)
                targetGraphic.color = currentColor = selectedColor;
            else if(targetGraphic != null && !isSelected)
                targetGraphic.color = currentColor = normalColor;
            CheckTargetGraphic();
        }

        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            if(_OnEnter != null)
                _OnEnter.Invoke();
            if(targetGraphic != null && !isSelected)
                targetGraphic.color = currentColor = highlightedColor;
            CheckTargetGraphic();
        }

        public void OnPointerExit(PointerEventData pointerEventData)
        {
            if(_OnExit != null)
                _OnExit.Invoke();
            
            if(targetGraphic != null && !isSelected)
                targetGraphic.color = currentColor = normalColor;
            CheckTargetGraphic();
        }

        public void OnPointerUp(PointerEventData pointerEventData)
        {
            if(_OnPressed != null)
                _OnPressed.Invoke();

            if(targetGraphic != null && !isSelected)
                targetGraphic.color = currentColor = highlightedColor;
            CheckTargetGraphic();
        }

        public void OnPointerDown(PointerEventData pointerEventData)
        {
            if(_OnReleased != null)
                _OnReleased.Invoke();
            
            if(targetGraphic != null && !isSelected)
                targetGraphic.color = currentColor = pressedColor;
            CheckTargetGraphic();
        }
    
        private void CheckTargetGraphic()
        {
            if(targetGraphic==null)
                Debug.LogWarning("'TargetGraphic' of MSButton is null. You may want to assign one");
        }
    }
}