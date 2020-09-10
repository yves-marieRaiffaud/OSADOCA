using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using UseFncs = UsefulFunctions;
using Matlab_Communication;
using System;

public class UI_Matlab_Panel : MonoBehaviour
{
    // This script is attached to the 'MainPanel' GameObject
    //=============================================================================================
    [Header("Spaceship Control Panels")]
    public GameObject remoteConnectionControlPanel;
    public GameObject keyboardControlPanel;

    [Header("Control Mode Dropdown")]
    public TMPro.TMP_Dropdown shipControlModeDropdown;
    //=======================
    //=======================
    [Header("Matlab Communication Handler Script")]
    [Tooltip("MatlabComHandler script instance located in the 'MatlabCom_Handler' GameObject")]
    public MatlabComHandler comHandler;

    [Header("Data Visualization Sender Fields")]
    public TMPro.TMP_InputField ipDataVisuInputField;
    public TMPro.TMP_InputField portDataVisuInputField;
    public Button dataVisuRevert_btn;
    public Button dataVisuTest_btn;

    [Header("Simulation Env Data Sender Fields")]
    public TMPro.TMP_InputField ipSimEnvInputField;
    public TMPro.TMP_InputField portSimEnvInputField;
    public Button simEnvRevert_btn;
    public Button simEnvTest_btn;

    [Header("Matlab Control Algorithm Receiver Fields")]
    public TMPro.TMP_InputField ipControlAlgoInputField;
    public TMPro.TMP_InputField portControlAlgoInputField;
    public Button controlAlgoRevert_btn;
    public Button controlAlgoTest_btn;
    //=======================
    //=======================

    void OnEnable()
    {
        
    }

    void OnDisable()
    {   
        
    }
    //==========
    void Start()
    {
        shipControlModeDropdown.onValueChanged.AddListener(delegate{
            ToggleControlTypePanels(shipControlModeDropdown.value);
            SaveSimulationEnvParams();
        });
        InitControlModeDropdownFromSavedParams();

        //=======================
        //=======================
        ipDataVisuInputField.onValueChanged.AddListener(delegate{
            UpdateConnectionChannel(comHandler.dataVisSenderChannel, ipDataVisuInputField, portDataVisuInputField);
        });
        portDataVisuInputField.onValueChanged.AddListener(delegate{
            UpdateConnectionChannel(comHandler.dataVisSenderChannel, ipDataVisuInputField, portDataVisuInputField);
        });
        //===========
        //===========
        dataVisuTest_btn.onClick.AddListener(delegate{OnTestConnectionClick(comHandler.dataVisSenderChannel);});
        simEnvTest_btn.onClick.AddListener(delegate{OnTestConnectionClick(comHandler.simEnvTCPServer);});
        controlAlgoTest_btn.onClick.AddListener(delegate{OnTestConnectionClick(comHandler.controlAlgoTCPServer);});

        dataVisuRevert_btn.onClick.AddListener(delegate{OnRevertBtnClick(comHandler.dataVisSenderChannel, ipDataVisuInputField, portDataVisuInputField);});
        //simEnvRevert_btn.onClick.AddListener(delegate{OnRevertBtnClick(comHandler.simEnvSenderChannel, ipSimEnvInputField, portSimEnvInputField);});
        //controlAlgoRevert_btn.onClick.AddListener(delegate{OnRevertBtnClick(comHandler.controlAlgoReceiverChannel, ipControlAlgoInputField, portControlAlgoInputField);});
    }

    void ToggleControlTypePanels(int selectedDropdownIdx)
    {
        switch(selectedDropdownIdx)
        {
            case 0:
                // The 'Use Keyboard' option is selected
                if(!keyboardControlPanel.activeSelf)
                    keyboardControlPanel.SetActive(true);
                if(remoteConnectionControlPanel.activeSelf)
                    remoteConnectionControlPanel.SetActive(false);
                break;

            case 1:
                // The 'Use remote connections to send/receive commands' option is selected
                if(!remoteConnectionControlPanel.activeSelf)
                    remoteConnectionControlPanel.SetActive(true);
                if(keyboardControlPanel.activeSelf)
                    keyboardControlPanel.SetActive(false);
                break;
        }
    }

    void InitControlModeDropdownFromSavedParams()
    {
        SimulationEnv simulationEnv = UseFncs.GetSimEnvObjectFrom_ReadUserParams();
        bool useKeyboardParamvalue = simulationEnv.shipUseKeyboardControl.value;
        shipControlModeDropdown.value = Convert.ToInt32(!useKeyboardParamvalue);
    }

    bool Get_UseKeyboard_SimSettingValue_FromDropdown()
    {
        bool useKeyboardControlBool;
        switch(shipControlModeDropdown.value) {
            case 0:
                // The 'Use Keyboard' option is selected
                useKeyboardControlBool= true;
                break;
            case 1:
                // The 'Use remote connections to send/receive commands' option is selected
                useKeyboardControlBool= false;
                break;
            default:
                useKeyboardControlBool = true;
                break;
        }
        return useKeyboardControlBool;
    }

    void SaveSimulationEnvParams()
    {
        // Get fresh simulationEnv values, in case some params were modified when switching between tabs
        SimulationEnv simulationEnv = UseFncs.GetSimEnvObjectFrom_ReadUserParams();
        // From the retrieved instance, only change the params that were modified in this tab
        simulationEnv.shipUseKeyboardControl.value = Get_UseKeyboard_SimSettingValue_FromDropdown();
        string filepath = UsefulFunctions.WriteToFileSimuSettingsSaveData(simulationEnv);
        Debug.Log("SimSettings has succesfuly been saved at : '" + filepath + "'."); 
    }
    //=======================
    //=======================
    //=======================
    //=======================
    private void UpdateConnectionChannel<T>(MatlabComChannel<T> channel, TMPro.TMP_InputField ipField, TMPro.TMP_InputField portField)
    where T : SenderReceiverBaseChannel
    {
        channel.IP = ipField.text;
        if(!UseFncs.IP_AddressIsValid(channel.IP))
            return;
        int parsedPort;
        if(int.TryParse(portField.text, out parsedPort))
        {
            channel.port = int.Parse(portField.text);
            channel.InitChannelObj();
        }
    }

    private void OnRevertBtnClick<T>(MatlabComChannel<T> channel, TMPro.TMP_InputField fieldIP, TMPro.TMP_InputField fieldPort)
    where T : SenderReceiverBaseChannel
    {
        fieldIP.text = channel.defaultIP;
        fieldPort.text = channel.defaultPort.ToString();
        UpdateConnectionChannel(channel, fieldIP, fieldPort);
    }

    private void OnTestConnectionClick<T>(MatlabComChannel<T> channelToTest)
    where T : SenderReceiverBaseChannel
    {
        if(typeof(T) == typeof(UDPSender))
        {
            UDPSender sender = channelToTest.channelObj as UDPSender;
            if(sender == null)
                Debug.LogWarning("UDPSender 'sender' is null. Can not send 'TEST' message to target.");
            sender.Send("TEST");
            Debug.Log("A test message has been sent using UDP at IP: " + sender.IP + " and port: " + sender.port + ". Channel open status is " + sender.channelIsOpen);
        }
        else if(typeof(T) == typeof(UDPReceiver))
        {
            UDPReceiver receiver = channelToTest.channelObj as UDPReceiver;
        }
        else if(typeof(T) == typeof(TCPServer))
        {
            TCPServer server = channelToTest.channelObj as TCPServer;
            server.Send("TEST");
            Debug.Log("A test message has been sent using TCP/IP at IP: " + server.IP + " and port: " + server.port);
        }
    }
}