using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ComOps = CommonMethods.CommunicationOps;
using Communication;
using PanelSavingStruct=UI_ComPanelHandler.PanelSavingStruct;
using TMPro;
using System;
using System.IO;

public class UI_ControlSelection_Panel : MonoBehaviour
{
    // This script is attached to the 'MainPanel' GameObject
    //=============================================================================================
    [Tooltip("RectTransform of the 'ControlAlgo_Panel' that contains the every com panels")]
    public RectTransform controlAlgoPanel_TR;
    public RectTransform scrollRectPanel_TR;
    [Header("Spaceship Control Panels")]
    public GameObject remoteConnectionControlPanel;
    public GameObject keyboardControlPanel;

    [Header("Control Mode Dropdown")]
    public TMPro.TMP_Dropdown shipControlModeDropdown;

    [Header("Com Panel Prefab")]
    public GameObject comPanelPrefab;
    [Header("Add Panel Prefab")]
    public GameObject addPanelPrefab;

    public MainPanelIsSetUp panelIsFullySetUp;

    GameObject addPanel_GO;


    void Start()
    {
        if(panelIsFullySetUp == null)
            panelIsFullySetUp = new MainPanelIsSetUp();

        shipControlModeDropdown.onValueChanged.AddListener(delegate{
            ToggleControlTypePanels(shipControlModeDropdown.value);
            SaveSimulationEnvParams();
        });
        InitControlModeDropdownFromSavedParams();

        Load_ComsPanels_FromDisk();
        Add_AddPanelToScrollView();

        //Save_ComsPanels_2_Disk();
        
        //=======================
        //=======================
        /*ipDataVisuInputField.onValueChanged.AddListener(delegate{
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
        */
        //simEnvRevert_btn.onClick.AddListener(delegate{OnRevertBtnClick(comHandler.simEnvSenderChannel, ipSimEnvInputField, portSimEnvInputField);});
        //controlAlgoRevert_btn.onClick.AddListener(delegate{OnRevertBtnClick(comHandler.controlAlgoReceiverChannel, ipControlAlgoInputField, portControlAlgoInputField);});
        //=======================
        //=======================
        //ToggleControlTypePanels(1); // Init at remote connections
        shipControlModeDropdown.value = 1;
        CheckPanels_SetUp();
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
        CheckPanels_SetUp();
    }
    bool CheckPanels_SetUp()
    {
        if(keyboardControlPanel.activeSelf && !remoteConnectionControlPanel.activeSelf) {
            if(Check_Keyboard_Control_SetUp()) {
                panelIsFullySetUp.Invoke(2, 1);
                return true;
            }
            else {
                panelIsFullySetUp.Invoke(2, 0);
                return false;
            }
        }
        else if(!keyboardControlPanel.activeSelf && remoteConnectionControlPanel.activeSelf) {
            if(Check_ControlAlgo_Control_SetUp()) {
                panelIsFullySetUp.Invoke(2, 1);
                return true;
            }
            else {
                panelIsFullySetUp.Invoke(2, 0);
                return false;
            }
        }
        else
            return false;
    }
    void SaveSimulationEnvParams()
    {
        // Get fresh simulationEnv values, in case some params were modified when switching between tabs
        /*SimulationEnv simulationEnv = UseFncs.GetSimEnvObjectFrom_ReadUserParams();
        // From the retrieved instance, only change the params that were modified in this tab
        simulationEnv.shipUseKeyboardControl.value = Get_UseKeyboard_SimSettingValue_FromDropdown();
        string filepath = UsefulFunctions.WriteToFileSimuSettingsSaveData(simulationEnv);
        Debug.Log("SimSettings has succesfuly been saved at : '" + filepath + "'."); */
    }

    

    public bool SendControlBarTriangleUpdate()
    {
        return CheckPanels_SetUp();
    }

    bool Check_Keyboard_Control_SetUp()
    {
        return true;
    }

    bool Check_ControlAlgo_Control_SetUp()
    {
        return false;
    }

    

