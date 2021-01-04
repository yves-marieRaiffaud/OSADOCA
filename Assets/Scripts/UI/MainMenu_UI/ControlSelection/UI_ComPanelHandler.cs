using TMPro;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

using Communication;
using MSDropdownNamespace;
using ComOps = CommonMethods.CommunicationOps;
using ComChannelGeneric = Communication.ComChannelInterface;

public class UI_ComPanelHandler : MonoBehaviour
{
    internal ComPanelIsRemoved removedPanelEvent;
    bool isAlreadyAwaken=false;
    internal UnityEvent hasBeenModified;

    // Persistent object to handle coms in every scenes (positioned in the DontDestroyOnLoad folder)
    ComsOverallHandler comsHandler;

    internal int channelIdx; // Index of the object linked ComChannel<T> to this UI panel from the 'List<ComChannel> channels' from 'comsHandler'  

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
    //=======================
    //=======================
    internal void Start()
    {
        if(!isAlreadyAwaken) {
            if(hasBeenModified == null)
                hasBeenModified = new UnityEvent();

            if(removedPanelEvent == null)
                removedPanelEvent = new ComPanelIsRemoved();

            Assign_Variables();
            Init_Variables();

            isAlreadyAwaken = true;
        }
    }
    void Assign_Variables()
    {
        comsHandler = GameObject.Find("ComsHandler").GetComponent<ComsOverallHandler>();

        disabledPanel = transform.Find("Disabled").GetComponent<RectTransform>();
        enabledPanel = transform.Find("Enabled").GetComponent<RectTransform>();

        disabledPanelTxt = transform.Find("Disabled/DisabledChannel_txt").GetComponent<TMP_Text>();
        removePanelBtn = transform.Find("Disabled/RemovePanel_btn").GetComponent<Button>();
        removePanelBtn.onClick.AddListener(delegate {
            OnRemovePanelBtn_Clicked();
            });

        panelTitle = transform.Find("Title_Toggle_Panel/Panel_Title").GetComponent<TMP_Text>();
        mainToggleSwitch = transform.Find("Title_Toggle_Panel/Panel_OverallToggle/Toggle").GetComponent<ToggleSwitch>();
        mainToggleSwitch.Awake(); // Calling the Awake method as the object is not active --> won't initialize its onX & offX correctly otherwise
        mainToggleSwitch.onValueChanged.AddListener(delegate {
            OnToggleSwitch_Clicked();
            ModifiedPanel_Event();
            });

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
            OnDefaultBtn_Clicked(comsHandler.channels[channelIdx], ipField, portField);
            ModifiedPanel_Event();
            });

        testBtn = transform.Find("Enabled/Btns_Panel/Test_btn").GetComponent<Button>();
        testBtn.onClick.AddListener(delegate {
            OnTestConnectionClick(comsHandler.channels[channelIdx]);
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
        if(comsHandler.channels != null && comsHandler.channels.Count > 0)
            comsHandler.channels[channelIdx].channelObj_generic.isActive = mainToggleSwitch.isOn;
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
        else if(chosenCoType.Equals(ComConectionType.None))
            panelTitle.text = "None";

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

    void OnRemovePanelBtn_Clicked()
    {
        GameObject.DestroyImmediate(gameObject);
        removedPanelEvent.Invoke(channelIdx);
    }

    internal void Create_ComObj()
    {
        string ip = ipField.text;
        int port = int.Parse(portField.text);
        ComProtocol prot = ComOps.Str_2_ComProtocol(protDropdown.options[protDropdown.value].text);
        ComConectionType coType = ComOps.Str_2_ComConnectionType(dataTypeDropdown.options[dataTypeDropdown.value].text);
        //----------------------------------------------------------
        ComDataFields dataFields= ComDataFields.None; // TO MODIFIY
        //----------------------------------------------------------
        ComSendReceiveType sendReceiveType;
        if(prot.Equals(ComProtocol.UDP_Receiver) || prot.Equals(ComProtocol.UDP_Sender))
            sendReceiveType = ComSendReceiveType.classExplicit;
        else {
            // Is TCPIP_Sender or TCPIP_Receiver
            if(coType.Equals(ComConectionType.shipControlOrders))
                sendReceiveType = ComSendReceiveType.receiveOnly;
            else if(coType.Equals(ComConectionType.simulationEnv))
                sendReceiveType = ComSendReceiveType.sendOnly;
            else
                sendReceiveType = ComSendReceiveType.sendOnly; // DataVizualisation
        }

        ComChannelParams channelParams = new ComChannelParams(ip, port, prot, coType, dataFields, sendReceiveType, 5012);
        channelIdx = comsHandler.Add_ComObject(channelParams);
        comsHandler.channels[channelIdx].channelObj_generic.isActive = mainToggleSwitch.isOn;
    }
    //=======================
    //=======================
    // CONNECTIONS HANDLING
    void OnDefaultBtn_Clicked(ComChannelGeneric channel, TMP_InputField fieldIP, TMP_InputField fieldPort)
    {
        fieldIP.text = channel.defaultIP;
        fieldPort.text = channel.defaultPort.ToString();
        UpdateConnectionChannel(channel, fieldIP, fieldPort);
    }
    void UpdateConnectionChannel(ComChannelInterface channel, TMP_InputField ipField, TMP_InputField portField)
    {
        channel.IP = ipField.text;
        if(!ComOps.IP_AddressIsValid(channel.IP))
            return;
        int parsedPort;
        if(int.TryParse(portField.text, out parsedPort)) {
            channel.port = int.Parse(portField.text);
            channel.InitChannelObj();
        }
    }
    void OnTestConnectionClick(ComChannelGeneric channelToTest)
    {
        Debug.Log("channelIdx = " + channelIdx);
        UpdateConnectionChannel(channelToTest, ipField, portField);
        Debug.Log("channelToTest.comParams.protocol = " + channelToTest.comParams.protocol);
        if(channelToTest.comParams.protocol.Equals(ComProtocol.UDP_Sender)) {
            UDPSender sender = channelToTest.channelObj_generic as UDPSender;
            if(sender == null)
                Debug.LogWarning("UDPSender 'sender' is null. Can not send 'TEST' message to target.");
            sender.Send("TEST");
            Debug.Log("A test message has been sent using UDP at IP: " + sender.IP + " and port: " + sender.port + ". Channel open status is " + sender.channelIsOpen);
        }
        else if(channelToTest.comParams.protocol.Equals(ComProtocol.UDP_Receiver)) {
            UDPReceiver receiver = channelToTest.channelObj_generic as UDPReceiver;
        }
        else if(channelToTest.comParams.protocol.Equals(ComProtocol.TCPIP_Sender)) {
            TCPServer server = channelToTest.channelObj_generic as TCPServer;
            server.Send("TEST");
            Debug.Log("A test message has been sent using TCP/IP at IP: " + server.IP + " and port: " + server.port);
        }
    }
    //=======================
    //=======================
    internal static bool ComChannelInterface_Is_Outgoing_SimEnv(ComChannelInterface comObj)
    {
        if(comObj.comParams.connectionType.Equals(ComConectionType.simulationEnv) && comObj.channelObj_generic.isActive)
            return true;
        return false;
    }
    internal static bool ComChannelInterface_Is_Incoming_ShipOrders(ComChannelInterface comObj)
    {
        if(comObj.comParams.connectionType.Equals(ComConectionType.shipControlOrders) && comObj.channelObj_generic.isActive)
            return true;
        return false;
    }
    //=======================
    //=======================
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
