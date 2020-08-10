using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace Matlab_Communication
{
    public class UDPSender : SenderReceiverBaseChannel
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
        //========
        private IPEndPoint _RemoteEndPoint;
        private UdpClient _TransmitClient;
        //=======================
        //=======================
        public UDPSender(string _ip, int _port)
        {
            IP = _ip;
            port = _port;
            _RemoteEndPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);
            _TransmitClient = new UdpClient();
        }
        //=======================
        //=======================
        public void Send(byte[] val)
        {
            try
            {
                _TransmitClient.Send(val, val.Length, _RemoteEndPoint);
            }
            catch (Exception err)
            {
                Debug.Log("<color=red>" + err.Message + "</color>");
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

        public void Send(string val)
        {
            byte[] data = Encoding.ASCII.GetBytes(val);
            Send(data);
        }

        public void Send(string[] val)
        {
            foreach(string stringVal in val)
            {
                Send(stringVal);
            }
        }
        //=======================
        //=======================
        /// <summary>
        /// Close the UDP client connection, ensuring there will be no issue when restarting the app 
        /// </summary>
        public void Terminate()
        {
            try
            {
                _TransmitClient.Close();
            }
            catch (Exception err)
            {
                Debug.Log("<color=red>" + err.Message + "</color>");
            }
        }
    }
}