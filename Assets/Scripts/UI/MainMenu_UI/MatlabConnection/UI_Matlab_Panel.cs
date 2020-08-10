using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using Fncs = UsefulFunctions;
using Matlab_Communication;
using System.Text.RegularExpressions;

public class UI_Matlab_Panel : MonoBehaviour
{
    // This script is attached to the 'MainPanel' GameObject
    //=============================================================================================
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
    //==========
    void Start()
    {
        ipDataVisuInputField.onValueChanged.AddListener(delegate{
            UpdateConnectionChannel(comHandler.dataVisSenderChannel, ipDataVisuInputField, portDataVisuInputField);
        });
        portDataVisuInputField.onValueChanged.AddListener(delegate{
            UpdateConnectionChannel(comHandler.dataVisSenderChannel, ipDataVisuInputField, portDataVisuInputField);
        });
        //===========
        //===========
        ipSimEnvInputField.onValueChanged.AddListener(delegate{
            UpdateConnectionChannel(comHandler.simEnvSenderChannel, ipSimEnvInputField, portSimEnvInputField);
        });
        portSimEnvInputField.onValueChanged.AddListener(delegate{
            UpdateConnectionChannel(comHandler.simEnvSenderChannel, ipSimEnvInputField, portSimEnvInputField);
        });
        //===========
        //===========
        ipControlAlgoInputField.onValueChanged.AddListener(delegate{
            UpdateConnectionChannel(comHandler.controlAlgoReceiverChannel, ipControlAlgoInputField, portControlAlgoInputField);
        });
        portControlAlgoInputField.onValueChanged.AddListener(delegate{
            UpdateConnectionChannel(comHandler.controlAlgoReceiverChannel, ipControlAlgoInputField, portControlAlgoInputField);
        });
        //===========
        //===========
        dataVisuTest_btn.onClick.AddListener(delegate{OnTestConnectionClick(comHandler.dataVisSenderChannel);});
        simEnvTest_btn.onClick.AddListener(delegate{OnTestConnectionClick(comHandler.simEnvSenderChannel);});
        controlAlgoTest_btn.onClick.AddListener(delegate{OnTestConnectionClick(comHandler.controlAlgoReceiverChannel);});
    }

    private void UpdateConnectionChannel<T>(MatlabComChannel<T> channel, TMPro.TMP_InputField ipField, TMPro.TMP_InputField portField)
    where T : SenderReceiverBaseChannel
    {
        channel.ip_address = ipField.text;
        if(!IP_AddressIsValid(channel.ip_address))
            return;
        int parsedPort;
        if(int.TryParse(portField.text, out parsedPort))
        {
            channel.port = int.Parse(portField.text);
            channel.InitChannelObj();
        }
    }

    private bool IP_AddressIsValid(string IP_toCheck)
    {
        string ipv4_REGEX = "(([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])";
        return Regex.IsMatch(IP_toCheck, ipv4_REGEX);
    }

    private void OnTestConnectionClick<T>(MatlabComChannel<T> channelToTest)
    where T : SenderReceiverBaseChannel
    {
        // Do something here
        Debug.Log(channelToTest.connectionType);
    }
}