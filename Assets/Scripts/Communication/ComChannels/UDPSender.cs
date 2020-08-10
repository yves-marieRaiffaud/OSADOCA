using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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

        private IPEndPoint _RemoteEndPoint;
        private UdpClient _TransmitClient;

        /// <summary>
        /// Create a new UDPTransmitter object
        /// </summary>
        public UDPSender(string _ip, int _port)
        {
            IP = _ip;
            port = _port;
            if(IP.Equals("")) { return; }
            _RemoteEndPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);
            _TransmitClient = new UdpClient();
        }

        /// <summary>
        /// Sends a double value to target port and ip.
        /// </summary>
        /// <param name="val"></param>
        public void Send(double val)
        {
            try
            {
                // Convert string message to byte array.  
                byte[] serverMessageAsByteArray = BitConverter.GetBytes(val);

                _TransmitClient.Send(serverMessageAsByteArray, serverMessageAsByteArray.Length, _RemoteEndPoint);
            }
            catch (Exception err)
            {
                Debug.Log("<color=red>" + err.Message + "</color>");
            }
        }

        /// <summary>
        /// Sends a double array to target port and ip.
        /// </summary>
        /// <param name="val"></param>
        public void Send(double[] val)
        {
            try
            {
                for (int i = 0; i < val.Length; i++)
                    Send(val[i]);
            }
            catch (Exception err)
            {
                Debug.Log("<color=red>" + err.Message + "</color>");
            }
        }

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