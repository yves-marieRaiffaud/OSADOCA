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
    public class TCPSender : SenderReceiverBaseChannel
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
        
        private Thread _serverThread;
        private TcpClient client;
        private NetworkStream stream;
        private bool connectionIsOpen;

        public TCPSender(string _ip, int _port)
        {
            IP = _ip;
            port = _port;
            connectionIsOpen = false;
            if(UsefulFunctions.IP_AddressIsValid(IP) && port > 0)
                OpenConnection();
        }

        public void OpenConnection()
        {
            try
            {
                client = new TcpClient(new IPEndPoint(IPAddress.Parse(IP), port));
                stream = client.GetStream();
                connectionIsOpen = true;
            }
            catch(SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }
        //=====================
        //=====================
        public void Send(byte[] val)
        {
            try
            {
                if(connectionIsOpen)
                    stream.Write(val, 0, val.Length);
            }
            catch(SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        public void Send(string val)
        {
            byte[] data = Encoding.ASCII.GetBytes(val);
            Send(data);
        }

        public void Send(string[] val)
        {
            foreach(string stringItem in val)
            {
                byte[] data = Encoding.ASCII.GetBytes(stringItem);
                Send(data);
            }
        }

        public void Send(double val)
        {
            byte[] data = BitConverter.GetBytes(val);
            Send(data);
        }

        public void Send(double[] val)
        {
            foreach(double doubleItem in val)
            {
                Send(doubleItem);
            }
        }
        //======================
        //======================
        /// <summary>
        /// Close the TCP client connection, ensuring there will be no issue when restarting the app 
        /// </summary>
        private void Terminate()
        {
            try
            {
                client.Close();
            }
            catch (Exception err)
            {
                Debug.Log("<color=red>" + err.Message + "</color>");
            }
        }
    }
}