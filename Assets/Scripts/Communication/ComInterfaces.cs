using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace Matlab_Communication
{
    public interface SenderReceiverBaseChannel
    {
        string IP {get; set;}
        int port {get; set;}
    }

    [Serializable]
    public class OnDataReceivedEvent : UnityEvent<byte[]> {}
}