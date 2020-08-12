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
        public MatlabConectionType connectionType;
        public MatlabSendReceiveType sendReceiveType;

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

        public string IP;
        public int port;

        public string defaultIP;
        public int defaultPort;

        // Only for TCP/IP server
        private TcpListener serverToConnectTo;

        public MatlabComChannel(MatlabConectionType _coType, MatlabSendReceiveType _sendReceiveType=MatlabSendReceiveType.classExplicit,
                                int _port=-1, int _defaultPort=25010, string _ip="", string _defaultIP="127.0.0.1")
        {
            connectionType = _coType;
            sendReceiveType = _sendReceiveType;
            IP = _ip;
            port = _port;
            defaultIP = _defaultIP;
            defaultPort = _defaultPort;
            serverToConnectTo = null;

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
}