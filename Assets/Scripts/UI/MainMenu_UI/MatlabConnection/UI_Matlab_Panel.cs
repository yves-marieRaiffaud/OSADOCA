using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using Fncs = UsefulFunctions;
using Matlab_Communication;

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
        if(!Fncs.IP_AddressIsValid(channel.ip_address))
            return;
        int parsedPort;
        if(int.TryParse(portField.text, out parsedPort))
        {
            channel.port = int.Parse(portField.text);
            channel.InitChannelObj();
        }
    }

    private void OnTestConnectionClick<T>(MatlabComChannel<T> channelToTest)
    where T : SenderReceiverBaseChannel
    {
        if(typeof(T) == typeof(UDPSender))
        {
            UDPSender sender = channelToTest.channelObj as UDPSender;
            sender.Send("TEST");
        }
        else if(typeof(T) == typeof(UDPReceiver))
        {
            UDPReceiver sender = channelToTest.channelObj as UDPReceiver;
            //sender.
        }
        else if(typeof(T) == typeof(TCPSender))
        {
            TCPSender sender = channelToTest.channelObj as TCPSender;
            sender.Send("TEST");
        }
        else if(typeof(T) == typeof(TCPReceiver))
        {
            TCPReceiver sender = channelToTest.channelObj as TCPReceiver;
        }
    }
}