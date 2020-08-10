using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Matlab_Communication
{
    public class MatlabComHandler : MonoBehaviour, IReceiverObserver
    {
        public MatlabComChannel<UDPSender> dataVisSenderChannel;
        public MatlabComChannel<TCPSender> simEnvSenderChannel;
        public MatlabComChannel<TCPReceiver> controlAlgoReceiverChannel;
        //================
        /*private UDPReceiver _UdpReceiver;
        private TCPReceiver _TcpReceiver;
        private UDPSender _UdpTransmitter;
        public bool receiveUsingUDP;*/
        //===============================
        void Awake()
        {
            DontDestroyOnLoad(this);

            dataVisSenderChannel = new MatlabComChannel<UDPSender>(MatlabConectionType.dataVisualization);
            simEnvSenderChannel = new MatlabComChannel<TCPSender>(MatlabConectionType.simulationEnv);
            controlAlgoReceiverChannel = new MatlabComChannel<TCPReceiver>(MatlabConectionType.shipControlOrders);
            /*if(receiveUsingUDP)
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

            _UdpTransmitter = GetComponent<UDPTransmitter>();*/
        }

        /// <summary>
        /// Send data immediately after receiving it.
        /// </summary>
        /// <param name="val"></param>
        void IReceiverObserver.OnDataReceived(double[] val)
        {
            //_UdpTransmitter.Send(val);
        }
    }
}