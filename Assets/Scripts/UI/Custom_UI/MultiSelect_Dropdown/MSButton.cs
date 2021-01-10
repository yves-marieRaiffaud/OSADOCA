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
        public UnityEvent OnClick {get; set;}
        public UnityEvent OnEnter {get; set;}
        public UnityEvent OnExit {get; set;}
        public UnityEvent OnPressed {get; set;}
        public UnityEvent OnReleased {get; set;}
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

        void Awake()
        {
            if(OnClick == null)
                OnClick = new UnityEvent();
            if(OnEnter == null)
                OnEnter = new UnityEvent();
            if(OnExit == null)
                OnExit = new UnityEvent();
            if(OnPressed == null)
                OnPressed = new UnityEvent();
            if(OnReleased == null)
                OnReleased = new UnityEvent();
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
            if(OnClick != null)
                OnClick.Invoke();

            isSelected = !isSelected;
            if(targetGraphic != null && isSelected)
                targetGraphic.color = currentColor = selectedColor;
            else if(targetGraphic != null && !isSelected)
                targetGraphic.color = currentColor = normalColor;
            CheckTargetGraphic();
        }

        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            if(OnEnter != null)
                OnEnter.Invoke();
            if(targetGraphic != null && !isSelected)
                targetGraphic.color = currentColor = highlightedColor;
            CheckTargetGraphic();
        }

        public void OnPointerExit(PointerEventData pointerEventData)
        {
            if(OnExit != null)
                OnExit.Invoke();
            
            if(targetGraphic != null && !isSelected)
                targetGraphic.color = currentColor = normalColor;
            CheckTargetGraphic();
        }

        public void OnPointerUp(PointerEventData pointerEventData)
        {
            if(OnPressed != null)
                OnPressed.Invoke();

            if(targetGraphic != null && !isSelected)
                targetGraphic.color = currentColor = highlightedColor;
            CheckTargetGraphic();
        }

        public void OnPointerDown(PointerEventData pointerEventData)
        {
            if(OnReleased != null)
                OnReleased.Invoke();
            
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