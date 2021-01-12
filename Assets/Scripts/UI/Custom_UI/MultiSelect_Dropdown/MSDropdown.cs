using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Linq;

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
        public bool displaySelectionOptions=true;
        public bool useCustomDropdownListWidth=false;
        public int dropdownListCustomWidth=0;
        public bool useCustomShapedButtons=false;
        CustomShaped_Button customBtnsScript=null;
        //=====
        stringBoolStruct[] _options;
        public List<stringBoolStruct> options
        {
            get {
                return _options.ToList<stringBoolStruct>();
            }
            set {
                _options=value.ToArray();
            }
        }
        
        List<MSButton> _buttonsList;
        public List<MSButton> buttonsList
        {
            get {
                return _buttonsList;
            }
        }
        //==========
        TMPro.TMP_Text mainTitle;
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
        private bool hasDoneStart=false;

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
            if(useCustomShapedButtons) {
                bool foundScript = transform.TryGetComponent<CustomShaped_Button>(out customBtnsScript);
                if(foundScript) {
                    customBtnsScript.OnEnter.AddListener(OnPointerEnter_CustomShapedBtn);
                    customBtnsScript.OnExit.AddListener(OnPointerExit_CustomShapedBtn);
                }
            }

            _buttonsList = new List<MSButton>();

            templateGO = transform.Find("Template").gameObject;
            itemGO = templateGO.transform.Find("Viewport").Find("Content").Find("Item").gameObject;
            mainTitle = transform.Find("Label").GetComponent<TMPro.TMP_Text>();
            mainTitle.gameObject.SetActive(displaySelectionOptions);
            transform.Find("Arrow").gameObject.SetActive(displaySelectionOptions);
            if(transform.Find("Image") != null) {
                Image img = transform.Find("Image").GetComponent<Image>();
                if(img.sprite != null)
                    img.gameObject.SetActive(!displaySelectionOptions);
                else
                    img.gameObject.SetActive(false);
            }

            dropdownListIsActive = false;
            if(transform.Find("Dropdown List") == null) {
                dropdownListGO = Instantiate(templateGO, transform);
                dropdownListGO.name = "Dropdown List";
            }
            else
                dropdownListGO = transform.Find("Dropdown List").gameObject;

            if(useCustomDropdownListWidth) {
                RectTransform dropdownListRT = dropdownListGO.GetComponent<RectTransform>();
                dropdownListRT.sizeDelta = new Vector2(dropdownListCustomWidth, dropdownListRT.sizeDelta.y);
            }
            
            // Destroy the Template Item
            if(dropdownListGO.transform.Find("Viewport").Find("Content").Find("Item") != null)
                Destroy(dropdownListGO.transform.Find("Viewport").Find("Content").Find("Item").gameObject);
            contentGO = dropdownListGO.transform.Find("Viewport").Find("Content").gameObject;

            hasDoneStart=true;
        }

        void InitOptions()
        {
            InitOptions(options);
        }
        void InitOptions(List<stringBoolStruct> optionsList)
        {
            if(!hasDoneStart)
                Start();

            //StartInit();
            options = optionsList;
            dropdownListGO.SetActive(true);
            float yPos = 0;
            float itemHeight = itemGO.GetComponent<RectTransform>().sizeDelta.y;
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
                itemBtn.Awake();
                _buttonsList.Add(itemBtn);
                itemBtn.normalColor = normalColor;
                itemBtn.pressedColor = pressedColor;
                itemBtn.highlightedColor = highlightedColor;
                itemBtn.selectedColor = selectedColor;
                itemBtn.disabledColor = disabledColor;

                itemBtn.UpdateButton(option.optionIsSelected);
                itemBtn.OnClick.AddListener(delegate{OnItemClick(option, item);});
            }
            RectTransform dropdownlistRT = dropdownListGO.GetComponent<RectTransform>();
            dropdownlistRT.sizeDelta = new Vector2(dropdownlistRT.sizeDelta.x, Mathf.Abs(yPos)+dropdownlistRT.anchoredPosition.y+4f);
            dropdownListGO.SetActive(false);
            UpdateDropdownMainText();
        }

        public void ClearOptions()
        {
            if(!hasDoneStart)
                Start();
            ClearDropdownGOs();
            RectTransform dropdownlistRT = dropdownListGO.GetComponent<RectTransform>();
            dropdownlistRT.sizeDelta = new Vector2(dropdownlistRT.sizeDelta.x, dropdownlistRT.anchoredPosition.y+4f);
            TMPro.TMP_Text mainTitle = transform.Find("Label").GetComponent<TMPro.TMP_Text>();
            mainTitle.text = "None";
            options = new List<stringBoolStruct>();
            _buttonsList.Clear();
        }
        private void ClearDropdownGOs()
        {
            if(!hasDoneStart)
                Start();
            foreach(Transform child in contentGO.transform)
                Destroy(child.gameObject);
        }

        public void SetOptions(List<stringBoolStruct> optionsData)
        {
            if(!hasDoneStart)
                Start();
            InitOptions(optionsData);
        }

        void OnItemClick(stringBoolStruct option, GameObject clickedItem)
        {
            if(!hasDoneStart)
                Start();

            int idx = options.FindIndex(0, options.Count, item => item.optionString.Equals(option.optionString));
            //stringBoolStruct itm = new stringBoolStruct(option.optionString, !option.optionIsSelected);
            /*options.RemoveAt(idx);
            options.Insert(idx, itm);*/
            _options[idx].optionIsSelected = !_options[idx].optionIsSelected;

            GameObject checkmark = clickedItem.transform.Find("Item Checkmark").gameObject;
            checkmark.SetActive(_options[idx].optionIsSelected);
            UpdateDropdownMainText();
            /*ClearDropdownGOs();
            InitOptions();*/

            if(OnValueChanged != null)
                OnValueChanged.Invoke();
        }

        void UpdateDropdownMainText()
        {
            if(!hasDoneStart)
                Start();

            string title = "None";
            int nb_selected = 0;
            foreach(stringBoolStruct item in options) {
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
            mainTitle.text = title;
        }

        public void UnfoldDropdownList()
        {
            Debug.LogWarning("unfolding...");
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
            if(!hasDoneStart)
                Start();
            if(OnClick != null)
                OnClick.Invoke();
        }
        
        void OnPointerEnter_CustomShapedBtn()
        {
            if(!hasDoneStart)
                Start();
            cursorOverDropdown = true;
            if(OnEnter != null)
                OnEnter.Invoke();
        }
        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            if(useCustomShapedButtons)
                return;
            else
                OnPointerEnter_CustomShapedBtn();
        }
        void OnPointerExit_CustomShapedBtn()
        {
            Debug.LogWarning("onPointyer exit");
            Debug.LogWarning("hasDoneStart = " + hasDoneStart);
            if(!hasDoneStart)
                Start();
            cursorOverDropdown = false;
            if(OnExit != null)
                OnExit.Invoke();
        }
        public void OnPointerExit(PointerEventData pointerEventData)
        {
            if(useCustomShapedButtons)
                return;
            else
                OnPointerExit_CustomShapedBtn();
        }
    }
}