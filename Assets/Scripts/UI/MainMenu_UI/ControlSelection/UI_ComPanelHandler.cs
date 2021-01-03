using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using ComOps = CommonMethods.CommunicationOps;
using Communication;
using TMPro;
using MSDropdownNamespace;
using System;
using UnityEngine.Playables;
using UnityEngine.Events;

public class UI_ComPanelHandler : MonoBehaviour
{
    bool isAlreadyAwaken=false;
    internal UnityEvent hasBeenModified;

    ComsOverallHandler comsHandler;

    RectTransform disabledPanel;
    RectTransform enabledPanel;

    TMP_Text disabledPanelTxt;
    TMP_Text panelTitle;
    internal ToggleSwitch mainToggleSwitch; // Toggle switch to enable/disable the whole connection panel
    internal TMP_Dropdown protDropdown; // Dropdown for the the selection of the com protocol to use
    internal TMP_Dropdown dataTypeDropdown; // Dropdown for the the selection of the data type to send/receive

    TMP_Text dataToSendReceive;
    internal MSDropdown dataToSendReceive_MSDrop; // Multi-select dropdown for the selection of the data fields to send (only for UDP/TCP SENDERS for now), else will be null

    internal TMP_InputField ipField;
    internal TMP_InputField portField;

    Button removePanelBtn;
    Button defaultBtn;
    Button testBtn;

    internal void Start()
    {
        if(!isAlreadyAwaken) {
            if(hasBeenModified == null)
                hasBeenModified = new UnityEvent();
            Assign_Variables();
            Init_Variables();
        }
        isAlreadyAwaken = true;
    }
    void Assign_Variables()
    {
        comsHandler = GameObject.Find("ComsHandler").GetComponent<ComsOverallHandler>();

        disabledPanel = transform.Find("Disabled").GetComponent<RectTransform>();
        disabledPanelTxt = transform.Find("Disabled/DisabledChannel_txt").GetComponent<TMP_Text>();
        removePanelBtn = transform.Find("Disabled/RemovePanel_btn").GetComponent<Button>();
        removePanelBtn.onClick.AddListener(delegate {
            OnRemovePanelBtn_Clicked();
            });

        panelTitle = transform.Find("Title_Toggle_Panel/Panel_Title").GetComponent<TMP_Text>();
        mainToggleSwitch = transform.Find("Title_Toggle_Panel/Panel_OverallToggle/Toggle").GetComponent<ToggleSwitch>();
        mainToggleSwitch.onValueChanged.AddListener(delegate {
            OnToggleSwitch_Clicked();
            ModifiedPanel_Event();
            });

        enabledPanel = transform.Find("Enabled").GetComponent<RectTransform>();
        protDropdown = transform.Find("Enabled/Protocol_Panel/Protocol_Dropdown").GetComponent<TMP_Dropdown>();
        protDropdown.ClearOptions();
        protDropdown.onValueChanged.AddListener(delegate {
            OnProtocol_ValueChanged();
            ModifiedPanel_Event();
            });

        dataTypeDropdown = transform.Find("Enabled/DataType_Panel/DataType_Dropdown").GetComponent<TMP_Dropdown>();
        dataTypeDropdown.ClearOptions();
        dataTypeDropdown.onValueChanged.AddListener(delegate {
            OnDataType_ValueChanged();
            ModifiedPanel_Event();
            });

        ipField = transform.Find("Enabled/IP_Panel/IP_input").GetComponent<TMP_InputField>();
        portField = transform.Find("Enabled/Port_Panel/Port_input").GetComponent<TMP_InputField>();

        defaultBtn = transform.Find("Enabled/Btns_Panel/Default_btn").GetComponent<Button>();
        defaultBtn.onClick.AddListener(delegate {
            OnDefaultBtn_Clicked();
            ModifiedPanel_Event();
            });

        testBtn = transform.Find("Enabled/Btns_Panel/Test_btn").GetComponent<Button>();
        testBtn.onClick.AddListener(delegate {
            OnTestBtn_Clicked();
            ModifiedPanel_Event();
            });

        dataToSendReceive_MSDrop = transform.Find("Enabled/DataToSendReceive_Panel/DataToSend_MSDropdown").GetComponent<MSDropdown>();
        //dataToSendReceive_MSDrop.ClearOptions();
        dataToSendReceive_MSDrop.OnValueChanged.AddListener(delegate {
            OnDataToSend_ValueChanged();
            ModifiedPanel_Event();
            });
        dataToSendReceive = transform.Find("Enabled/DataToSendReceive_Panel/DataToSendReceive_txt").GetComponent<TMP_Text>();
    }
    void Init_Variables()
    {
        List<string> protocolOptions = Enum.GetValues(typeof(ComProtocol)).Cast<ComProtocol>().Select(v => v.ToString()).ToList();
        protDropdown.AddOptions(protocolOptions);

        OnProtocol_ValueChanged();
    }
    void ModifiedPanel_Event()
    {
        if(hasBeenModified != null)
            hasBeenModified.Invoke();
    }

