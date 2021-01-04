using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace Communication
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
        //========
        private IPEndPoint _RemoteEndPoint;
        private UdpClient _TransmitClient;
        public bool channelIsOpen
        {
            get {
                return _TransmitClient.Client.Connected;
            }
        }
        //=======================
        //=======================
        public UDPSender(string _ip, int _port)
        {
            IP = _ip;
            port = _port;
            _RemoteEndPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);
            _TransmitClient = new UdpClient();
            _TransmitClient.Connect(_RemoteEndPoint);
            Debug.Log("UDP is connected: " + _TransmitClient.Client.Connected + " at IP: " + IP + " and port: " + port);
        }
        //=======================
        //=======================
        public void Send(byte[] val)
        {
            if(_isActive)
            {
                if(!channelIsOpen) {
                    Debug.LogWarning("Channel of type UDPSender at IP:" + IP + " and port: " + port + " is closed. The 'Send' method can not be used while the channel is closed.");
                    return;
                }
                try {
                    _TransmitClient.Send(val, val.Length);
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

            foreach(string stringItem in val)
            {
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
        //=======================
        //=======================
        /// <summary>
        /// Close the UDP client connection, ensuring there will be no issue when restarting the app 
        /// </summary>
        public void Terminate()
        {
            try {
                _TransmitClient.Close();
            }
            catch (Exception err) {
                Debug.Log("<color=red>" + err.Message + "</color>");
            }
        }
    }
}