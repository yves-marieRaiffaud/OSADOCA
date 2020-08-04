using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class UI_SimSettings_Panel : MonoBehaviour
{
    // This script is attached to the 'Content' GameObject, right under the 'Viewport' GameObject of the Sim_Setting Scrollview
    //=========================================================================================================================
    [Header("Prefabs for each SimSetting Category")]
    public GameObject prefabSection_Title;
    public GameObject prefabSimSetting_int;
    public GameObject prefabSimSetting_float;
    public GameObject prefabSimSetting_bool;
    public GameObject prefab_verticalSeparator;
    public GameObject prefab_horizontalSeparator;
    //================================================
    private RectTransform rectTransform;
    private SimulationEnv simulationEnv;
    private string simSettingJSONPath;
    private int currCategoryBeingDrawn;
    private int yPosition;

    void Start()
    {
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
                if(addRight) {
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
                if(addRight) {
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

    private void AddSeparator(int newYPosition, GameObject prefabGO)
    {
        GameObject verticalSepGO = GameObject.Instantiate(prefabGO, new Vector3(0f, 0f, 0f), Quaternion.identity, transform);
        RectTransform rectTransform = verticalSepGO.GetComponent<RectTransform>();
        
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0, (float)(newYPosition));
    }
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

    private void UIAddNewSimSetting_Float(SimSettingFloat setting, int newYPosition, bool addLeft)
    {
        GameObject floatSection_Panel = GameObject.Instantiate(prefabSimSetting_float, new Vector3(0f, 0f, 0f), Quaternion.identity, transform);
        RectTransform rectTransform = floatSection_Panel.GetComponent<RectTransform>();

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
        //=================
        TMPro.TMP_Text setting_txt = floatSection_Panel.transform.Find("Setting_txt").GetComponent<TMPro.TMP_Text>();
        setting_txt.text = setting.displayName;

        Slider floatSlider = floatSection_Panel.transform.Find("Int_Slider").GetComponent<Slider>();
        floatSlider.minValue = setting.minValue;
        floatSlider.maxValue = setting.maxValue;
        floatSlider.value = setting.value;
        
        TMPro.TMP_InputField intInputField = floatSection_Panel.transform.Find("Int_InputField").GetComponent<TMPro.TMP_InputField>();
        intInputField.text = setting.value.ToString();

        floatSlider.onValueChanged.AddListener(delegate { 
            OnFloatSliderValueUpdate(floatSlider, intInputField);
            UpdateSimSettingFloatValue(setting, intInputField);
            SaveSettingsToFile();
        });
        intInputField.onValueChanged.AddListener(delegate {
            OnFloatInputFieldValueUpdate(intInputField, floatSlider);
            UpdateSimSettingFloatValue(setting, intInputField);
            SaveSettingsToFile();
        });

        Button revert_btn = floatSection_Panel.transform.Find("Revert_btn").GetComponent<Button>();
        revert_btn.onClick.AddListener(delegate{
            OnRevert_BtnClick<float>(setting.default_value, floatSlider, intInputField);
        });
    }

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

    private void UIAddNewSimSetting_Int(SimSettingInt setting, int newYPosition, bool addLeft)
    {
        GameObject intSection_Panel = GameObject.Instantiate(prefabSimSetting_int, new Vector3(0f, 0f, 0f), Quaternion.identity, transform);
        RectTransform rectTransform = intSection_Panel.GetComponent<RectTransform>();

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
        //=================
        TMPro.TMP_Text setting_txt = intSection_Panel.transform.Find("Setting_txt").GetComponent<TMPro.TMP_Text>();
        setting_txt.text = setting.displayName;

        Slider intSlider = intSection_Panel.transform.Find("Int_Slider").GetComponent<Slider>();
        intSlider.minValue = setting.minValue;
        intSlider.maxValue = setting.maxValue;
        intSlider.value = (float)setting.value;
        
        TMPro.TMP_InputField intInputField = intSection_Panel.transform.Find("Int_InputField").GetComponent<TMPro.TMP_InputField>();
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

        Button revert_btn = intSection_Panel.transform.Find("Revert_btn").GetComponent<Button>();
        revert_btn.onClick.AddListener(delegate{
            OnRevert_BtnClick<int>(setting.default_value, intSlider, intInputField);
        });
    }

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
    private void UIAddNewSimSetting_Bool(SimSettingBool setting, int newYPosition, bool addLeft)
    {
        GameObject boolSection_Panel = GameObject.Instantiate(prefabSimSetting_bool, new Vector3(0f, 0f, 0f), Quaternion.identity, transform);
        RectTransform rectTransform = boolSection_Panel.GetComponent<RectTransform>();

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
        //=================
        TMPro.TMP_Text setting_txt = boolSection_Panel.transform.Find("Setting_txt").GetComponent<TMPro.TMP_Text>();
        setting_txt.text = setting.displayName;

        ToggleSwitch toggleSwitch = boolSection_Panel.transform.Find("Toggle").GetComponent<ToggleSwitch>();
        toggleSwitch.isOn = setting.value;

        toggleSwitch.onValueChanged.AddListener(delegate { 
            toggleSwitch.Toggle(toggleSwitch.isOn);
            UpdateSimSettingBoolValue(setting, toggleSwitch);
            SaveSettingsToFile();
        });

        Button revert_btn = boolSection_Panel.transform.Find("Revert_btn").GetComponent<Button>();
        revert_btn.onClick.AddListener(delegate{
            OnRevert_BtnClick<bool>(setting.default_value, toggleSwitch);
        });
    }
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
    
    private void SaveSettingsToFile()
    {
        string filepath = UsefulFunctions.WriteToFileSimuSettingsSaveData(simulationEnv);
        Debug.Log("SimSettings has succesfuly been saved at : '" + filepath + "'.");
    }
}