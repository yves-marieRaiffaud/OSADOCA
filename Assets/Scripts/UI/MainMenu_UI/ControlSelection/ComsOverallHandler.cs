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
    public MainPanelIsSetUp panelIsFullySetUp;

    void Awake()
    {
        if(panelIsFullySetUp == null)
            panelIsFullySetUp = new MainPanelIsSetUp();

        channels = new List<ComChannel>();
    }
}