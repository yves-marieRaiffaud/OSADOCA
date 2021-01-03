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
    [Flags] public enum ComConectionType { None=0, dataVisualization=1, simulationEnv=2, shipControlOrders=4 }
    public enum ComSendReceiveType { sendOnly, receiveOnly, sendReceive, classExplicit }
    [Flags] public enum ComDataFields { None=0, shipAcc=1, shipVel=2, shipVelIncr=4, shipPos=8, shipDeltaRot=16 };

    public class ComChannelDict
    {
        public static readonly Dictionary<ComProtocol, ComConectionType> comEnumsCombinations = new Dictionary<ComProtocol, ComConectionType> {
            { ComProtocol.UDP_Sender    , ComConectionType.dataVisualization|ComConectionType.simulationEnv },
            { ComProtocol.UDP_Receiver  , ComConectionType.shipControlOrders },
            { ComProtocol.TCPIP_Sender  , ComConectionType.dataVisualization|ComConectionType.simulationEnv },
            { ComProtocol.TCPIP_Receiver, ComConectionType.shipControlOrders }
        };
    }

    public abstract class ComChannel {}

    public class ComChannel<T> : ComChannel where T: SenderReceiverBaseChannel
    {
        public ComChannelParams comParams;

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

        public string IP
        {
            get {
                return comParams.IP;
            }
            set {
                comParams.IP=value;
            }
        }
        public int port
        {
            get {
                return comParams.port;
            }
            set {
                comParams.port=value;
            }
        }
        public string defaultIP
        {
            get {
                return comParams.defaultIP;
            }
            set {
                comParams.defaultIP=value;
            }
        }
        public int defaultPort
        {
            get {
                return comParams.defaultPort;
            }
            set {
                comParams.defaultPort=value;
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
        public ComDataFields dataFields;

        public ComChannelParams(string _IP, int _port, ComProtocol _protocol, ComConectionType _coType, ComDataFields _dataFields, ComSendReceiveType _sendReceiveType, int _defaultPort, string _defaultIP="127.0.0.1")
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