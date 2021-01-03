using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using ComOps = CommonMethods.CommunicationOps;
using Communication;
using System;

public class ComsOverallHandler : MonoBehaviour
{
    public const int maxNBConnections=8; // Max connections that can be created
    public List<ComChannelInterface> channels;
    [HideInInspector] public MainPanelIsSetUp panelIsFullySetUp;

    void Awake()
    {
        DontDestroyOnLoad(this);

        if(panelIsFullySetUp == null)
            panelIsFullySetUp = new MainPanelIsSetUp();

        channels = new List<ComChannelInterface>();
    }

    // TO FIX: wrong int index
    public int Add_ComObject(ComChannelParams comChannelParams)
    {
        // Method that creates a new com objects with the speciied ComChannelParams and returns the int index of the object in the 'channels' List<ComChannel>
        switch(comChannelParams.protocol)
        {
            case ComProtocol.UDP_Sender:
                channels.Add(new ComChannel<UDPSender>(comChannelParams));
                Debug.Log("UDP_Sender added = " + (channels.Count-1));
                break;
            case ComProtocol.UDP_Receiver:
                channels.Add(new ComChannel<UDPReceiver>(comChannelParams));
                Debug.Log("UDP_Receiver added = " + (channels.Count-1));
                break;
            case ComProtocol.TCPIP_Sender | ComProtocol.TCPIP_Receiver:
                channels.Add(new ComChannel<TCPServer>(comChannelParams));
                Debug.Log("TCPServer added = " + (channels.Count-1));
                break;
        }
        return channels.Count-1;
    }
}