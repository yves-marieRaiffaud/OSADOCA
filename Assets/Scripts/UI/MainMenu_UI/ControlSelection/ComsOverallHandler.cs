using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using ComOps = CommonMethods.CommunicationOps;
using Communication;
using System;

public class ComsOverallHandler : MonoBehaviour
{
    const int maxNBConnections=4; // Max connections that can be created
    public List<ComChannel> channels;
    [HideInInspector] public MainPanelIsSetUp panelIsFullySetUp;

    void Awake()
    {
        DontDestroyOnLoad(this);

        if(panelIsFullySetUp == null)
            panelIsFullySetUp = new MainPanelIsSetUp();

        channels = new List<ComChannel>();
    }

    public int Add_ComObject(ComChannelParams comChannelParams)
    {
        // Method that creates a new com objects with the speciied ComChannelParams and returns the int index of the object in the 'channels' List<ComChannel>
        /*ComProtocol protocol= ComProtocol.UDP_Sender;
        ComConectionType coType = ComConectionType.dataVisualization;
        ComSendReceiveType srType = ComSendReceiveType.classExplicit;
        ComChannelParams dataParams = new ComChannelParams("169.254.183.22", 25010, protocol, coType, srType, 25010);*/
        switch(comChannelParams.protocol)
        {
            case ComProtocol.UDP_Sender:
                channels.Add(new ComChannel<UDPSender>(comChannelParams));
                break;
            case ComProtocol.UDP_Receiver:
                channels.Add(new ComChannel<UDPReceiver>(comChannelParams));
                break;
            case ComProtocol.TCPIP_Sender | ComProtocol.TCPIP_Receiver:
                channels.Add(new ComChannel<TCPServer>(comChannelParams));
                break;
        }
        return channels.Count;
    }
}