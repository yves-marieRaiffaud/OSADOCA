using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommunicationController : MonoBehaviour, IReceiverObserver
{
    private UDPReceiver _UdpReceiver;
    private TCPReceiver _TcpReceiver;
    private UDPTransmitter _UdpTransmitter;
    public bool receiveUsingUDP;
    //===============================
    private void Awake()
    {
        if(receiveUsingUDP)
        {
            _UdpReceiver = GetComponent<UDPReceiver>();
            _UdpReceiver.SetObserver(this);
            _TcpReceiver = null;
        }
        else {
            _TcpReceiver = GetComponent<TCPReceiver>();
            _TcpReceiver.SetObserver(this);
            _UdpReceiver = null;
        }

        _UdpTransmitter = GetComponent<UDPTransmitter>();
    }

    /// <summary>
    /// Send data immediately after receiving it.
    /// </summary>
    /// <param name="val"></param>
    void IReceiverObserver.OnDataReceived(double[] val)
    {
        _UdpTransmitter.Send(val);
    }
}