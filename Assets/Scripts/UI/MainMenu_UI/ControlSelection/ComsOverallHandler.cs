using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ComOps = CommonMethods.CommunicationOps;
using Communication;
using System;

public class ComsOverallHandler : MonoBehaviour
{
    public const int maxNBConnections=8; // Max connections that can be created
    public List<ComChannelInterface> channels;
    public List<bool> channels_Cor_AreRunning; // List of bool to indicate if the channels, with the same mapping as 'channels', have their coroutine running

    [HideInInspector] public MainPanelIsSetUp panelIsFullySetUp;

    void Awake()
    {
        DontDestroyOnLoad(this);

        if(panelIsFullySetUp == null)
            panelIsFullySetUp = new MainPanelIsSetUp();

        channels = new List<ComChannelInterface>();
        channels_Cor_AreRunning = new List<bool>();
    }
    // UI METHOD TO ADD A NEW CONNECTION TO THE 'channels' List<ComChannelInterface>
    public int Add_ComObject(ComChannelParams comChannelParams)
    {
        // Method that creates a new com objects with the speciied ComChannelParams and returns the int index of the object in the 'channels' List<ComChannel>
        switch(comChannelParams.protocol)
        {
            case ComProtocol.UDP_Sender:
                channels.Add(new ComChannel<UDPSender>(comChannelParams));
                break;
            case ComProtocol.UDP_Receiver:
                channels.Add(new ComChannel<UDPReceiver>(comChannelParams));
                break;
            case ComProtocol.TCPIP_Sender: case ComProtocol.TCPIP_Receiver:
                channels.Add(new ComChannel<TCPServer>(comChannelParams));
                break;
        }
        // By default, when creating an object, we are not activating its corresponding coroutine to send/receive data
        channels_Cor_AreRunning.Add(false);
        return channels.Count-1;
    }
    public void Remove_ComObject_At_Index(int removedIndex)
    {
        // Removing comObjects and its corresponding bool in every List of the ComsOverallHandler
        channels.RemoveAt(removedIndex);
        channels_Cor_AreRunning.RemoveAt(removedIndex);
    }


    IEnumerator SimEnv_Coroutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);
        }
    }

    public void StartChannel(int channelIdx)
    {
        if(channelIdx < channels.Count && channels[channelIdx] != null)
        {
            // Authorized to start the channel
            channels_Cor_AreRunning[channelIdx] = true; // Setting to true
            // StartCoroutine(...);
        }
    }
}