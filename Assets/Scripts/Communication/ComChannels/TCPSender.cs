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
        
        public bool channelIsOpen
        {
            get {
                return senderClient.Connected;
            }
        }

        private TcpClient senderClient;
        private NetworkStream stream;
        //=========
        public TCPSender(string _ip, int _port)
        {
            IP = _ip;
            port = _port;
            if(UsefulFunctions.IP_AddressIsValid(IP) && port > 0)
                OpenConnection();
        }

        public void OpenConnection()
        {
            try
            {
                senderClient = new TcpClient();
                senderClient.Connect(new IPEndPoint(IPAddress.Parse(IP), port));
                stream = senderClient.GetStream();
                Debug.Log(senderClient.Connected);
            }
            catch(SocketException e)
            {
                Debug.Log("SocketException: " + e);
            }
        }
        //=====================
        //=====================
        public void Send(byte[] val)
        {
            if(!channelIsOpen) {
                Debug.LogWarning("Channel of type UDPSender at IP:" + IP + " and port: " + port + " is closed. The 'Send' method can not be used while the channel is closed.");
                return;
            }
            try
            {
                stream.Write(val, 0, val.Length);
            }
            catch(SocketException e)
            {
                Debug.Log("SocketException: " + e);
            }
        }

        public void Send(string val)
        {
            byte[] data = Encoding.ASCII.GetBytes(val);
            Send(data);
        }

        public void Send(string[] val)
        {
            int valCharLength = 0;
            foreach(string stringItem in val)
                valCharLength += stringItem.Length;

            byte[] blockData = new byte[valCharLength * val.Length * sizeof(char)];
            Buffer.BlockCopy(val, 0, blockData, 0, blockData.Length);
            Send(blockData);
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
        //======================
        //======================
        /// <summary>
        /// Close the TCP client connection, ensuring there will be no issue when restarting the app 
        /// </summary>
        public void Terminate()
        {
            try
            {
                senderClient.Close();
            }
            catch (Exception err)
            {
                Debug.Log("<color=red>" + err.Message + "</color>");
            }
        }
    }
}