using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Matlab_Communication
{
    public interface SenderReceiverBaseChannel
    {
        string IP {get; set;}
        int port {get; set;}
    }

    public interface IReceiverObserver
    {
        void OnDataReceived(double[] val);
    }
}