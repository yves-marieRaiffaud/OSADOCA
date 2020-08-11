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
        public OnDataReceivedEvent onDataReceivedEvent;
        public bool isListening {get; private set;}
        //==================
        //==================
        public UDPReceiver(string _ip, int _port)
        {
            IP = _ip;
            port = _port;
            isListening = false;
            _ReceiveThread = new Thread(new ThreadStart(ReceiveData));
            _ReceiveThread.IsBackground = true;
            _ReceiveThread.Start();
            if(onDataReceivedEvent == null)
                onDataReceivedEvent = new OnDataReceivedEvent();
        }
        //===================
        //===================
        private void ReceiveData()
        {
            _ReceiveClient = new UdpClient(port);
            isListening = true;
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
                    if(onDataReceivedEvent != null)
                        onDataReceivedEvent.Invoke(data);
                }
                catch (Exception err)
                {
                    isListening = false;
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
                isListening = false;
            }
            catch (Exception err)
            {
                Debug.Log("<color=red>" + err.Message + "</color>");
            }
        }
    }
}