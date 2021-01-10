using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace MSDropdownNamespace
{
    public class CustomShaped_Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
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

        void Start()
        {
            if(_OnEnter == null)
                _OnEnter = new UnityEvent();
            if(_OnExit == null)
                _OnExit = new UnityEvent();
            this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        }

        public void OnPointerEnter(PointerEventData data)
        {
            if(_OnEnter != null)
                _OnEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData data)
        { 
            if(_OnExit != null)
                _OnExit.Invoke();
        }
    }
}