using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace Communication
{
    [Serializable]
    public class TCPServer : SenderReceiverBaseChannel
    {
        // Defines the IPs that the server can accept
        [SerializeField] private string _IP;
        public string IP
        {
            get {
                return _IP;
            }
            set {
                _IP=value;
            }
        }
        [SerializeField] private int _port;
        public int port
        {
            get {
                return _port;
            }
            set {
                _port=value;
            }
        }
        
        bool _isActive;
        public bool isActive
        {
            get {
                return _isActive;
            }
            set {
                _isActive=value;
            }
        }

        public bool serverIsConnected
        {
            get {
                return server.Server.Connected;
            }
        }

        ComSendReceiveType sendReceiveType;
        //=========
        public TcpListener server;
        public TcpClient tcpClient;
        private NetworkStream matlabStream;
        private Thread serverThread;
        //=========
        public string lastReceivedData;
        public OnDataReceivedEvent onDataReceivedEvent;
        //=========
        public TCPServer(int _port, ComSendReceiveType _sendReceiveType=ComSendReceiveType.sendReceive, string _IP="127.0.0.1")
        {
            sendReceiveType = _sendReceiveType;
            IP = _IP; // y default set to localhost
            port = _port;
            if(onDataReceivedEvent == null)
                onDataReceivedEvent = new OnDataReceivedEvent();
            StartServerThread();
        }

        public void StartServerThread()
        {
            serverThread = new Thread(new ThreadStart(StartServer));
            serverThread.IsBackground = true;
            serverThread.Start();
        }
        private void StartServer()
        {
            try {
                server = new TcpListener(IPAddress.Parse(IP), port);
                Debug.Log("Started Server listening IP: " + IP);
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while(true) {
                    if(_isActive)
                    {
                        Debug.Log("Waiting for a connection on port " + port + "...");
                        // Perform a blocking call to accept requests.
                        // You could also use server.AcceptSocket() here.
                        tcpClient = server.AcceptTcpClient();
                        Debug.Log("Connected!");
                        data = null;
                        //--------------------
                        // Get a stream object for reading and writing
                        matlabStream = tcpClient.GetStream();
                        //--------------------
                        if(sendReceiveType == ComSendReceiveType.sendOnly)
                            continue;
                        int i;
                        // Loop to receive all the data sent by the client.
                        while((i = matlabStream.Read(bytes, 0, bytes.Length))!=0) {
                            // Translate data bytes to a ASCII string.
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                            lastReceivedData = data;

                            if(onDataReceivedEvent != null)
                                onDataReceivedEvent.Invoke(data);
                        }
                        tcpClient.Close();
                    }
                }
            }
            catch(SocketException e) {
                Debug.Log("SocketException: " + e);
            }
            finally {
                server.Stop();
            }
        }
        //=============
        //=============
        public void RestartServerWithNewConfig()
        {
            StopServer();
            StartServerThread();
        } 
        //=============
        //=============
        public void Send(byte[] val)
        {
            if(_isActive)
            {
                if(sendReceiveType == ComSendReceiveType.receiveOnly)
                    return;
                try {
                    matlabStream.Write(val, 0, val.Length);
                }
                catch (Exception err) {
                    Debug.Log("<color=red>" + err.Message + "</color>");
                }
            }
        }
        public void Send(double val)
        {
            byte[] data = BitConverter.GetBytes(val);
            Send(data);
        }
        public void Send(double[] val)
        {
            byte[] blockData = new byte[val.Length * sizeof(double)];
            Buffer.BlockCopy(val, 0, blockData, 0, blockData.Length);
            Send(blockData);
        }
        public void Send(string val)
        {
            byte[] data = Encoding.ASCII.GetBytes(val);
            Send(data);
        }
        public void Send(string[] val)
        {
            List<byte> byteList = new List<byte>();
            byte[] spaceASCIIBytes = Encoding.ASCII.GetBytes(" ");

            foreach(string stringItem in val) {
                foreach(byte byteItem in Encoding.ASCII.GetBytes(stringItem))
                    byteList.Add(byteItem);
                // Add a space ' ' between each sent string
                foreach(byte byteItem in spaceASCIIBytes)
                    byteList.Add(byteItem);
            }

            byte[] blockData = byteList.ToArray();
            //Buffer.BlockCopy(val, 0, blockData, 0, blockData.Length);
            Send(blockData);
        }
        //============
        //=============
        public void StopServer()
        {
            try {
                server.Stop();
                serverThread.Abort();
                serverThread = null;
            }
            catch(SocketException e) {
                Debug.Log("SocketException: " + e);
            }
        }
    }
}