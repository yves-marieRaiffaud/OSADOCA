using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace Matlab_Communication
{
    public class TCPServer : SenderReceiverBaseChannel
    {
        // Defines the IPs that the server can accept
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
        
        public bool serverIsActive
        {
            get {
                return server.Server.Connected;
            }
        }

        public TcpListener server;
        //=========
        public TCPServer(int _port)
        {
            IP = null;
            port = _port;
            StartServer();
        }

        public void StartServer()
        {
            try
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
            }
            catch(SocketException e)
            {
                Debug.Log("SocketException: " + e);
            }
        }

        public void StopServer()
        {
            try
            {
                server.Stop();
            }
            catch(SocketException e)
            {
                Debug.Log("SocketException: " + e);
            }
        }
    }
}