    void OnToggleSwitch_Clicked()
    {
        disabledPanel.gameObject.SetActive(!mainToggleSwitch.isOn);
        enabledPanel.gameObject.SetActive(mainToggleSwitch.isOn);
    }

    void OnProtocol_ValueChanged()
    {
        dataTypeDropdown.ClearOptions();
        ComProtocol chosenProt = ComOps.Str_2_ComProtocol(protDropdown.options[protDropdown.value].text);
        ComConectionType possibleCos = ComChannelDict.comEnumsCombinations[chosenProt];
        List<string> possibleCoTypes = Enum.GetValues(typeof(ComConectionType)).Cast<ComConectionType>()
                                        .Where(val => (possibleCos & val) == val)
                                        .Select(v => v.ToString())
                                        .ToList();
        dataTypeDropdown.AddOptions(possibleCoTypes);

        if(chosenProt == ComProtocol.TCPIP_Receiver || chosenProt == ComProtocol.UDP_Receiver) {
            // handle options of the dropdown here
            dataToSendReceive_MSDrop.enabled = false;
            dataToSendReceive.text = "Data fields to receive";
        }
        else {
            // handle options of the dropdown here
            dataToSendReceive_MSDrop.enabled = true;
            dataToSendReceive.text = "Data fields to send";
        }

        OnDataType_ValueChanged();
    }
    void OnDataType_ValueChanged()
    {
        ComConectionType chosenCoType = ComOps.Str_2_ComConnectionType(dataTypeDropdown.options[dataTypeDropdown.value].text);
        if(chosenCoType.Equals(ComConectionType.dataVisualization))
            panelTitle.text = "Data Visualization";
        else if(chosenCoType.Equals(ComConectionType.simulationEnv))
            panelTitle.text = "Sim Environment";
        else if(chosenCoType.Equals(ComConectionType.shipControlOrders))
            panelTitle.text = "Ship Control Orders";

        OnDataToSend_ValueChanged();
    }

    void OnDataToSend_ValueChanged()
    {
        List<stringBoolStruct> opts = new List<stringBoolStruct>();
        opts.Add(new stringBoolStruct("Option 1",false));
        opts.Add(new stringBoolStruct("Option 2",true));
        opts.Add(new stringBoolStruct("Option 3",true));
        opts.Add(new stringBoolStruct("Option 4",false));
        dataToSendReceive_MSDrop.SetOptions(opts);
    }

    void OnDefaultBtn_Clicked()
    {
        Debug.Log("Clicked on the default button !");
    }
    void OnTestBtn_Clicked()
    {
        Debug.Log("Clicked on the test button !");
    }
    void OnRemovePanelBtn_Clicked()
    {
        GameObject.Destroy(gameObject);
    }

    public PanelSavingStruct Get_PanelSavingStruct()
    {
        bool comIsEnabled = mainToggleSwitch.isOn;
        string prot = protDropdown.options[protDropdown.value].text;
        string dataType = dataTypeDropdown.options[dataTypeDropdown.value].text;
        List<stringBoolStruct> dataFields = dataToSendReceive_MSDrop.GetValues();
        string dataFieldsStr = "";
        foreach(stringBoolStruct item in dataFields) {
            if(item.optionIsSelected) {
                dataFieldsStr += item.optionString + ",";
            }
        }
        dataFieldsStr = dataFieldsStr.Substring(0, dataFieldsStr.Length-1);
        string remoteIP = ipField.text;
        int port = int.Parse(portField.text);
        return new PanelSavingStruct(comIsEnabled, prot, dataType, dataFieldsStr, remoteIP, port);
    }
    [Serializable]
    public struct PanelSavingStruct {
        public bool comIsEnabled;
        public string protocol;
        public string dataType;
        public string dataFields;
        public string remoteIP;
        public int port;

        public PanelSavingStruct(bool _comIsEnabled, string _protocol, string _dataType, string _dataFields, string _ip, int _port) {
            comIsEnabled = _comIsEnabled;
            protocol = _protocol;
            dataType = _dataType;
            dataFields = _dataFields;
            remoteIP = _ip;
            port = _port;
        }
    }
}
