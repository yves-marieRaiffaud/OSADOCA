using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using Fncs = UsefulFunctions;
using System.Net;
using System.Net.Sockets;
using System;

namespace Matlab_Communication
{
    public enum MatlabConectionType { dataVisualization, simulationEnv, shipControlOrders };
    public enum MatlabSendReceiveType { sendOnly, receiveOnly, sendReceive, classExplicit };

    public class MatlabComChannel<T>
    {
        //====================
        public MatlabConectionType connectionType;
        public MatlabSendReceiveType sendReceiveType;
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
        // Only for TCP/IP server
        private TcpListener serverToConnectTo;
        //====================
        //====================
        public MatlabComChannel(ComChannelParams parameters)
        {
            IP = parameters.IP;
            port = parameters.port;
            defaultIP = parameters.defaultIP;
            defaultPort = parameters.defaultPort;
            connectionType = parameters.connectionType;
            sendReceiveType = parameters.sendReceiveType;
            //======
            serverToConnectTo = null;
            //======
            if((Fncs.IP_AddressIsValid(IP) && port > -1) || typeof(T) == typeof(TCPServer))
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
                channelObj = (T)Convert.ChangeType(new TCPServer(port, sendReceiveType), typeof(T));
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
        public MatlabConectionType connectionType;
        public MatlabSendReceiveType sendReceiveType;
        //========
        public ComChannelParams(string _IP, int _port, MatlabConectionType _coType, MatlabSendReceiveType _sendReceiveType, int _defaultPort, string _defaultIP="127.0.0.1")
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