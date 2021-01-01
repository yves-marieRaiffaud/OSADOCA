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
    public enum ComProtocol { UDP_Sender, UDP_Receiver, TCPIP_Sender, TCPIP_Receiver }
    [Flags] public enum ComConectionType { dataVisualization=1, simulationEnv=2, shipControlOrders=4 }
    public enum ComSendReceiveType { sendOnly, receiveOnly, sendReceive, classExplicit }

    public class ComChannel
    {
        public static readonly Dictionary<ComProtocol, ComConectionType> comEnumsCombinations = new Dictionary<ComProtocol, ComConectionType> {
            { ComProtocol.UDP_Sender    , ComConectionType.dataVisualization|ComConectionType.simulationEnv },
            { ComProtocol.UDP_Receiver  , ComConectionType.shipControlOrders },
            { ComProtocol.TCPIP_Sender  , ComConectionType.dataVisualization|ComConectionType.simulationEnv },
            { ComProtocol.TCPIP_Receiver, ComConectionType.shipControlOrders }
        };
    }

    public class ComChannel<T>
    {
        //====================
        public ComProtocol protocol;
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
            protocol = parameters.protocol;
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
                channelObj = (T)Convert.ChangeType(new TCPServer(port, sendReceiveType, IP), typeof(T));
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
        public ComProtocol protocol;
        public ComConectionType connectionType;
        public ComSendReceiveType sendReceiveType;
        //========
        public ComChannelParams(string _IP, int _port, ComProtocol _protocol, ComConectionType _coType, ComSendReceiveType _sendReceiveType, int _defaultPort, string _defaultIP="127.0.0.1")
        {
            defaultIP = _defaultIP;
            IP = _IP;
            defaultPort = _defaultPort;
            port = _port;
            protocol = _protocol;
            connectionType = _coType;
            sendReceiveType = _sendReceiveType;
        }
    }
    //=================================
    //=================================
}