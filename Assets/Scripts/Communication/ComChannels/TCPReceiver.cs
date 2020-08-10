using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Matlab_Communication
{
    public class TCPReceiver : SenderReceiverBaseChannel
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
        
        private TcpListener server;
        private Thread _serverThread;
        private TcpClient client;
        //=====
        //private IReceiverObserver _Observer;
        public OnDataReceivedEvent onDataReceivedEvent;

        public TCPReceiver(string _ip, int _port)
        {
            IP = _ip;
            port = _port;
            _serverThread = new Thread(new ThreadStart(ReceiveData));
            _serverThread.IsBackground = true;
            _serverThread.Start();
            onDataReceivedEvent = new OnDataReceivedEvent();
        }

        /*public void SetObserver(IReceiverObserver observer)
        {
            _Observer = observer;
        }*/

        /// <summary>
        /// Receive data with pooling.
        /// </summary>
        private void ReceiveData()
        {
            try
            {
                server = new TcpListener(IPAddress.Parse(IP), port);
                server.Start();
                //======
                Byte[] bytes = new Byte[256];
                while(true)
                {
                    client = server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    int i;
                    while((i = stream.Read(bytes, 0, bytes.Length))!=0)
                    {
                        //double[] values = new double[bytes.Length / 8];
                        //Buffer.BlockCopy(bytes, 0, values, 0, values.Length * 8);
                        //==========
                        //if (_Observer != null)
                            //_Observer.OnDataReceived(values);
                        if(onDataReceivedEvent != null)
                            onDataReceivedEvent.Invoke(bytes);
                    }
                }
            }
            catch(SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        /// <summary>
        /// Close the UDP client connection, ensuring there will be no issue when restarting the app 
        /// </summary>
        private void Terminate()
        {
            try
            {
                _serverThread.Abort();
                _serverThread = null;
                client.Close();
                server.Stop();
            }
            catch (Exception err)
            {
                Debug.Log("<color=red>" + err.Message + "</color>");
            }
        }
    }
}