using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using Fncs = UsefulFunctions;

public class UI_SimSettings_Panel : MonoBehaviour
{
    // This script is attached to the 'Content' GameObject, right under the 'Viewport' GameObject of the Sim_Setting Scrollview
    //=========================================================================================================================
    [Header("Prefabs for the Settings Panel")]
    public GameObject prefabSection_Title;
    public GameObject prefabSimSetting_int;
    public GameObject prefabSimSetting_float;
    public GameObject prefabSimSetting_bool;
    public GameObject prefab_verticalSeparator;
    public GameObject prefab_horizontalSeparator;
    [Header("Prefabs for the More Info Panel")]
    public GameObject prefab_moreInfoTitle;
    public GameObject prefab_moreInfo_text;
    public GameObject prefab_moreInfo_horizontalSep;
    public GameObject prefab_moreInfo_formula;
    public RectTransform moreInfoSetting_Panel;
    //================================================
    private RectTransform rectTransform;
    private SimulationEnv simulationEnv;
    private string simSettingJSONPath;
    private int currCategoryBeingDrawn;
    private int yPosition;

    void Start()
    {
        // At start, hide the 'moreInfoPanel' as no settings has been selected for more info
        moreInfoSetting_Panel.gameObject.SetActive(false);

        rectTransform = gameObject.GetComponent<RectTransform>();

        currCategoryBeingDrawn = -1;
        int initialTopMargin = -30;
        yPosition = initialTopMargin; // Initial top padding

        simulationEnv = ScriptableObject.CreateInstance<SimulationEnv>();
        simSettingJSONPath = Application.persistentDataPath + Filepaths.simulation_settings;
        JsonUtility.FromJsonOverwrite(File.ReadAllText(simSettingJSONPath), simulationEnv);

        int settingCounter = 0;
        bool addLeft = true; // Start by adding to the left
        bool addRight = false;
        foreach(var property in simulationEnv.GetType().GetFields()) 
        {
            bool justAddedCategoryTitle = false;
            if(property.GetValue(simulationEnv) is SimSettingInt)
            {
                SimSettingInt setting = (SimSettingInt) property.GetValue(simulationEnv);
                if(currCategoryBeingDrawn == -1 || (int)setting.category != currCategoryBeingDrawn)
                {
                    if(currCategoryBeingDrawn != -1) {
                        // Add horizontal separator at the end of the previous category
                        AddSeparator(yPosition-100, prefab_horizontalSeparator);
                    }
                    // Need to add the Subsection label
                    if(yPosition != initialTopMargin) { yPosition -= 120; }
                    UIAddNewCategoryTitle((int)setting.category, yPosition);
                    justAddedCategoryTitle = true;
                    addLeft = true;
                    addRight = false;
                }

                if(justAddedCategoryTitle) { yPosition -= 80; }
                else if(!justAddedCategoryTitle && addLeft) {
                    yPosition -= 80;
                }
                if(addLeft) {
                    // Add vertical separator between two settings on the same line
                    AddSeparator(yPosition, prefab_verticalSeparator);
                }
                // Adding the setting prefab
                UIAddNewSimSetting_Int(setting, yPosition, addLeft);
                //===============
                addLeft = !addLeft;
                addRight = !addRight;
                currCategoryBeingDrawn = (int)setting.category;
                settingCounter++;
            }

            else if(property.GetValue(simulationEnv) is SimSettingBool)
            {
                SimSettingBool setting = (SimSettingBool) property.GetValue(simulationEnv);
                if(currCategoryBeingDrawn == -1 || (int)setting.category != currCategoryBeingDrawn)
                {
                    if(currCategoryBeingDrawn != -1) {
                        // Add horizontal separator at the end of the previous category
                        AddSeparator(yPosition-100, prefab_horizontalSeparator);
                    }
                    // Need to add the Subsection label
                    if(yPosition != initialTopMargin) { yPosition -= 120; }
                    UIAddNewCategoryTitle((int)setting.category, yPosition);
                    justAddedCategoryTitle = true;
                    addLeft = true;
                    addRight = false;
                }

                if(justAddedCategoryTitle) { yPosition -= 80; }
                else if(!justAddedCategoryTitle && addLeft) {
                    yPosition -= 80;
                }
                if(addLeft) {
                    // Add vertical separator between two settings on the same line
                    AddSeparator(yPosition, prefab_verticalSeparator);
                }
                // Adding the setting prefab
                UIAddNewSimSetting_Bool(setting, yPosition, addLeft);
                //===============
                addLeft = !addLeft;
                addRight = !addRight;
                currCategoryBeingDrawn = (int)setting.category;
                settingCounter++;
            }

            else if(property.GetValue(simulationEnv) is SimSettingFloat)
            {
                SimSettingFloat setting = (SimSettingFloat) property.GetValue(simulationEnv);
                if(currCategoryBeingDrawn == -1 || (int)setting.category != currCategoryBeingDrawn)
                {
                    if(currCategoryBeingDrawn != -1) {
                        // Add horizontal separator at the end of the previous category
                        AddSeparator(yPosition-100, prefab_horizontalSeparator);
                    }
                    // Need to add the Subsection label
                    if(yPosition != initialTopMargin) { yPosition -= 120; }
                    UIAddNewCategoryTitle((int)setting.category, yPosition);
                    justAddedCategoryTitle = true;
                    addLeft = true;
                    addRight = false;
                }

                if(justAddedCategoryTitle) { yPosition -= 80; }
                else if(!justAddedCategoryTitle && addLeft) {
                    yPosition -= 80;
                }
                if(addRight) {
                    // Add vertical separator between two settings on the same line
                    AddSeparator(yPosition, prefab_verticalSeparator);
                }
                // Adding the setting prefab
                UIAddNewSimSetting_Float(setting, yPosition, addLeft);
                //===============
                addLeft = !addLeft;
                addRight = !addRight;
                currCategoryBeingDrawn = (int)setting.category;
                settingCounter++;
            }
        }
        // Need to a final horizontal separator for the last category that was drawn
        AddSeparator(yPosition-100, prefab_horizontalSeparator);
        // Manually adjust the size of the content container
        rectTransform.sizeDelta = new Vector2(0f, Mathf.Abs(yPosition-140));
    }
    //=============================================================================================
    //=============================================================================================
    //=============================================================================================
    //=============================================================================================
    private void AddSeparator(int newYPosition, GameObject prefabGO)
    {
        GameObject verticalSepGO = GameObject.Instantiate(prefabGO, new Vector3(0f, 0f, 0f), Quaternion.identity, transform);
        RectTransform rectTransform = verticalSepGO.GetComponent<RectTransform>();
        
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0, (float)(newYPosition));
    }
    
    private GameObject SpawnPosition_NewSimSettingPrefab(GameObject prefabToSpaw, int newYPosition, bool addLeft)
    {
        GameObject section_Panel = GameObject.Instantiate(prefabToSpaw, Vector3.zero, Quaternion.identity, transform);
        RectTransform rectTransform = section_Panel.GetComponent<RectTransform>();

        int xPosition = 30; // Padding of the prefab
        if(addLeft)
        {
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
        }
        else {
            rectTransform.anchorMin = new Vector2(1f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(1f, 1f);
            xPosition = -xPosition;
        }
        rectTransform.anchoredPosition = new Vector2((float)xPosition, (float)(newYPosition));
        return section_Panel;
    }
    //=============================================================================================
    //=============================================================================================
    //=============================================================================================
    //=============================================================================================
    private void OnintSliderValueUpdate(Slider intSlider, TMPro.TMP_InputField intInputField)
    {
        // Called when the intSlider has been modified. Thus must modify the value of the int_inputField based on the new value of the slider
        intInputField.text = ((int)intSlider.value).ToString();
    }

    private void OnFloatSliderValueUpdate(Slider floatSlider, TMPro.TMP_InputField floatInputField)
    {
        // Called when the intSlider has been modified. Thus must modify the value of the int_inputField based on the new value of the slider
        floatInputField.text = UsefulFunctions.FloatToSignificantDigits(floatSlider.value, 5);
    }

    private void OnintInputFieldValueUpdate(TMPro.TMP_InputField intInputField, Slider intSlider)
    {
        // Called when the intInputField has been modified. Thus must modify the value of the intSlider based on the new value of the intInputField
        int parsedInt;
        if(int.TryParse(intInputField.text, out parsedInt))
        {
            intSlider.value = (float)parsedInt;
        }
    }

    private void OnFloatInputFieldValueUpdate(TMPro.TMP_InputField intInputField, Slider floatSlider)
    {
        // Called when the intInputField has been modified. Thus must modify the value of the intSlider based on the new value of the intInputField
        float parsedFloat;
        if(float.TryParse(intInputField.text, out parsedFloat))
        {
            floatSlider.value = parsedFloat;
        }
    }
    //=============================================================================================
    //=============================================================================================
    //=============================================================================================
    //=============================================================================================
    private void UIAddNewCategoryTitle(int categoryEnumIntToDraw, int newYPosition)
    {
        GameObject title = GameObject.Instantiate(prefabSection_Title, new Vector3(0f, 0f, 0f), Quaternion.identity, transform);
        RectTransform rectTransform = title.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);

        rectTransform.anchoredPosition = new Vector2(0f, (float)(newYPosition));

        TMPro.TMP_Text textField = title.GetComponent<TMPro.TMP_Text>();
        textField.text = SimulationEnv.simSettingCategoryLabels[categoryEnumIntToDraw];
    }

    private void UIAddNewSimSetting_Bool(SimSettingBool setting, int newYPosition, bool addLeft)
    {
        GameObject boolSectionPanel = SpawnPosition_NewSimSettingPrefab(prefabSimSetting_bool, newYPosition, addLeft);
        //=================
        TMPro.TMP_Text setting_txt = boolSectionPanel.transform.Find("Setting_txt").GetComponent<TMPro.TMP_Text>();
        setting_txt.text = setting.displayName;

        ToggleSwitch toggleSwitch = boolSectionPanel.transform.Find("Toggle").GetComponent<ToggleSwitch>();
        toggleSwitch.isOn = setting.value;

        toggleSwitch.onValueChanged.AddListener(delegate { 
            toggleSwitch.Toggle(toggleSwitch.isOn);
            UpdateSimSettingBoolValue(setting, toggleSwitch);
            SaveSettingsToFile();
        });

        Button revert_btn = boolSectionPanel.transform.Find("Revert_btn").GetComponent<Button>();
        revert_btn.onClick.AddListener(delegate{
            OnRevert_BtnClick<bool>(setting.default_value, toggleSwitch);
        });

        Init_InfoBtn<SimSettingBool, bool>(boolSectionPanel, setting);
    }

    private void UIAddNewSimSetting_Int(SimSettingInt setting, int newYPosition, bool addLeft)
    {
        GameObject intSectionPanel = SpawnPosition_NewSimSettingPrefab(prefabSimSetting_int, newYPosition, addLeft);
        //=================
        TMPro.TMP_Text setting_txt = intSectionPanel.transform.Find("Setting_txt").GetComponent<TMPro.TMP_Text>();
        setting_txt.text = setting.displayName;

        Slider intSlider = intSectionPanel.transform.Find("Int_Slider").GetComponent<Slider>();
        intSlider.minValue = setting.minValue;
        intSlider.maxValue = setting.maxValue;
        intSlider.value = (float)setting.value;
        
        TMPro.TMP_InputField intInputField = intSectionPanel.transform.Find("Int_InputField").GetComponent<TMPro.TMP_InputField>();
        intInputField.text = setting.value.ToString();

        intSlider.onValueChanged.AddListener(delegate { 
            OnintSliderValueUpdate(intSlider, intInputField);
            UpdateSimSettingIntValue(setting, intInputField);
            SaveSettingsToFile();
        });
        intInputField.onValueChanged.AddListener(delegate {
            OnintInputFieldValueUpdate(intInputField, intSlider);
            UpdateSimSettingIntValue(setting, intInputField);
            SaveSettingsToFile();
        });

        Button revert_btn = intSectionPanel.transform.Find("Revert_btn").GetComponent<Button>();
        revert_btn.onClick.AddListener(delegate{
            OnRevert_BtnClick<int>(setting.default_value, intSlider, intInputField);
        });

        Init_InfoBtn<SimSettingInt, int>(intSectionPanel, setting);
    }

    private void UIAddNewSimSetting_Float(SimSettingFloat setting, int newYPosition, bool addLeft)
    {
        GameObject floatSectionPanel = SpawnPosition_NewSimSettingPrefab(prefabSimSetting_float, newYPosition, addLeft);
        //=================
        TMPro.TMP_Text setting_txt = floatSectionPanel.transform.Find("Setting_txt").GetComponent<TMPro.TMP_Text>();
        setting_txt.text = setting.displayName;

        Slider floatSlider = floatSectionPanel.transform.Find("Float_Slider").GetComponent<Slider>();
        floatSlider.minValue = setting.minValue;
        floatSlider.maxValue = setting.maxValue;
        floatSlider.value = setting.value;
        
        TMPro.TMP_InputField floatInputField = floatSectionPanel.transform.Find("Float_InputField").GetComponent<TMPro.TMP_InputField>();
        floatInputField.text = setting.value.ToString();

        floatSlider.onValueChanged.AddListener(delegate { 
            OnFloatSliderValueUpdate(floatSlider, floatInputField);
            UpdateSimSettingFloatValue(setting, floatInputField);
            SaveSettingsToFile();
        });
        floatInputField.onValueChanged.AddListener(delegate {
            OnFloatInputFieldValueUpdate(floatInputField, floatSlider);
            UpdateSimSettingFloatValue(setting, floatInputField);
            SaveSettingsToFile();
        });

        Button revert_btn = floatSectionPanel.transform.Find("Revert_btn").GetComponent<Button>();
        revert_btn.onClick.AddListener(delegate{
            OnRevert_BtnClick<float>(setting.default_value, floatSlider, floatInputField);
        });

        Init_InfoBtn<SimSettingFloat, float>(floatSectionPanel, setting);
    }
    //=============================================================================================
    //=============================================================================================
    //=============================================================================================
    //=============================================================================================
    private void Init_InfoBtn<T1, T2>(GameObject sectionPanel, object simSettingObj)
    where T1 : SimSettingInterface<T2>
    {
        Button info_btn = sectionPanel.transform.Find("Info_btn").GetComponent<Button>();
        info_btn.onClick.AddListener(delegate{
            OnInfo_BtnClick<T1, T2>(simSettingObj);
        });
    }

    private void OnInfo_BtnClick<T1, T2>(object simSettingObj)
    where T1 : SimSettingInterface<T2>
    {
        T1 simSetting = (T1)(dynamic)simSettingObj;
        //==================
        moreInfoSetting_Panel.gameObject.SetActive(true);
        //==================
        // First clear everything in the 'MoreInfoSetting_Panel' except for the 'MoreInfo_Title' GameObject
        foreach(Transform child in moreInfoSetting_Panel.transform)
        {
            if(!child.name.Equals("MoreInfo_Title")) {
                GameObject.Destroy(child.gameObject);
            }
        }
        //==================
        GameObject title;
        if(moreInfoSetting_Panel.transform.Find("MoreInfo_Title") == null)
        {
            title = GameObject.Instantiate(prefab_moreInfoTitle, Vector3.zero, Quaternion.identity, moreInfoSetting_Panel);
            RectTransform rectTransform = title.GetComponent<RectTransform>();

            RectTransform parent_RT = title.transform.parent.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(parent_RT.anchoredPosition.x/2f, 0f);
        }
        else {
            title = moreInfoSetting_Panel.transform.Find("MoreInfo_Title").gameObject;
        }
        TMPro.TMP_Text title_txt = title.GetComponent<TMPro.TMP_Text>();
        title_txt.text = simSetting.displayName;
        //==================
        DipslayInfoSettingParams<T1, T2>(simSettingObj);
    }

    private void DipslayInfoSettingParams<T1, T2>(object simSettingObj)
    where T1 : SimSettingInterface<T2>
    {
        // Casting to the specific class. Either 'SimSettingFloat', 'SimSettingInt' or 'SimSettingBool' to get access to its specific properties        
        if(simSettingObj is SimSettingFloat)
        {
            SimSettingFloat setting = (SimSettingFloat)simSettingObj;
            string paramsDetails = "<i>default value = " + Fncs.FloatToSignificantDigits(setting.default_value, 4) + "\n" +
                                    "type = " + setting.type + "\n" + 
                                    "min value = " + Fncs.FloatToSignificantDigits(setting.minValue, 4) + "\n" +
                                    "max value = " + Fncs.FloatToSignificantDigits(setting.maxValue, 4) + "</i>";

            (Vector3, Vector3) result = AddSimSettings_Info_HorizontalSeparators(paramsDetails);
            AddSimSettings_Info_Texts<T1, T2>(simSettingObj, result.Item1, result.Item2);
        }
        else if(simSettingObj is SimSettingInt)
        {
            SimSettingInt setting = (SimSettingInt)simSettingObj;
            string paramsDetails = "<i>default value = " + Fncs.FloatToSignificantDigits(setting.default_value, 4) + "\n" +
                                    "type = " + setting.type + "\n" + 
                                    "min value = " + setting.minValue.ToString() + "\n" +
                                    "max value = " + setting.maxValue.ToString() + "</i>";

            (Vector3, Vector3) result = AddSimSettings_Info_HorizontalSeparators(paramsDetails);
            AddSimSettings_Info_Texts<T1, T2>(simSettingObj, result.Item1, result.Item2);
        }
        else if(simSettingObj is SimSettingBool)
        {
            SimSettingBool setting = (SimSettingBool)simSettingObj;
            string paramsDetails = "<i>default value = " + setting.default_value.ToString() + "\n" +
                                    "type = " + setting.type + "</i>";

            (Vector3, Vector3) result = AddSimSettings_Info_HorizontalSeparators(paramsDetails);
            AddSimSettings_Info_Texts<T1, T2>(simSettingObj, result.Item1, result.Item2);
        }
    }

    private (Vector3, Vector3) AddSimSettings_Info_HorizontalSeparators(string paramsDetails)
    {
        Vector3 pos = moreInfoSetting_Panel.transform.position - new Vector3(moreInfoSetting_Panel.rect.width/2f, 50f, 0f);

        GameObject horizontalSepTop = GameObject.Instantiate(prefab_moreInfo_horizontalSep, pos+new Vector3(0f, 5f, 0f), Quaternion.identity, moreInfoSetting_Panel);
        GameObject paramsDetailsGO = GameObject.Instantiate(prefab_moreInfo_text, pos, Quaternion.identity, moreInfoSetting_Panel);
        TMPro.TMP_Text paramsDetailsTxt = paramsDetailsGO.GetComponent<TMPro.TMP_Text>();
        paramsDetailsTxt.fontSize = 14f;
        paramsDetailsTxt.text = paramsDetails;
        
        RectTransform rt = paramsDetailsGO.GetComponent<RectTransform>();
        pos = paramsDetailsGO.transform.position - new Vector3(0f, (int)(paramsDetailsTxt.preferredHeight), 0f);
        GameObject horizontalSepBot = GameObject.Instantiate(prefab_moreInfo_horizontalSep, pos-new Vector3(0f, 5f, 0f), Quaternion.identity, moreInfoSetting_Panel);

        return (pos, horizontalSepBot.transform.position);
    }

    private void AddSimSettings_Info_Texts<T1, T2>(object simSettingObj, Vector3 pos, Vector3 horizontalSepBotPos)
    where T1 : SimSettingInterface<T2>
    {
        T1 setting = (T1)(dynamic)simSettingObj;
        if(setting.simSettings_Info == null) { return; }
        Dictionary<string, bool> infoStrings = setting.simSettings_Info.text_strings;
        GameObject lastSpawnedGO = null;
        TEXDraw lastFormulaTxt = null;
        TMPro.TMP_Text lastTMPTxt = null;
        bool lastSpawedWasFormula = false;
        bool isIndex0 = true;
        Vector3 interCompSpacing = new Vector3(0f, 10f, 0f);
        foreach(KeyValuePair<string, bool> pair in infoStrings)
        {
            if(isIndex0) {
                pos = horizontalSepBotPos - interCompSpacing;
            }
            else {
                if(lastSpawedWasFormula) {
                    pos = lastSpawnedGO.transform.position - new Vector3(0f, (int)(lastFormulaTxt.preferredHeight), 0f) - interCompSpacing;
                }
                else {
                    pos = lastSpawnedGO.transform.position - new Vector3(0f, (int)(lastTMPTxt.preferredHeight), 0f) - interCompSpacing;
                }
            }

            if(pair.Value)
            {
                // It is a formula
                lastSpawnedGO = GameObject.Instantiate(prefab_moreInfo_formula, pos, Quaternion.identity, moreInfoSetting_Panel);
                lastFormulaTxt = lastSpawnedGO.GetComponent<TEXDraw>();
                lastFormulaTxt.text = pair.Key;
                // End of the section
                isIndex0 = false;
                lastSpawedWasFormula = true;
            }
            else {
                // It is some (rich) text
                lastSpawnedGO = GameObject.Instantiate(prefab_moreInfo_text, pos, Quaternion.identity, moreInfoSetting_Panel);
                lastTMPTxt = lastSpawnedGO.GetComponent<TMPro.TMP_Text>();
                lastTMPTxt.text = pair.Key;
                // End of the section
                isIndex0 = false;
                lastSpawedWasFormula = false;
            }
        }
    }
    //=============================================================================================
    //=============================================================================================
    //=============================================================================================
    //=============================================================================================
    private void OnRevert_BtnClick<T>(T defaultValue, params object[] objToChange)
    {
        if(defaultValue is bool)
        {
            // Only one passed obj : the toggle component
            ToggleSwitch toggleSwitch = (ToggleSwitch)(dynamic)objToChange[0];
            toggleSwitch.isOn = (bool)(dynamic)defaultValue;
        }
        else if(defaultValue is float)
        {
            // For float, first obj is the slider, second obj is the float input field
            float val = (float)(dynamic)defaultValue;
            Slider floatSlider = (Slider)(dynamic)objToChange[0];
            floatSlider.value = val;
            TMPro.TMP_InputField inputField = (TMPro.TMP_InputField)(dynamic)objToChange[1];
            inputField.text = UsefulFunctions.FloatToSignificantDigits(val, 5);
        }
        else if(defaultValue is int)
        {
            // For int, first obj is the slider, second obj is the int input field
            float val = (int)(dynamic)defaultValue;
            Slider intSlider = (Slider)(dynamic)objToChange[0];
            intSlider.value = val;
            TMPro.TMP_InputField inputField = (TMPro.TMP_InputField)(dynamic)objToChange[1];
            inputField.text = defaultValue.ToString();
        }
    }
    //=============================================================================================
    //=============================================================================================
    //=============================================================================================
    //=============================================================================================
    void UpdateSimSettingBoolValue(SimSettingBool settingBool, ToggleSwitch toggleSwitch)
    {
        settingBool.value = toggleSwitch.isOn;
    }

    private void UpdateSimSettingIntValue(SimSettingInt settingInt, TMPro.TMP_InputField intInputField)
    {
        int parsedInt;
        if(int.TryParse(intInputField.text, out parsedInt)) {
            settingInt.value = parsedInt;
        }
    }

    private void UpdateSimSettingFloatValue(SimSettingFloat settingInt, TMPro.TMP_InputField intInputField)
    {
        float parsedInt;
        if(UsefulFunctions.ParseStringToFloat(intInputField.text, out parsedInt)) {
            settingInt.value = parsedInt;
        }
    }
    //=============================================================================================
    //=============================================================================================
    //=============================================================================================
    //=============================================================================================
    private void SaveSettingsToFile()
    {
        string filepath = UsefulFunctions.WriteToFileSimuSettingsSaveData(simulationEnv);
        Debug.Log("SimSettings has succesfuly been saved at : '" + filepath + "'.");
    }
}