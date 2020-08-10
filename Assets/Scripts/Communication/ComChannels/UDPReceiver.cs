using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Matlab_Communication
{
    public class UDPReceiver : SenderReceiverBaseChannel
    {
        private string _IP;
        public string IP
        {
            get {
                return _IP;
            }
            set {
                _IP=value;
            }
        }
        private int _port;
        public int port
        {
            get {
                return _port;
            }
            set {
                _port=value;
            }
        }

        private UdpClient _ReceiveClient;
        private Thread _ReceiveThread;
        //=======
        //private IReceiverObserver _Observer;
        public OnDataReceivedEvent onDataReceivedEvent;
        //==================
        //==================
        public UDPReceiver(string _ip, int _port)
        {
            IP = _ip;
            port = _port;
            _ReceiveThread = new Thread(new ThreadStart(ReceiveData));
            _ReceiveThread.IsBackground = true;
            _ReceiveThread.Start();
            if(onDataReceivedEvent == null)
                onDataReceivedEvent = new OnDataReceivedEvent();
        }
        //===================
        //===================
        /*public void SetObserver(IReceiverObserver observer)
        {
            _Observer = observer;
        }*/

        private void ReceiveData()
        {
            _ReceiveClient = new UdpClient(port);
            while (true)
            {
                try
                {
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse(IP), 0);
                    byte[] data = _ReceiveClient.Receive(ref anyIP);
                    //======
                    //double[] values = new double[data.Length / 8];
                    //Buffer.BlockCopy(data, 0, values, 0, values.Length * 8);
                    //======
                    //if (_Observer != null)
                        //_Observer.OnDataReceived(values);
                    if(onDataReceivedEvent != null)
                        onDataReceivedEvent.Invoke(data);
                }
                catch (Exception err)
                {
                    Debug.Log("<color=red>" + err.Message + "</color>");
                }
            }
        }
        //===================
        //===================
        /// <summary>
        /// Close the UDP connection, ensuring there will be no issue when restarting the app 
        /// </summary>
        public void Terminate()
        {
            try
            {
                _ReceiveThread.Abort();
                _ReceiveThread = null;
                _ReceiveClient.Close();
            }
            catch (Exception err)
            {
                Debug.Log("<color=red>" + err.Message + "</color>");
            }
        }
    }
}