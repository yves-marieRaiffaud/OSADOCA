using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using ComOps = CommonMethods.CommunicationOps;
using System.Net;
using System.Net.Sockets;
using System;

namespace Communication
{
    public enum ComConectionType { dataVisualization, simulationEnv, shipControlOrders };
    public enum ComSendReceiveType { sendOnly, receiveOnly, sendReceive, classExplicit };

    public class ComChannel<T>
    {
        //====================
        public ComConectionType connectionType;
        public ComSendReceiveType sendReceiveType;
        //====================
        //====================
        // T is either the 'UDPReceiver', 'UDPSender' or 'TCPServer' object
        private T _channelObj;
        public T channelObj
        {
            get {
                return _channelObj;
            }
            private set {
                _channelObj=value;
            }
        }
        //====
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
        //====
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
        //====
        private string _defaultIP;
        public string defaultIP
        {
            get {
                return _defaultIP;
            }
            set {
                _defaultIP=value;
            }
        }
        //====
        private int _defaultPort;
        public int defaultPort
        {
            get {
                return _defaultPort;
            }
            set {
                _defaultPort=value;
            }
        }
        //====================
        //====================
        public ComChannel(ComChannelParams parameters)
        {
            IP = parameters.IP;
            port = parameters.port;
            defaultIP = parameters.defaultIP;
            defaultPort = parameters.defaultPort;
            connectionType = parameters.connectionType;
            sendReceiveType = parameters.sendReceiveType;
            //======
            if((ComOps.IP_AddressIsValid(IP) && port > -1) || typeof(T) == typeof(TCPServer))
                InitChannelObj();
        }

        public void InitChannelObj()
        {
            if(typeof(T) == typeof(UDPSender))
                channelObj = (T)Convert.ChangeType(new UDPSender(IP, port), typeof(T));

            else if(typeof(T) == typeof(UDPReceiver))
                channelObj = (T)Convert.ChangeType(new UDPReceiver(IP, port), typeof(T));

            else if(typeof(T) == typeof(TCPServer))
            {
                channelObj = (T)Convert.ChangeType(new TCPServer(port, sendReceiveType, IP), typeof(T));
            }
                
        }
    }
    //=================================
    //=================================
    [Serializable]
    public struct ComChannelParams
    {
        public string defaultIP;
        public string IP;
        //========
        public int defaultPort;
        public int port;
        //========
        public ComConectionType connectionType;
        public ComSendReceiveType sendReceiveType;
        //========
        public ComChannelParams(string _IP, int _port, ComConectionType _coType, ComSendReceiveType _sendReceiveType, int _defaultPort, string _defaultIP="127.0.0.1")
        {
            this.defaultIP = _defaultIP;
            this.IP = _IP;
            this.defaultPort = _defaultPort;
            this.port = _port;
            connectionType = _coType;
            sendReceiveType = _sendReceiveType;
        }
    }
    //=================================
    //=================================
}