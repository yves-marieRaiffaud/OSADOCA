using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using ComOps = CommonMethods.CommunicationOps;
using System;

namespace Communication
{
    public enum ComProtocol { UDP_Sender, UDP_Receiver, TCPIP_Sender, TCPIP_Receiver }
    [Flags] public enum ComConectionType { None=0, dataVisualization=1, simulationEnv=2, shipControlOrders=4 }
    public enum ComSendReceiveType { sendOnly, receiveOnly, sendReceive, classExplicit }

    // Enum containing both the 'incomingDataFields' & the 'outgoingDataFields'
    [Flags] public enum ComDataFieldsOut { None=0, shipAcc=1, shipVel=2, shipVelIncr=4, shipPos=8, shipDeltaRot=16 };
    [Flags] public enum ComDataFieldsIn { None=0, propulsionIsOn=1 };

    public class ComChannelDict
    {
        public static readonly Dictionary<ComProtocol, ComConectionType> comEnumsCombinations = new Dictionary<ComProtocol, ComConectionType> {
            { ComProtocol.UDP_Sender    , ComConectionType.dataVisualization|ComConectionType.simulationEnv },
            { ComProtocol.UDP_Receiver  , ComConectionType.shipControlOrders },
            { ComProtocol.TCPIP_Sender  , ComConectionType.dataVisualization|ComConectionType.simulationEnv },
            { ComProtocol.TCPIP_Receiver, ComConectionType.shipControlOrders }
        };
    }

    public class ComChannel<T> : ComChannelInterface where T: SenderReceiverBaseChannel
    {
        ComChannelParams _comParams;
        public ComChannelParams comParams
        {
            get {
                return _comParams;
            }
            set {
                _comParams=value;
            }
        }

        private T _channelObj; // T is either the 'UDPReceiver', 'UDPSender' or 'TCPServer' object
        public T channelObj
        {
            get {
                return _channelObj;
            }
            private set {
                _channelObj=value;
            }
        }
        public SenderReceiverBaseChannel channelObj_generic
        {
            get {
                return (SenderReceiverBaseChannel) _channelObj;
            }
        }

        public string IP
        {
            get {
                return comParams.IP;
            }
            set {
                _comParams.IP=value;
            }
        }
        public int port
        {
            get {
                return comParams.port;
            }
            set {
                _comParams.port=value;
            }
        }
        public string defaultIP
        {
            get {
                return comParams.defaultIP;
            }
            set {
                _comParams.defaultIP=value;
            }
        }
        public int defaultPort
        {
            get {
                return comParams.defaultPort;
            }
            set {
                _comParams.defaultPort=value;
            }
        }

        public ComChannel(ComChannelParams parameters)
        {
            comParams = parameters;
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
                channelObj = (T)Convert.ChangeType(new TCPServer(port, comParams.sendReceiveType, IP), typeof(T));
        }
    }
    //=================================
    [Serializable]
    public struct ComChannelParams
    {
        public string defaultIP;
        public string IP;

        public int defaultPort;
        public int port;

        public ComProtocol protocol;
        public ComConectionType connectionType;
        public ComSendReceiveType sendReceiveType;
        public Enum dataFields;

        public ComChannelParams(string _IP, int _port, ComProtocol _protocol, ComConectionType _coType, Enum _dataFields, ComSendReceiveType _sendReceiveType, int _defaultPort, string _defaultIP="127.0.0.1")
        {
            defaultIP = _defaultIP;
            IP = _IP;
            defaultPort = _defaultPort;
            port = _port;
            protocol = _protocol;
            connectionType = _coType;
            dataFields = _dataFields;
            sendReceiveType = _sendReceiveType;
        }
    }
}