    void InitControlModeDropdownFromSavedParams()
    {
        /*SimulationEnv simulationEnv = UseFncs.GetSimEnvObjectFrom_ReadUserParams();
        bool useKeyboardParamvalue = simulationEnv.shipUseKeyboardControl.value;
        shipControlModeDropdown.value = Convert.ToInt32(!useKeyboardParamvalue);*/
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

    //=======================
    private void UpdateConnectionChannel<T>(ComChannel<T> channel, TMPro.TMP_InputField ipField, TMPro.TMP_InputField portField)
    where T : SenderReceiverBaseChannel
    {
        channel.IP = ipField.text;
        if(!ComOps.IP_AddressIsValid(channel.IP))
            return;
        int parsedPort;
        if(int.TryParse(portField.text, out parsedPort))
        {
            channel.port = int.Parse(portField.text);
            channel.InitChannelObj();
        }
    }

    private void OnRevertBtnClick<T>(ComChannel<T> channel, TMPro.TMP_InputField fieldIP, TMPro.TMP_InputField fieldPort)
    where T : SenderReceiverBaseChannel
    {
        fieldIP.text = channel.defaultIP;
        fieldPort.text = channel.defaultPort.ToString();
        UpdateConnectionChannel(channel, fieldIP, fieldPort);
    }

    private void OnTestConnectionClick<T>(ComChannel<T> channelToTest)
    where T : SenderReceiverBaseChannel
    {
        if(typeof(T) == typeof(UDPSender)) {
            UDPSender sender = channelToTest.channelObj as UDPSender;
            if(sender == null)
                Debug.LogWarning("UDPSender 'sender' is null. Can not send 'TEST' message to target.");
            sender.Send("TEST");
            Debug.Log("A test message has been sent using UDP at IP: " + sender.IP + " and port: " + sender.port + ". Channel open status is " + sender.channelIsOpen);
        }
        else if(typeof(T) == typeof(UDPReceiver)) {
            UDPReceiver receiver = channelToTest.channelObj as UDPReceiver;
        }
        else if(typeof(T) == typeof(TCPServer)) {
            TCPServer server = channelToTest.channelObj as TCPServer;
            server.Send("TEST");
            Debug.Log("A test message has been sent using TCP/IP at IP: " + server.IP + " and port: " + server.port);
        }
    }


    public void Save_ComsPanels_2_Disk()
    {
        List<PanelSavingStruct> dataToSave = new List<PanelSavingStruct>();
        for(int idx=0; idx<scrollRectPanel_TR.childCount; idx++)
        {
            UI_ComPanelHandler comPanel;
            bool res = scrollRectPanel_TR.GetChild(idx).TryGetComponent<UI_ComPanelHandler>(out comPanel);
            if(res)
                dataToSave.Add(comPanel.Get_PanelSavingStruct());
        }
        string filepath = Application.persistentDataPath + Filepaths.comsPanel_params;
        File.WriteAllText(filepath, JsonHelper.ToJson<PanelSavingStruct>(dataToSave.ToArray(), true));
        Debug.LogFormat("Successfully saved the Coms panels params at '{0}'", filepath);
    }
    void Load_ComsPanels_FromDisk()
    {
        Remove_Every_ComPanels();
        string filepath = Application.persistentDataPath + Filepaths.comsPanel_params;
        if(!File.Exists(filepath)) {
            Create_Default_ComPanels();
            return;
        }

        PanelSavingStruct[] panelsParams = JsonHelper.FromJson<PanelSavingStruct>(File.ReadAllText(filepath));
        foreach(PanelSavingStruct item in panelsParams) {
            ComProtocol prot = ComOps.Str_2_ComProtocol(item.protocol);
            ComConectionType comDataType = ComOps.Str_2_ComConnectionType(item.dataType);
            UI_ComPanelHandler addedPanel = AddComPanelInstance(item.comIsEnabled, prot, item.remoteIP, item.port, comDataType);
        }
    }
    void Create_Default_ComPanels()
    {
        float prefabHeight = comPanelPrefab.GetComponent<RectTransform>().sizeDelta.y;
        // Creating default panels
        UI_ComPanelHandler dataVizPanel = AddComPanelInstance(false, ComProtocol.UDP_Sender, "127.0.0.1", 5012, ComConectionType.dataVisualization);
        UI_ComPanelHandler simEnvPanel = AddComPanelInstance(true, ComProtocol.TCPIP_Sender, "127.0.0.1", 5013, ComConectionType.simulationEnv);
        UI_ComPanelHandler shipOrdersPanel = AddComPanelInstance(true, ComProtocol.TCPIP_Receiver, "127.0.0.1", 5014, ComConectionType.shipControlOrders);
    }
    void Remove_Every_ComPanels()
    {
        for(int idx=0; idx<scrollRectPanel_TR.childCount; idx++) {
            GameObject.Destroy(scrollRectPanel_TR.GetChild(idx).gameObject);
        }
    }

    UI_ComPanelHandler AddComPanelInstance(bool toggleSwitch, ComProtocol protocol, string ip, int port, ComConectionType comDataType=ComConectionType.None, ComDataFields dataFields=ComDataFields.None)
    {
        GameObject itemGO = GameObject.Instantiate(comPanelPrefab, Vector3.zero, Quaternion.identity, scrollRectPanel_TR);
        itemGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f,0f);
        UI_ComPanelHandler itemComOb = itemGO.GetComponent<UI_ComPanelHandler>();
        itemComOb.Start();
        itemComOb.mainToggleSwitch.Toggle(toggleSwitch);
        //itemComOb.mainToggleSwitch.isOn = toggleSwitch;
        itemComOb.protDropdown.value = itemComOb.protDropdown.options.FindIndex(option => option.text == protocol.ToString());
        itemComOb.protDropdown.RefreshShownValue(); // Will call the action related to the protDropdown onValueChanged
        itemComOb.dataTypeDropdown.value = itemComOb.dataTypeDropdown.options.FindIndex(option => option.text == comDataType.ToString());
        itemComOb.dataTypeDropdown.RefreshShownValue(); // Will call the action related to the protDropdown onValueChanged
        itemComOb.ipField.text = ip;
        itemComOb.portField.text = port.ToString();
        itemComOb.hasBeenModified.AddListener(Save_ComsPanels_2_Disk);
        return itemComOb;
    }

    void Add_AddPanelToScrollView()
    {
        // Adding the AddPanel prefab to the scrollView, after having loaded the saved comParams
        addPanel_GO = GameObject.Instantiate(addPanelPrefab, Vector3.zero, Quaternion.identity, scrollRectPanel_TR);
        addPanel_GO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f,0f);
        addPanel_GO.transform.Find("btn").GetComponent<Button>().onClick.AddListener(AddNewComPanel);

    }
    void AddNewComPanel()
    {
        // Adding a new comPanel because the addPanel button has been clicked, and moving AddPanel button to the end of the scrollView
        UI_ComPanelHandler newPanel = AddComPanelInstance(false, ComProtocol.UDP_Sender, "127.0.0.1", 5012, ComConectionType.dataVisualization);
        addPanel_GO.transform.SetAsLastSibling(); // Make sure the addPanel is the last panel in the group layout canvas
    }



}