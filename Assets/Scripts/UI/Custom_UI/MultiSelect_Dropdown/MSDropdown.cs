using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace MSDropdownNamespace
{
    public class MSDropdown : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Color normalColor;
        public Color pressedColor;
        public Color highlightedColor;
        public Color selectedColor;
        public Color disabledColor;
        //=====
        public bool addEverythingOption;
        public bool addNoneOption;
        //=====
        public List<stringBoolStruct> _options;
        public List<stringBoolStruct> options
        {
            get {
                return _options;
            }
            set {
                _options=value;
            }
        }
        //==========
        GameObject templateGO;
        GameObject contentGO;
        GameObject dropdownListGO;
        GameObject itemGO;
        bool dropdownListIsActive;
        //=========
        public UnityEvent OnValueChanged;
        public UnityEvent OnClick;
        public UnityEvent OnEnter;
        public UnityEvent OnExit;
        //=========
        private bool cursorOverDropdown;

        public List<stringBoolStruct> GetValues()
        {
            return options;
        }

        void Start()
        {
            StartInit();
        }

        void StartInit()
        {
            cursorOverDropdown = false;
            //=============
            templateGO = transform.Find("Template").gameObject;
            itemGO = templateGO.transform.Find("Viewport").Find("Content").Find("Item").gameObject;
            
            dropdownListIsActive = false;
            if(transform.Find("Dropdown List") == null)
            {
                dropdownListGO = Instantiate(templateGO, transform);
                dropdownListGO.name = "Dropdown List";
            }
            else
                dropdownListGO = transform.Find("Dropdown List").gameObject;
            
            // Destroy the Template Item
            if(dropdownListGO.transform.Find("Viewport").Find("Content").Find("Item") != null)
                Destroy(dropdownListGO.transform.Find("Viewport").Find("Content").Find("Item").gameObject);
            contentGO = dropdownListGO.transform.Find("Viewport").Find("Content").gameObject;
        }

        void InitOptions()
        {
            InitOptions(options);
        }

        void InitOptions(List<stringBoolStruct> optionsList)
        {
            StartInit();
            options = optionsList;
            dropdownListGO.SetActive(true);
            int yPos = 0;
            int itemHeight = 20;
            if(options == null) { return; }
            foreach(stringBoolStruct option in optionsList)
            {
                // Create a new GameObject Item
                GameObject item = Instantiate(itemGO, new Vector3(0f, 0f, 0f), Quaternion.identity, contentGO.transform);
                GameObject checkmark = item.transform.Find("Item Checkmark").gameObject;
                checkmark.SetActive(option.optionIsSelected);

                RectTransform itemRT = item.GetComponent<RectTransform>();
                itemRT.anchorMin = new Vector2(0f, 0.5f);
                itemRT.anchorMax = new Vector2(1f, 0.5f);
                itemRT.pivot = new Vector2(0.5f, 0.5f);
                itemRT.anchoredPosition = new Vector2(0f, yPos);

                item.name = "Item : " + option.optionString;
                TMPro.TMP_Text itemText = item.transform.Find("Item Label").GetComponent<TMPro.TMP_Text>();
                itemText.text = option.optionString;
                yPos -= itemHeight;

                MSButton itemBtn = item.GetComponent<MSButton>();
                itemBtn.normalColor = normalColor;
                itemBtn.pressedColor = pressedColor;
                itemBtn.highlightedColor = highlightedColor;
                itemBtn.selectedColor = selectedColor;
                itemBtn.disabledColor = disabledColor;

                itemBtn.UpdateButton(option.optionIsSelected);
                if(itemBtn.OnClick==null)
                    itemBtn.OnClick = new UnityEngine.Events.UnityEvent();
                itemBtn.OnClick.AddListener(delegate{OnItemClick(option, item);});
            }
            RectTransform dropdownlistRT = dropdownListGO.GetComponent<RectTransform>();
            dropdownlistRT.sizeDelta = new Vector2(dropdownlistRT.sizeDelta.x, Mathf.Abs(yPos)+dropdownlistRT.anchoredPosition.y+4f);
            dropdownListGO.SetActive(false);
            UpdateDropdownMainText();
        }

        public void ClearOptions()
        {
            ClearDropdownGOs();
            RectTransform dropdownlistRT = dropdownListGO.GetComponent<RectTransform>();
            dropdownlistRT.sizeDelta = new Vector2(dropdownlistRT.sizeDelta.x, dropdownlistRT.anchoredPosition.y+4f);
            TMPro.TMP_Text mainTitle = transform.Find("Label").GetComponent<TMPro.TMP_Text>();
            mainTitle.text = "None";
            options = new List<stringBoolStruct>();
        }

        private void ClearDropdownGOs()
        {
            foreach(Transform child in contentGO.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void SetOptions(List<stringBoolStruct> optionsData)
        {
            InitOptions(optionsData);
        }

        void OnItemClick(stringBoolStruct option, GameObject clickedItem)
        {
            StartInit();
            int idx = options.FindIndex(0, options.Count, item => item.optionString.Equals(option.optionString));
            stringBoolStruct itm = new stringBoolStruct(option.optionString, !option.optionIsSelected);
            options.RemoveAt(idx);
            options.Insert(idx, itm);

            GameObject checkmark = clickedItem.transform.Find("Item Checkmark").gameObject;
            checkmark.SetActive(itm.optionIsSelected);
            UpdateDropdownMainText();
            ClearDropdownGOs();
            InitOptions();

            if(OnValueChanged != null)
                OnValueChanged.Invoke();
        }

        void UpdateDropdownMainText()
        {
            string title = "None";
            int nb_selected = 0;
            foreach(stringBoolStruct item in options)
            {
                if(item.optionIsSelected && nb_selected == 0)
                    title = item.optionString;
                else if(item.optionIsSelected && nb_selected >= 1)
                    title = "Mixed";

                if(item.optionIsSelected)
                    nb_selected++;
            }
            if(nb_selected == options.Count)
                title = "Everything";
            else if(nb_selected == 0)
                title = "None";
            TMPro.TMP_Text mainTitle = transform.Find("Label").GetComponent<TMPro.TMP_Text>();
            mainTitle.text = title;
        }

        public void UnfoldDropdownList()
        {
            dropdownListIsActive = !dropdownListIsActive;
            dropdownListGO.SetActive(dropdownListIsActive);
        }
        //====================================
        //====================================
        void Update()
        {
            if(Input.GetMouseButton(0))
            {
                if(!cursorOverDropdown && dropdownListIsActive)
                    dropdownListGO.SetActive(dropdownListIsActive = false);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(OnClick != null)
                OnClick.Invoke();
        }

        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            cursorOverDropdown = true;
            if(OnEnter != null)
                OnEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData pointerEventData)
        {
            cursorOverDropdown = false;
            if(OnExit != null)
                OnExit.Invoke();
        }
    }
}