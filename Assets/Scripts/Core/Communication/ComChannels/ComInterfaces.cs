using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace Communication
{
    public interface SenderReceiverBaseChannel
    {
        string IP {get; set;}
        int port {get; set;}
        bool isActive {get; set;}
        void Send(string val);
    }

    public interface ComChannelInterface
    {
        SenderReceiverBaseChannel channelObj_generic {get;}
        ComChannelParams comParams {get; set;}
        string IP {get; set;}
        int port {get; set;}
        string defaultIP {get; set;}
        int defaultPort {get; set;}

        void InitChannelObj();
    }

    [Serializable]
    public class OnDataReceivedEvent : UnityEvent<string> {}
}