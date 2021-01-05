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
    public TMP_Dropdown shipControlModeDropdown;

    [Header("Com Panel Prefab")]
    public GameObject comPanelPrefab;
    [Header("Add Panel Prefab")]
    public GameObject addPanelPrefab;

    public MainPanelIsSetUp panelIsFullySetUp;

    GameObject addPanel_GO;
    ComsOverallHandler comsHandler;

    void Start()
    {
        if(panelIsFullySetUp == null)
            panelIsFullySetUp = new MainPanelIsSetUp();

        comsHandler = GameObject.Find("ComsHandler").GetComponent<ComsOverallHandler>();

        shipControlModeDropdown.onValueChanged.AddListener(delegate{
            ToggleControlTypePanels(shipControlModeDropdown.value);
            SaveSimulationEnvParams();
        });
        InitControlModeDropdownFromSavedParams();

        Load_ComsPanels_FromDisk(); // Read the saved JSON containing the comPanels parameters, else spawning the default panels
        Add_AddPanelToScrollView(); // Adding an 'AddButton' panel at the end of the scroll view to be able to add new comPanel
        //==================
        //==================
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
        // In order to return true to the remote connection panel:
        // One of the connection must be  an outgoing connection sending the simulationEnv (either UDP_Sender or TCP_Sender)
        // One of the connection must be an  incoming connection receiving the shipControlOrders (either UDP_Receiver or TCP_Receiver)
        bool simEnv_OK = false;
        bool shipOrders_OK = false;
        foreach(ComChannelInterface comItem in comsHandler.channels) {
            if(UI_ComPanelHandler.ComChannelInterface_Is_Incoming_ShipOrders(comItem))
                shipOrders_OK = true;
            if(UI_ComPanelHandler.ComChannelInterface_Is_Outgoing_SimEnv(comItem))
                simEnv_OK = true;
        }
        if(simEnv_OK && shipOrders_OK)
            return true;
        else
            return false;
    }
    //=======================
    //=======================
    // SIMULATION ENV SAVING
    void SaveSimulationEnvParams()
    {
        // Get fresh simulationEnv values, in case some params were modified when switching between tabs
        SimulationEnv simulationEnv = SimulationEnv.GetSimEnvObjectFrom_ReadUserParams();
        // From the retrieved instance, only change the params that were modified in this tab
        simulationEnv.shipUseKeyboardControl.value = Get_UseKeyboard_SimSettingValue_FromDropdown();
        string filepath = SimulationEnv.WriteToFileSimuSettingsSaveData(simulationEnv);
        Debug.Log("SimSettings has succesfuly been saved at : '" + filepath + "'.");
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
    void InitControlModeDropdownFromSavedParams()
    {
        SimulationEnv simulationEnv = SimulationEnv.GetSimEnvObjectFrom_ReadUserParams();
        bool useKeyboardParamvalue = simulationEnv.shipUseKeyboardControl.value;
        shipControlModeDropdown.value = Convert.ToInt32(!useKeyboardParamvalue);
    }
    //=======================
    //=======================
    // UI HANDLING
    public void Save_ComsPanels_2_Disk()
    {
        List<PanelSavingStruct> dataToSave = new List<PanelSavingStruct>();
        foreach(Transform child in scrollRectPanel_TR)//for(int idx=0; idx<scrollRectPanel_TR.childCount; idx++)
        {
            UI_ComPanelHandler comPanel;
            bool res = child.TryGetComponent<UI_ComPanelHandler>(out comPanel);
            //bool res = scrollRectPanel_TR.GetChild(idx).TryGetComponent<UI_ComPanelHandler>(out comPanel);
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
        // removing every comPanel, keeping only the 'Add' panel button
        foreach(Transform child in scrollRectPanel_TR)
        {
            UI_ComPanelHandler comPanel;
            bool res = child.TryGetComponent<UI_ComPanelHandler>(out comPanel);
            if(res)
                GameObject.Destroy(child.gameObject);
        }
    }

    UI_ComPanelHandler AddComPanelInstance(bool toggleSwitch, ComProtocol protocol, string ip, int port, ComConectionType comDataType=ComConectionType.None, Enum dataFields=null)
    {
        GameObject itemGO = GameObject.Instantiate(comPanelPrefab, Vector3.zero, Quaternion.identity, scrollRectPanel_TR);
        itemGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f,0f);
        UI_ComPanelHandler itemComOb = itemGO.GetComponent<UI_ComPanelHandler>();
        itemComOb.Start();
        itemComOb.mainToggleSwitch.isOn = toggleSwitch;

        itemComOb.protDropdown.value = itemComOb.protDropdown.options.FindIndex(option => option.text == protocol.ToString());
        itemComOb.protDropdown.RefreshShownValue(); // Will call the action related to the protDropdown onValueChanged
        itemComOb.dataTypeDropdown.value = itemComOb.dataTypeDropdown.options.FindIndex(option => option.text == comDataType.ToString());
        itemComOb.dataTypeDropdown.RefreshShownValue(); // Will call the action related to the protDropdown onValueChanged
        itemComOb.ipField.text = ip;
        itemComOb.portField.text = port.ToString();

        itemComOb.Create_ComObj();

        itemComOb.hasBeenModified.AddListener(Save_ComsPanels_2_Disk);
        itemComOb.removedPanelEvent.AddListener(OnPanelRemoved);
        return itemComOb;
    }

    void Add_AddPanelToScrollView()
    {
        // Adding the AddPanel prefab to the scrollView, after having loaded the saved comParams
        addPanel_GO = GameObject.Instantiate(addPanelPrefab, Vector3.zero, Quaternion.identity, scrollRectPanel_TR);
        addPanel_GO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f,0f);
        addPanel_GO.transform.Find("btn").GetComponent<Button>().onClick.AddListener(delegate {
            AddNewComPanel();
            Save_ComsPanels_2_Disk();
        });
    }
    void AddNewComPanel()
    {
        // Adding a new comPanel because the addPanel button has been clicked, and moving AddPanel button to the end of the scrollView
        UI_ComPanelHandler newPanel = AddComPanelInstance(false, ComProtocol.UDP_Sender, "127.0.0.1", 5012, ComConectionType.dataVisualization);
        addPanel_GO.transform.SetAsLastSibling(); // Make sure the addPanel is the last panel in the group layout canvas
        Check_AddPanel_MaxNBPanels();
    }
    void Check_AddPanel_MaxNBPanels()
    {
        if(comsHandler.channels.Count == ComsOverallHandler.maxNBConnections-1)
            addPanel_GO.SetActive(!addPanel_GO.activeSelf);
    }
    void OnPanelRemoved(int removedIndex)
    {
        Check_AddPanel_MaxNBPanels();
        comsHandler.channels.RemoveAt(removedIndex);

        // When a panel is removed from the UI Group Layout, the removed panel sends its channelIdx. This method thus reassigns the indexes for the comsHandler.channels of the other panels
        foreach(Transform child in scrollRectPanel_TR)
        {
            UI_ComPanelHandler comPanel;
            bool res = child.TryGetComponent<UI_ComPanelHandler>(out comPanel);
            if(res) {
                if(comPanel.channelIdx > removedIndex)
                    comPanel.channelIdx -= 1;
            }
        }
        Save_ComsPanels_2_Disk(); // Saving changes
    }
}